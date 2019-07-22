using System;
using System.ComponentModel;
using System.Windows.Media;

namespace ShowRoomSitePlugin
{
    internal class ShowRoomSiteOptionsViewModel : INotifyPropertyChanged
    {
        public bool IsAutoSetNickname
        {
            get { return ChangedOptions.IsAutoSetNickname; }
            set { ChangedOptions.IsAutoSetNickname = value; }
        }
        public bool IsShowJoinMessage
        {
            get { return ChangedOptions.IsShowJoinMessage; }
            set { ChangedOptions.IsShowJoinMessage = value; }
        }
        public bool IsShowLeaveMessage
        {
            get { return ChangedOptions.IsShowLeaveMessage; }
            set { ChangedOptions.IsShowLeaveMessage = value; }
        }
        public bool IsIgnore50Counts
        {
            get => ChangedOptions.IsIgnore50Counts;
            set => ChangedOptions.IsIgnore50Counts = value;
        }
        internal IShowRoomSiteOptions OriginOptions { get; }
        internal IShowRoomSiteOptions ChangedOptions { get; }

        internal ShowRoomSiteOptionsViewModel(IShowRoomSiteOptions siteOptions)
        {
            OriginOptions = siteOptions;
            ChangedOptions = siteOptions.Clone();
        }
        public ShowRoomSiteOptionsViewModel() : this(new ShowRoomSiteOptions())
        {
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
