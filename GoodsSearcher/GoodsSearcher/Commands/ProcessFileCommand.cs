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
		readonly string amazonPageUrl = "https://www.ebay.co.uk/";
		Dictionary<string, int> proxies;
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
			proxies = new Dictionary<string, int>();

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
			//await scrapeDataFromMerchantWord(titles);


			DoWork("");
			//foreach (var key in combinationKeys) //Temporary sync solution TODO: Async
			//{
			//	await DoWork(key);
			//}

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

		private async Task DoWork(string combination)
		{
			//var items = combination.Split(' ');
			string[] items = { "back", "one", "two", "three" };
			foreach (var proxy in proxies.Keys.ToList())
			{
				//var proxiedCLient = WebHelper.CreateProxiedClient(proxy.Key);
				var proxiedCLient = WebHelper.CreateClient();
				try
				{
					bool continueWork = false;
					int pageNumber = 1;
					do
					{
						var str = await amazonPageUrl.WithClient(proxiedCLient).GetStringAsync();
						var url = amazonPageUrl
							.AppendPathSegment("sch")
							.AppendPathSegment("i.html")
							.SetQueryParam("_nkw", $"{items[0]}+{items[1]}+{items[2]}_pgn={pageNumber}&_skc=50&rt=nc");
						var page = await url.WithClient(proxiedCLient)
							.GetStringAsync();
						var table = WebHelper.GetSearchEbayResultsTable(page);
						var itemsOnPage = DataHelper.GetHrefsFromHtmlList(table);
						foreach (var itemUrl in itemsOnPage)
						{
							/*Process every item on page*/
							string result = await scrapDataFromItemPage(itemUrl, proxiedCLient);
							if (!string.IsNullOrEmpty(result))
							{
								continueWork = false;
							}
						}

					} while (continueWork);
					
				}
				catch (Exception ex)
				{
					lock(lockObject)
					{
						proxies[proxy]++;
					}
				}
			}
		}

		private async Task<string> scrapDataFromItemPage(string itemPageUrl, FlurlClient client)
		{
			string result = string.Empty;

			var str = await itemPageUrl.WithClient(client).GetStringAsync();


			return result;
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
							var secondTablePage = await merchantWordsUrl.AppendPathSegment(string.Format("search/uk/{0}%20{1}%20{2}/sort-highest", items[2], items[3], items[4]))
							.WithClient(flurlClient)
							.GetStringAsync();

							var secondNode = WebHelper.GetSearchMerchantWordsResultsTable(secondTablePage);
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

							var thirdNode = WebHelper.GetSearchMerchantWordsResultsTable(thirdTablePage);
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
	}
}
