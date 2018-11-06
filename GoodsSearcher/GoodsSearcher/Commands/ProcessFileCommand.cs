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
using System.Threading;
using System.Diagnostics;

namespace GoodsSearcher.Commands
{
	internal class ProcessFileCommand : ICommand
	{
		public event EventHandler CanExecuteChanged;
		readonly MainViewModel parent;
		FlurlClient flurlClient;
		string merchantWordsUrl;
		List<string> errors = new List<string>();
		int counter = 0;
		object lockObject = new object();
		List<string> CombinationKeys;
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
            CombinationKeys = new List<string>();
            string inputFileChosenPath = parent.InputFileProcessingLabelData;
            string proxiesFileChosenPath = parent.ProxiesFileProcessingLabelData;

            if (string.IsNullOrEmpty(inputFileChosenPath))
            {
                return;
            }
            inputFileChosenPath = inputFileChosenPath.Trim();

            parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Processing;
            merchantWordsUrl = "https://www.merchantwords.com";
            var titles = FilesHelper.ConvertCSVtoListofTitles(inputFileChosenPath);

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
                        CombinationKeys.Add(maxCombinationKey);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine(title);
                        Debug.WriteLine();
                    }
                }
            }
            parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Finish;
            Console.WriteLine(StringConsts.FileProcessingLabelData_Finish);
        }

		private async Task DownloadFileAsync(string title)
		{
			
		}

		private async Task DownloadMultipleTablesAsync(List<string> titles)
		{
			counter++;
			await Task.WhenAll(titles.Select(title => DownloadFileAsync(title)));
			if (errors.Any() && counter < 10)
			{
				List<string> items = new List<string>(errors);
				errors.Clear();
				await DownloadMultipleTablesAsync(items);
			}
		}
	}
}
