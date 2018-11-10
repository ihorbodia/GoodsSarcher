using GoodsSearcher.Common.Helpers;
using GoodsSearcher.ViewModels;
using Sraper.Common;
using System;
using System.Windows.Input;

namespace GoodsSearcher.Commands
{
    internal class ChooseProxiesFileCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        readonly MainViewModel parent;
        public ChooseProxiesFileCommand(MainViewModel parent)
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
            string chosenProxiesFilePath = FilesHelper.SelectFile();
            if (!string.IsNullOrEmpty(chosenProxiesFilePath.Trim()))
            {
                parent.ProxiesFileProcessingLabelData = chosenProxiesFilePath;
                if (!string.IsNullOrEmpty(parent.ProxiesFileProcessingLabelData) && !string.IsNullOrEmpty(parent.InputFileProcessingLabelData))
                {
                    parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_CanProcess;
                }
                if (string.IsNullOrEmpty(parent.InputFileProcessingLabelData))
                {
                    parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_ChooseInputFile;
                }
                if (string.IsNullOrEmpty(parent.ProxiesFileProcessingLabelData))
                {
                    parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_ChooseProxyFile;
                }
                if (string.IsNullOrEmpty(parent.ResultFolderLabelData))
                {
                    parent.FileProcessingLabelData = StringConsts.FileProcessingLabelData_ChooseResultFolder;
                }
            }
        }
    }
}
