﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GoodsSearcher.ViewModels;
using Sraper.Common;
using Flurl;
using Flurl.Http;
using System.Linq;
using System.Data;
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
        static readonly object lockObject = new object();
		List<string> combinationKeys;

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
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Combination;ASIN;Volume");
                foreach (var item in WebHelper.ResultList)
                {
                    sb.AppendLine(string.Format("{0};{1};{2}",
                        item.Combination,
                        item.ASIN,
                        item.Price));
                }
                File.WriteAllText(Path.Combine(parent.ResultFolderLabelData, "AmazonData.csv"), sb.ToString());
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Finish;
                    Console.WriteLine(StringConsts.FileProcessingLabelData_Finish);
                },DispatcherPriority.DataBind);
            }, uiScheduler);
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
                var items = title.Split(' ');
                Dictionary<string, int> combinations = new Dictionary<string, int>();
                try
                {
                    if (items.Length > 3)
                    {
                        var firstTablePage = merchantWordsUrl.AppendPathSegment(string.Format("search/uk/{0}%20{1}%20{2}/sort-highest", items[1], items[2], items[3]))
                        .WithClient(flurlClient)
                        .GetStringAsync().GetAwaiter().GetResult();

                        var firstNode = WebHelper.GetSearchMerchantWordsResultsTable(firstTablePage);
                        var firstEumerable = DataHelper.ConvertHtmlTableToDataTable(firstNode)?
                            .AsEnumerable();
                        var firstVolumeString = firstEumerable?
                            .FirstOrDefault(datarow => datarow[0].ToString()
                            .Equals($"{items[1]} {items[2]} {items[3]}".ToLower()))?
                            [2].ToString();

                        combinations.Add($"{items[1]} {items[2]} {items[3]}", DataHelper.ToInt(firstVolumeString));
                    }
                    if (items.Length > 4)
                    {
                        var secondTablePage = merchantWordsUrl.AppendPathSegment(string.Format("search/uk/{0}%20{1}%20{2}/sort-highest", items[2], items[3], items[4]))
                        .WithClient(flurlClient)
                        .GetStringAsync();

                        var secondNode = WebHelper.GetSearchMerchantWordsResultsTable(secondTablePage.Result);
                        var secondEnumerable = DataHelper.ConvertHtmlTableToDataTable(secondNode)?
                            .AsEnumerable();
                        var stringSecondVolume = secondEnumerable?
                            .FirstOrDefault(datarow => datarow[0].ToString()
                            .Equals($"{items[2]} {items[3]} {items[4]}".ToLower()))?
                            [2].ToString();

                        combinations.Add($"{items[2]} {items[3]} {items[4]}", DataHelper.ToInt(stringSecondVolume));
                    }
                    if (items.Length > 6)
                    {
                        var thirdTablePage = merchantWordsUrl.AppendPathSegment(string.Format("search/uk/{0}%20{1}%20{2}/sort-highest", items[3], items[4], items[5]))
                        .WithClient(flurlClient)
                        .GetStringAsync();

                        var thirdNode = WebHelper.GetSearchMerchantWordsResultsTable(thirdTablePage.Result);
                        var thirdEnumerable = DataHelper.ConvertHtmlTableToDataTable(thirdNode)?
                            .AsEnumerable();
                        var thirdVolumeString = thirdEnumerable?
                            .FirstOrDefault(datarow => datarow[0].ToString()
                            .Equals($"{items[4]} {items[5]} {items[6]}".ToLower()))?
                            [2].ToString();

                        combinations.Add($"{items[4]} {items[5]} {items[6]}", DataHelper.ToInt(thirdVolumeString));
                    }
                    if (!combinations.Values.Distinct().Skip(1).Any() && combinations.Count > 1)
                    {
                        searchByAmazonSearchTerm(flurlClient, combinations.Keys.ToList());
                    }
                    string maxCombinationKey = string.Empty;
                    if (combinations.Any())
                    {
                        maxCombinationKey = combinations.FirstOrDefault(x => x.Value == combinations.Values.Max()).Key;
                    }
                    else
                    {
                        return;
                    }
                    if (maxCombinationKey != null)
                    {
                        combinationKeys.Add(maxCombinationKey);
                    }
                }
                catch (FlurlHttpException ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(title);
                    Debug.WriteLine("___________");
                }
            }
		}

        private string searchByAmazonSearchTerm(FlurlClient flurlClient, List<string> combinations)
        {
            foreach (var combination in combinations)
            {
                var items = combination.Split(' ');
                var tablePage = merchantWordsUrl.AppendPathSegment(string.Format($"search/uk/{items[0]?? ""}%20{ items[1]?? ""}%20{items[2]?? ""}/sort-highest"))
                       .WithClient(flurlClient)
                       .WithTimeout(5)
                       .GetStringAsync();

                var node = WebHelper.GetSearchMerchantWordsResultsTable(tablePage.Result);
                var dt = DataHelper.ConvertHtmlTableToDataTable(node);
                var enumerable = dt?.AsEnumerable();
                var amazonSearchTermValue = enumerable?
                    .FirstOrDefault(datarow => datarow[0].ToString()
                    .Equals($"{items[4]} {items[5]} {items[6]}".ToLower()))?
                    [2].ToString();
            }

            return string.Empty;
        }
    }
}
