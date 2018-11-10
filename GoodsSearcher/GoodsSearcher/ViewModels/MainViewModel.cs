using GoodsSearcher.Commands;
using Sraper.Common;
using System.ComponentModel;
using System.Windows.Input;

namespace GoodsSearcher.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _fileProcessingLabel;
        private string _fileProcessingLabelData;

        private string _inputFileProcessingLabel;
        private string _inputFileProcessingLabelData;

        private string _proxiesFileProcessingLabel;
        private string _proxiesFileProcessingLabelData;

        private string _resultFolderLabel;
        private string _resultFolderLabelData;

        public MainViewModel()
        {
            ProcessFileCommand = new ProcessFileCommand(this);
            ChooseInputFileCommand = new ChooseInputFileCommand(this);
            ChooseProxiesFileCommand = new ChooseProxiesFileCommand(this);
            ChooseResultFolderCommand = new ChooseResultFolderCommand(this);
            InputFileProcessingLabel = StringConsts.InputFilePathLabel;
            ProxiesFileProcessingLabel = StringConsts.ProxyFilePathLabel;
            FileProcessingLabel = StringConsts.FileProcessingLabelConst;
            ResultFolderLabel = StringConsts.FileProcessingLabelData_Choose_folder;
            FileProcessingLabelData = string.Empty;
        }

        public ICommand ProcessFileCommand { get; private set; }
        public ICommand ChooseInputFileCommand { get; private set; }
        public ICommand ChooseProxiesFileCommand { get; private set; }
        public ICommand ChooseResultFolderCommand { get; private set; }

        public string ResultFolderLabel
        {
            get
            {
                return _resultFolderLabel;
            }
            private set
            {
                if (_resultFolderLabel != value)
                {
                    _resultFolderLabel = value;
                    RaisePropertyChanged(nameof(ResultFolderLabel));
                }
            }
        }
        public string ResultFolderLabelData
        {
            get
            {
                return _resultFolderLabelData;
            }
            set
            {
                if (_resultFolderLabelData != value)
                {
                    _resultFolderLabelData = value;
                    RaisePropertyChanged(nameof(ResultFolderLabelData));
                }
            }
        }

        public string InputFileProcessingLabel
        {
            get
            {
                return _inputFileProcessingLabel;
            }
            private set
            {
                if (_inputFileProcessingLabel != value)
                {
                    _inputFileProcessingLabel = value;
                    RaisePropertyChanged(nameof(InputFileProcessingLabel));
                }
            }
        }
        public string InputFileProcessingLabelData
        {
            get
            {
                return _inputFileProcessingLabelData;
            }
            set
            {
                if (_inputFileProcessingLabelData != value)
                {
                    _inputFileProcessingLabelData = value;
                    RaisePropertyChanged(nameof(InputFileProcessingLabelData));
                }
            }
        }

        public string ProxiesFileProcessingLabel
        {
            get
            {
                return _proxiesFileProcessingLabel;
            }
            private set
            {
                if (_proxiesFileProcessingLabel != value)
                {
                    _proxiesFileProcessingLabel = value;
                    RaisePropertyChanged(nameof(ProxiesFileProcessingLabel));
                }
            }
        }
        public string ProxiesFileProcessingLabelData
        {
            get
            {
                return _proxiesFileProcessingLabelData;
            }
            set
            {
                if (_proxiesFileProcessingLabelData != value)
                {
                    _proxiesFileProcessingLabelData = value;
                    RaisePropertyChanged(nameof(ProxiesFileProcessingLabelData));
                }
            }
        }

        public string FileProcessingLabel
        {
            get
            {
                return _fileProcessingLabel;
            }
            private set
            {
                if (_fileProcessingLabel != value)
                {
                    _fileProcessingLabel = value;
                    RaisePropertyChanged(nameof(FileProcessingLabel));
                }
            }
        }
        public string FileProcessingLabelData
        {
            get
            {
                return _fileProcessingLabelData;
            }
            set
            {
                if (_fileProcessingLabelData != value)
                {
                    _fileProcessingLabelData = value;
                    RaisePropertyChanged(nameof(FileProcessingLabelData));
                }
            }
        }

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
