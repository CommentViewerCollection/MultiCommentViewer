using System;
using System.ComponentModel;

namespace WhowatchSitePlugin
{
    internal class WhowatchSiteOptionsViewModel : INotifyPropertyChanged
    {
        public bool NeedAutoSubNickname
        {
            get => ChangedOptions.NeedAutoSubNickname;
            set => ChangedOptions.NeedAutoSubNickname = value;
        }

        internal IWhowatchSiteOptions OriginOptions { get; }
        internal IWhowatchSiteOptions ChangedOptions { get; }

        internal WhowatchSiteOptionsViewModel(IWhowatchSiteOptions siteOptions)
        {
            OriginOptions = siteOptions;
            ChangedOptions = siteOptions.Clone();
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
