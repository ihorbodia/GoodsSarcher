using GoodsSearcher.Common.Helpers;
using GoodsSearcher.ViewModels;
using Sraper.Common;
using System;
using System.Windows.Input;

namespace GoodsSearcher.Commands
{
    internal class ChooseInputFileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        readonly MainViewModel parent;
        public ChooseInputFileCommand(MainViewModel parent)
        {
            this.parent = parent;
            parent.PropertyChanged += delegate { CanExecuteChanged?.Invoke(this, EventArgs.Empty); };
        }
        public bool CanExecute(object parameter)
        {
            return !parent.FileProcessingLabelData.Equals(StringConsts.FileProcessingLabelData_Processing);
        }

        public void Execute(object parameter)
        {
            string inputFileChoosePath = FilesHelper.SelectFile();
            if (!string.IsNullOrEmpty(inputFileChoosePath.Trim()))
            {
                parent.InputFileProcessingLabelData = inputFileChoosePath;
                if (!string.IsNullOrEmpty(parent.InputFileProcessingLabelData) && !string.IsNullOrEmpty(parent.ProxiesFileProcessingLabelData))
                {
                    parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_CanProcess;
                }
                if (string.IsNullOrEmpty(parent.InputFileProcessingLabel))
                {
                    parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_ChooseProxyFile;
                }
                if (string.IsNullOrEmpty(parent.ProxiesFileProcessingLabelData))
                {
                    parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_ChooseProxyFile;
                }
            }
        }
    }
}
