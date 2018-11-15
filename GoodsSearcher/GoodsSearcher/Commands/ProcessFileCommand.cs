using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GoodsSearcher.ViewModels;
using Sraper.Common;
using Flurl;
using Flurl.Http;
using GoodsSearcher.Common.Helpers;
using Sraper.Common.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.IO;
using System.Windows.Threading;

namespace GoodsSearcher.Commands
{
	public class ProcessFileCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;
		readonly MainViewModel parent;
		readonly string merchantWordsUrl = "https://www.merchantwords.com";
        
        
        List<string> combinationKeys;

        public ProcessFileCommand()
        {
            
        }
        public ProcessFileCommand(MainViewModel parent)
		{
			this.parent = parent;
			parent.PropertyChanged += delegate { CanExecuteChanged?.Invoke(this, EventArgs.Empty); };

		}
		public bool CanExecute(object parameter)
		{
			return !string.IsNullOrEmpty(parent.InputFileProcessingLabelData) &&
					!string.IsNullOrEmpty(parent.ProxiesFileProcessingLabelData) &&
                    !string.IsNullOrEmpty(parent.ResultFolderLabelData) &&
                    !parent.FileProcessingLabelData.Equals(StringConsts.FileProcessingLabelData_Processing);
		}

        public void Execute(object parameter)
        {
            var file = File.CreateText(Path.Combine(parent.ResultFolderLabelData, "MerchantKeywords.csv"));
            file.Close();
            DataHelper.sw = File.AppendText(Path.Combine(parent.ResultFolderLabelData, "MerchantKeywords.csv"));
            combinationKeys = new List<string>();

            string inputFileChosenPath = parent.InputFileProcessingLabelData;
            string proxiesFileChosenPath = parent.ProxiesFileProcessingLabelData;

            if (string.IsNullOrEmpty(inputFileChosenPath))
            {
                return;
            }
            inputFileChosenPath = inputFileChosenPath.Trim();

            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Processing;
            }, DispatcherPriority.DataBind);

            var titles = FilesHelper.ConvertCSVtoListofTitles(inputFileChosenPath);
			WebHelper.Proxies = FilesHelper.ConvertProxyFileToDictionary(proxiesFileChosenPath);
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(() =>
            {
                List<Task> TaskList = new List<Task>();
                foreach (var title in titles)
                {
                    var LastTask = new Task(() =>
                    {
                        scrapeDataFromMerchantWord(title);
                    });
                    LastTask.Start();
                    TaskList.Add(LastTask);
                }
                Task.WaitAll(TaskList.ToArray());

            }).ContinueWith((action) =>
            {
                ParallelQueue(combinationKeys).GetAwaiter().GetResult();

            }).ContinueWith((action) =>
            {
                DataHelper.sw.Close();
                parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Finish;
                Console.WriteLine(StringConsts.FileProcessingLabelData_Finish);
            }, uiScheduler);
        }

        public static void AppendData(string lineContent, TextWriter w)
        {
            w.Write(lineContent);
        }

        private static async Task ParallelQueue<T>(List<T> combinations)
		{
			Queue pending = new Queue(combinations);
			List<Task> working = new List<Task>();

			while (pending.Count + working.Count != 0)
			{
				if (working.Count < 20 && pending.Count != 0)
				{
					var item = pending.Dequeue();
                    working.Add(CustomTaskFactory.GetNewTask((T)item));
				}
				else
				{
					await Task.WhenAny(working);
					working.RemoveAll(x => x.IsCompleted);
				}
			}
		}

        private void scrapeDataFromMerchantWord(string title)
        {
            using (FlurlClient flurlClient = new FlurlClient().EnableCookies())
            {
                merchantWordsUrl.AppendPathSegment("login")
                .WithClient(flurlClient)
                .PostUrlEncodedAsync(new
                {
                    email = "goncalo.cabecinha@gmail.com",
                    password = "qwertymns"
                });
                if (!IsResultGotByTittleCorrect(flurlClient, title) && !IsResultGotByAmazonSearchTermCorrect(flurlClient, title))
                {
                    SearchByWholeTitle(flurlClient, title);
                }
            }
		}

        private void SearchByWholeTitle(FlurlClient flurlClient, string title)
        {
            Dictionary<string, int> combinations = new Dictionary<string, int>();
            var combins = DataHelper.SplitTitle(title, true);
            try
            {
                foreach (var items in combins)
                {
                    if (items.Length == 0) { continue; }
                    merchantWordsUrl.AppendPathSegment($"search/uk/{items[0]}%20{items[1]}%20{items[2]}/sort-highest.csv")
                        .WithClient(flurlClient)
                        .DownloadFileAsync(Path.GetTempPath())
                        .GetAwaiter().GetResult();
                    var data = FilesHelper.ConvertCSVtoDataTable(items);
                    if (data.Rows.Count == 0) { continue; }

                    combinations.Add(data.Rows[0].ItemArray[0].ToString(), DataHelper.ToInt(data.Rows[0].ItemArray[1].ToString()));

                    if (File.Exists(FilesHelper.getPathToCsvDownloadedFile(items)))
                    {
                        File.Delete(FilesHelper.getPathToCsvDownloadedFile(items));
                    }
                }
                var maxValue = DataHelper.GetMaximumCombinationFromDict(combinations);
                if (maxValue.Value != 0)
                {
                    combinationKeys.Add(maxValue.Key);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(title);
                Debug.WriteLine("___________");
            }
        }
        private bool IsResultGotByAmazonSearchTermCorrect(FlurlClient flurlClient, string title)
        {
            Dictionary<string, int> combinations = new Dictionary<string, int>();
            var combins = DataHelper.SplitTitle(title, false);
            try
            {
                foreach (var items in combins)
                {
                    if (items.Length == 0) { continue; }
                    merchantWordsUrl.AppendPathSegment($"search/uk/{items[0]}%20{items[1]}%20{items[2]}/sort-highest.csv")
                        .WithClient(flurlClient)
                        .DownloadFileAsync(Path.GetTempPath())
                        .GetAwaiter().GetResult();
                    var data = FilesHelper.ConvertCSVtoDataTable(items);
                    if (data.Rows.Count == 0) { continue; }

                    combinations.Add(data.Rows[0].ItemArray[0].ToString(), DataHelper.ToInt(data.Rows[0].ItemArray[1].ToString()));

                    if (File.Exists(FilesHelper.getPathToCsvDownloadedFile(items)))
                    {
                        File.Delete(FilesHelper.getPathToCsvDownloadedFile(items));
                    }
                }
                var maxValue = DataHelper.GetMaximumCombinationFromDict(combinations);
                if (maxValue.Value == 0)
                {
                    return false;
                }
                else
                {
                    combinationKeys.Add(maxValue.Key);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(title);
                Debug.WriteLine("___________");
            }
            return true;
        }
        private bool IsResultGotByTittleCorrect(FlurlClient flurlClient, string title)
        {
            Dictionary<string, int> combinations = new Dictionary<string, int>();
            var combins = DataHelper.SplitTitle(title, false);
            try
            {
                foreach (var items in combins)
                {
                    if (items.Length == 0) { continue; }
                    merchantWordsUrl
                        .AppendPathSegment($"search/uk/{items[0]}%20{items[1]}%20{items[2]}/sort-highest.csv")
                        .WithClient(flurlClient)
                        .DownloadFileAsync(Path.GetTempPath()).GetAwaiter().GetResult();
                    var data = FilesHelper.ConvertCSVtoDataTable(items);
                    if (data.Rows.Count == 0) { continue; }

                    if (data.Rows[0].ItemArray[0].Equals(
                        string.Join(" ", items).ToLower()))
                    {
                        combinations.Add(string.Join(" ", items), DataHelper.ToInt(data.Rows[0].ItemArray[1].ToString()));
                    }

                    if (File.Exists(FilesHelper.getPathToCsvDownloadedFile(items)))
                    {
                        File.Delete(FilesHelper.getPathToCsvDownloadedFile(items));
                    }
                }
                var maxValue = DataHelper.GetMaximumCombinationFromDict(combinations);
                if (maxValue.Value == 0)
                {
                    return false;
                }
                else
                {
                    combinationKeys.Add(maxValue.Key);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(title);
                Debug.WriteLine("___________");
            }
            return true;
        }
    }
}
