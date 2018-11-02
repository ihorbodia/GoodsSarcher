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
            //string inputFileChosenPath = parent.InputFileProcessingLabelData;
            //string proxiesFileChosenPath = parent.ProxiesFileProcessingLabelData;

            //if (string.IsNullOrEmpty(inputFileChosenPath.Trim()))
            //{
            //    return;
            //}
            //parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Processing;
            Cookie cookie;
            try
            {
                Task.Factory.StartNew(async () =>
                {
                    using (var cli = new FlurlClient("https://www.merchantwords.com").EnableCookies())
                    {
                        await cli.Request("/login").PostJsonAsync(new
                        {
                            email = "goncalo.cabecinha@gmail.com",
                            password = "qwertymns"
                        });
                        cookie = cli.Cookies.First().Value;
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
