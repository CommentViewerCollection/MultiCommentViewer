using System;
using System.ComponentModel;
using System.Windows.Media;

namespace TwitchSitePlugin
{
    class TwitchSiteOptionsViewModel : INotifyPropertyChanged
    {
        public bool NeedAutoSubNickname
        {
            get => changed.NeedAutoSubNickname;
            set => changed.NeedAutoSubNickname = value;
        }
        public string NeedAutoSubNicknameStr
        {
            get => changed.NeedAutoSubNicknameStr;
            set => changed.NeedAutoSubNicknameStr = value;
        }
        public Color NoticeBackColor
        {
            get { return ChangedOptions.NoticeBackColor; }
            set { ChangedOptions.NoticeBackColor = value; }
        }
        public Color NoticeForeColor
        {
            get { return ChangedOptions.NoticeForeColor; }
            set { ChangedOptions.NoticeForeColor = value; }
        }
        private readonly TwitchSiteOptions _origin;
        private readonly TwitchSiteOptions changed;
        internal TwitchSiteOptions OriginOptions { get { return _origin; } }
        internal TwitchSiteOptions ChangedOptions { get { return changed; } }

        internal TwitchSiteOptionsViewModel(TwitchSiteOptions siteOptions)
        {
            _origin = siteOptions;
            changed = siteOptions.Clone();
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
