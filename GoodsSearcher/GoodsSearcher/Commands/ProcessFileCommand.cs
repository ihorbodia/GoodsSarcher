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

namespace GoodsSearcher.Commands
{
    internal class ProcessFileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        readonly MainViewModel parent;
        public ProcessFileCommand(MainViewModel parent)
        {
            this.parent = parent;
            parent.PropertyChanged += delegate { CanExecuteChanged?.Invoke(this, EventArgs.Empty); };
        }
        public bool CanExecute(object parameter)
        {
            return true;
            //return !string.IsNullOrEmpty(parent.InputFileProcessingLabelData) &&
            //        !string.IsNullOrEmpty(parent.ProxiesFileProcessingLabelData) &&
            //        !parent.FileProcessingLabelData.Equals(StringConsts.FileProcessingLabelData_Processing);
        }

        public void Execute(object parameter)
        {
			string inputFileChosenPath = parent.InputFileProcessingLabelData;
			string proxiesFileChosenPath = parent.ProxiesFileProcessingLabelData;

			if (string.IsNullOrEmpty(inputFileChosenPath))
			{
				return;
			}
			inputFileChosenPath = inputFileChosenPath.Trim();

			parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Processing;
			string url = "https://www.merchantwords.com";
			var csvtable = FilesHelper.ConvertCSVtoDataTable(inputFileChosenPath);

			try
            {
                Task.Factory.StartNew(async () =>
                {
                    using (var cli = new FlurlClient().EnableCookies())
                    {
						await url.AppendPathSegment("login")
						.WithClient(cli)
						.PostUrlEncodedAsync(new
						{
							email = "goncalo.cabecinha@gmail.com",
							password = "qwertymns"
						});

						var page = await url.AppendPathSegment("search/uk/one%20two%20three/sort-highest")
						.WithClient(cli)
						.GetStringAsync();

						HtmlDocument htmlDocument = new HtmlDocument();
						htmlDocument.LoadHtml(page);

						var htmlTable = htmlDocument.DocumentNode
						.SelectSingleNode("/html[1]/body[1]/div[2]/section[1]/div[2]/div[1]/div[1]/div[2]/table[1]");

						var headers = htmlDocument.DocumentNode.SelectNodes("//tr/th");
						DataTable table = new DataTable();
						foreach (HtmlNode header in headers)
							table.Columns.Add(header.InnerText); // create columns from th
																 // select rows with td elements 
						foreach (var row in htmlDocument.DocumentNode.SelectNodes("//tr[td]"))
							table.Rows.Add(row.SelectNodes("td").Select(td => td.InnerText).ToArray());

                    }
                });
                //.ContinueWith((action) =>
                //{
                //    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                //        parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Finish;
                //        Console.WriteLine(StringConsts.FileProcessingLabelData_Finish);
                //    }));
                //});
            }
            catch (Exception)
            {
                parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_ErrorMessage;
            }
        }
        
    }
}
