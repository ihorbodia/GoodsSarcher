using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GoodsSearcher.ViewModels;
using Sraper.Common;

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
            return !string.IsNullOrEmpty(parent.InputFileProcessingLabelData) &&
                    !string.IsNullOrEmpty(parent.ProxiesFileProcessingLabelData) &&
                    !parent.FileProcessingLabelData.Equals(StringConsts.FileProcessingLabelData_Processing);
        }

        public void Execute(object parameter)
        {
            string inputFileChosenPath = parent.InputFileProcessingLabelData;
            string proxiesFileChosenPath = parent.ProxiesFileProcessingLabelData;
            
            if (string.IsNullOrEmpty(inputFileChosenPath.Trim()))
            {
                return;
            }
            parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Processing;
            try
            {
                Task.Factory.StartNew(() =>
                {
                    //ShareholderAnalyzerLogic ms = new ShareholderAnalyzerLogic();
                    //ms.ProcessFile(chosenPath, chosenFodlerPath);
                })
                .ContinueWith((action) =>
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => {
                        parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_Finish;
                        Console.WriteLine(StringConsts.FileProcessingLabelData_Finish);
                    }));
                });
            }
            catch (Exception)
            {
                parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_ErrorMessage;
            }
        }
        
    }
}
