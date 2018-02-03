using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Windows.Input;
namespace BouyomiPlugin
{
    class ConfigViewModel : ViewModelBase
    {
        private readonly Options _options;
        public bool IsEnabled
        {
            get { return _options.IsEnabled; }
            set { _options.IsEnabled = value; }
        }
        public string ExeLocation
        {
            get { return _options.BouyomiChanPath; }
            set { _options.BouyomiChanPath = value; }
        }
        public bool IsReadHandleName
        {
            get { return _options.IsReadHandleName; }
            set { _options.IsReadHandleName = value; }
        }
        public bool IsReadComment
        {
            get { return _options.IsReadComment; }
            set {  _options.IsReadComment = value; }
        }
        public bool IsAppendNickTitle
        {
            get { return _options.IsAppendNickTitle; }
            set { _options.IsAppendNickTitle = value; }
        }
        public string NickTitle
        {
            get { return _options.NickTitle; }
            set { _options.NickTitle = value; }
        }
        private RelayCommand _showFilePickerCommand;
        public ICommand ShowFilePickerCommand
        {
            get
            {
                if(_showFilePickerCommand == null)
                {
                    _showFilePickerCommand = new RelayCommand(() =>
                    {
                        var fileDialog = new System.Windows.Forms.OpenFileDialog();
                        fileDialog.Title = "棒読みちゃんの実行ファイル（BouyomiChan.exe）を選択してください";
                        fileDialog.Filter = "棒読みちゃん | BouyomiChan.exe";
                        var result = fileDialog.ShowDialog();
                        if(result == System.Windows.Forms.DialogResult.OK)
                        {
                            this.ExeLocation = fileDialog.FileName;
                        }
                    });
                }
                return _showFilePickerCommand;
            }
        }
        public ConfigViewModel()
        {
            if (IsInDesignMode)
            {

            }else
            {
                throw new NotSupportedException();
            }
        }
        [GalaSoft.MvvmLight.Ioc.PreferredConstructor]
        public ConfigViewModel(Options options)
        {
            _options = options;
            _options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_options.IsEnabled):
                        RaisePropertyChanged(nameof(IsEnabled));
                        break;
                    case nameof(_options.BouyomiChanPath):
                        RaisePropertyChanged(nameof(ExeLocation));
                        break;
                    case nameof(_options.IsReadHandleName):
                        RaisePropertyChanged(nameof(IsReadHandleName));
                        break;
                    case nameof(_options.IsReadComment):
                        RaisePropertyChanged(nameof(IsReadComment));
                        break;
                }
            };
        }
    }
}
