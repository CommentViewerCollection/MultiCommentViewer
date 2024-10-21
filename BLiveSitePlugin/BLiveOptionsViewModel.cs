using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace BLiveSitePlugin
{
    public class BLiveOptionsViewModel : INotifyPropertyChanged
    {
        public ICommand ShowOpenStampMusicSelectorCommand { get; }
        public ICommand ShowOpenYellMusicSelectorCommand { get; }
        private void ShowOpenStampMusicSelector()
        {
            var filename = OpenFileDialog("", "音声ファイルを指定して下さい", "waveファイル|*.wav");
            if (!string.IsNullOrEmpty(filename))
            {
                StampMusicFilePath = filename;
            }
        }
        private void ShowOpenYellMusicSelector()
        {
            var filename = OpenFileDialog("", "音声ファイルを指定して下さい", "waveファイル|*.wav");
            if (!string.IsNullOrEmpty(filename))
            {
                YellMusicFilePath = filename;
            }
        }
        protected virtual string OpenFileDialog(string defaultPath, string title, string filter)
        {
            string ret = null;
            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Title = title;
            fileDialog.Filter = filter;
            var result = fileDialog.ShowDialog();
            if (result == true)
            {
                ret = fileDialog.FileName;
            }
            return ret;
        }
        public int StampSize
        {
            get { return _changed.StampSize; }
            set { _changed.StampSize = value; }
        }
        public bool IsPlayStampMusic
        {
            get { return _changed.IsPlayStampMusic; }
            set
            {
                _changed.IsPlayStampMusic = value;
                RaisePropertyChanged();
            }
        }
        public string StampMusicFilePath
        {
            get { return _changed.StampMusicFilePath; }
            set
            {
                _changed.StampMusicFilePath = value;
                RaisePropertyChanged();
            }
        }
        public bool IsPlayYellMusic
        {
            get { return _changed.IsPlayYellMusic; }
            set
            {
                _changed.IsPlayYellMusic = value;
                RaisePropertyChanged();
            }
        }
        public string YellMusicFilePath
        {
            get { return _changed.YellMusicFilePath; }
            set
            {
                _changed.YellMusicFilePath = value;
                RaisePropertyChanged();
            }
        }
        public bool IsAutoSetNickname
        {
            get { return ChangedOptions.IsAutoSetNickname; }
            set { ChangedOptions.IsAutoSetNickname = value; }
        }
        private readonly BLiveSiteOptions _origin;
        private readonly BLiveSiteOptions _changed;
        internal BLiveSiteOptions OriginOptions { get { return _origin; } }
        internal BLiveSiteOptions ChangedOptions { get { return _changed; } }

        internal BLiveOptionsViewModel(BLiveSiteOptions siteOptions)
        {
            _origin = siteOptions;
            _changed = siteOptions.Clone();
            ShowOpenStampMusicSelectorCommand = new RelayCommand(ShowOpenStampMusicSelector);
            ShowOpenYellMusicSelectorCommand = new RelayCommand(ShowOpenYellMusicSelector);
        }

        #region INotifyPropertyChanged
        [NonSerialized]
        private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
        /// <summary>
        /// 
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
