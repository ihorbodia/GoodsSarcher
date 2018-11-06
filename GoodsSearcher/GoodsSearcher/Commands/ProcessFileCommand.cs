using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
using System.Net.Sockets;

namespace GoodsSearcher.Commands
{
	internal class ProcessFileCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;
		readonly MainViewModel parent;
		FlurlClient flurlClient;
		readonly string merchantWordsUrl = "https://www.merchantwords.com";
		readonly string amazonPageUrl = "https://www.amazon.co.uk/";
		Dictionary<string, bool> proxies;
		int counter = 0;
		object lockObject = new object();
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
					!parent.FileProcessingLabelData.Equals(StringConsts.FileProcessingLabelData_Processing);
		}

        public async void Execute(object parameter)
        {
            combinationKeys = new List<string>();
			proxies = new Dictionary<string, bool>();

			string inputFileChosenPath = parent.InputFileProcessingLabelData;
            string proxiesFileChosenPath = parent.ProxiesFileProcessingLabelData;

            if (string.IsNullOrEmpty(inputFileChosenPath))
            {
                return;
            }
            inputFileChosenPath = inputFileChosenPath.Trim();

            parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Processing;

			
            var titles = FilesHelper.ConvertCSVtoListofTitles(inputFileChosenPath);
			proxies = FilesHelper.ConvertProxyFileToDictionary(proxiesFileChosenPath);
			await scrapeDataFromMerchantWord(titles);

			foreach (var key in combinationKeys)
			{
				await DoWork(key);
			}
			//Task t = ParallelQueue(combinationKeys, DoWork);


			parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Finish;
            Console.WriteLine(StringConsts.FileProcessingLabelData_Finish);
        }

		private static async Task ParallelQueue<T>(List<T> items, Func<T, Task> func)
		{
			Queue pending = new Queue(items);
			List<Task> working = new List<Task>();

			while (pending.Count + working.Count != 0)
			{
				if (working.Count < 20 && pending.Count != 0)
				{
					var item = pending.Dequeue();
					working.Add(Task.Factory.StartNew(async () => await func((T)item)));
				}
				else
				{
					await Task.WhenAny(working);
					working.RemoveAll(x => x.IsCompleted);
				}
			}
		}

		private async Task DoWork(string item)
		{
			var items = item.Split(' ');
			foreach (var proxy in proxies)
			{
				var proxiedCLient = WebHelper.CreateProxiedClient(proxy.Key);
				try
				{
					await amazonPageUrl.WithClient(proxiedCLient).GetAsync();
					var page = await amazonPageUrl
						.AppendPathSegment(string.Format("s/ref=nb_sb_noss_2?url=search-alias%3Daps&field-keywords={0}+{1}+{2}", items[1], items[2], items[3]))
						.WithClient(flurlClient)
						.GetStringAsync();
				}
				catch (FlurlHttpException ex)
				{

				}
	

			}
		}

		private async Task scrapeDataFromMerchantWord(List<string> titles)
		{
			foreach (var title in titles)
			{
				using (flurlClient = new FlurlClient().EnableCookies())
				{
					await merchantWordsUrl.AppendPathSegment("login")
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
							var firstTablePage = await merchantWordsUrl.AppendPathSegment(string.Format("search/uk/{0}%20{1}%20{2}/sort-highest", items[1], items[2], items[3]))
							.WithClient(flurlClient)
							.GetStringAsync();

							var firstNode = WebHelper.GetSearchResultsTable(firstTablePage);
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
							var secondTablePage = await merchantWordsUrl.AppendPathSegment(string.Format("search/uk/{0}%20{1}%20{2}/sort-highest", items[2], items[3], items[4]))
							.WithClient(flurlClient)
							.GetStringAsync();

							var secondNode = WebHelper.GetSearchResultsTable(secondTablePage);
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
							var thirdTablePage = await merchantWordsUrl.AppendPathSegment(string.Format("search/uk/{0}%20{1}%20{2}/sort-highest", items[3], items[4], items[5]))
							.WithClient(flurlClient)
							.GetStringAsync();

							var thirdNode = WebHelper.GetSearchResultsTable(thirdTablePage);
							var thirdEnumerable = DataHelper.ConvertHtmlTableToDataTable(thirdNode)?
								.AsEnumerable();
							var thirdVolumeString = thirdEnumerable?
								.FirstOrDefault(datarow => datarow[0].ToString()
								.Equals($"{items[4]} {items[5]} {items[6]}".ToLower()))?
								[2].ToString();

							combinations.Add($"{items[4]} {items[5]} {items[6]}", DataHelper.ToInt(thirdVolumeString));
						}

						string maxCombinationKey = string.Empty;
						if (combinations.Any())
						{
							maxCombinationKey = combinations.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
						}
						else
						{
							return;
						}
						combinationKeys.Add(maxCombinationKey);
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex.Message);
						Debug.WriteLine(title);
						Debug.WriteLine("___________");
					}
				}
			}
		}
		//private async Task DownloadMultipleTablesAsync(List<string> titles)
		//{
		//	//counter++;
		//	//await Task.WhenAll(titles.Select(title => DownloadFileAsync(title)));
		//	//if (errors.Any() && counter < 10)
		//	//{
		//	//	List<string> items = new List<string>(errors);
		//	//	errors.Clear();
		//	//	await DownloadMultipleTablesAsync(items);
		//	//}
		//}
	}
}
