using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Globalization;
using System.Collections.Concurrent;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Diagnostics;
using System.Net;
namespace CommentViewer.Plugin
{
    public sealed class ConfigViewModel : ViewModelBase
    {
        private readonly Options _options;
        public bool IsEnabled
        {
            get { return _options.IsEnabled; }
            set { _options.IsEnabled = value; }
        }
        public string HcgSettingFilePath
        {
            get { return _options.HcgSettingFilePath; }
            set { _options.HcgSettingFilePath = value; }
        }
        public ICommand ShowFilePickerCommand { get; }
        private void ShowFilePicker()
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Title = "HTML5コメジェネの設定ファイルを選択してください",
                Filter = "設定ファイル | setting.xml"
            };
            var result = fileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.HcgSettingFilePath = fileDialog.FileName;
            }
        }
        public ConfigViewModel()
        {
            if (IsInDesignMode)
            {
                _options = new Plugin.Options();
                IsEnabled = true;
                HcgSettingFilePath = "HTML5コメジェネ設定ファイルパス";
            }
            else
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
                    case nameof(_options.HcgSettingFilePath):
                        RaisePropertyChanged(nameof(HcgSettingFilePath));
                        break;
                }
            };
            ShowFilePickerCommand = new RelayCommand(ShowFilePicker);
        }
    }
}
