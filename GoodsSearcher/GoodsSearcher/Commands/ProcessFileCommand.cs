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
using System.Net;
using HtmlAgilityPack;
using System.Data;
using GoodsSearcher.Common.Helpers;
using Sraper.Common.Models;
using System.Collections.Generic;

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
			string inputFileChosenPath = parent.InputFileProcessingLabelData;
			string proxiesFileChosenPath = parent.ProxiesFileProcessingLabelData;

			if (string.IsNullOrEmpty(inputFileChosenPath))
			{
				return;
			}
			inputFileChosenPath = inputFileChosenPath.Trim();

			parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Processing;
			merchantWordsUrl = "https://www.merchantwords.com";
			var titles = FilesHelper.ConvertCSVtoDataTable(inputFileChosenPath)
				.AsEnumerable()
				.Select(s => s.Field<string>("eBay Title")
				.Replace('\uFFFD'.ToString(), "")
				.Replace("[", "")
				.Replace("]", ""))
				.ToList();

			List<string[]> patterns = new List<string[]>();
			

			using (flurlClient = new FlurlClient().EnableCookies())
			{
				await merchantWordsUrl.AppendPathSegment("login")
				.WithClient(flurlClient)
				.PostUrlEncodedAsync(new
				{
					email = "goncalo.cabecinha@gmail.com",
					password = "qwertymns"
				});

				await DownloadMultipleTablesAsync(titles);
			}

			parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Finish;
			Console.WriteLine(StringConsts.FileProcessingLabelData_Finish);
			try
            {
                await Task.Factory.StartNew(() =>
                {
                    
                })
                .ContinueWith((action) =>
				{
					Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
					{
						
					}));
				});
			}
            catch (Exception)
            {
                parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_ErrorMessage;
            }
        }

		private async Task DownloadFileAsync(string title)
		{
			string tempTitle = title;
			var items = title.Split(' ');
			try
			{
				var firstTablePage = string.Empty;
				var secondTablePage = string.Empty;
				var thirdTablePage = string.Empty;
				if (items.Length > 3)
				{
					firstTablePage = await merchantWordsUrl.AppendPathSegment(string.Format("search/uk/{0}%20{1}%20{2}/sort-highest",items[1],items[2], items[3]))
					.WithClient(flurlClient)
					.GetStringAsync();
				}
				if (items.Length > 4)
				{
					secondTablePage = await merchantWordsUrl.AppendPathSegment(string.Format("search/uk/{0}%20{1}%20{2}/sort-highest", items[2], items[3], items[4]))
					.WithClient(flurlClient)
					.GetStringAsync();
				}
				if (items.Length > 5)
				{
					thirdTablePage = await merchantWordsUrl.AppendPathSegment(string.Format("search/uk/{0}%20{1}%20{2}/sort-highest", items[3], items[4], items[5]))
					.WithClient(flurlClient)
					.GetStringAsync();
				}

				var firstNode = WebHelper.GetSearchResultsTable(firstTablePage);
				var firstDataTable = DataHelper.ConvertHtmlTableToDataTable(firstNode);

				var secondNode = WebHelper.GetSearchResultsTable(firstTablePage);
				var secondDataTable = DataHelper.ConvertHtmlTableToDataTable(secondNode);

				var thirdNode = WebHelper.GetSearchResultsTable(firstTablePage);
				var thirdDataTable = DataHelper.ConvertHtmlTableToDataTable(thirdNode);
			}
			catch (Exception ex)
			{
				lock (lockObject)
				{
					if (ex.Message.Contains("Call failed. Collection was modified; enumeration operation may not execute"))
					{
						errors.Add(title);
					}
				}
			}
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
