using System;
using System.ComponentModel;
using System.Windows.Media;

namespace LineLiveSitePlugin
{
    internal class LineLiveSiteOptionsViewModel : INotifyPropertyChanged
    {
        public bool IsAutoSetNickname
        {
            get { return ChangedOptions.IsAutoSetNickname; }
            set { ChangedOptions.IsAutoSetNickname = value; }
        }

        internal ILineLiveSiteOptions OriginOptions { get; }
        internal ILineLiveSiteOptions ChangedOptions { get; }

        internal LineLiveSiteOptionsViewModel(ILineLiveSiteOptions siteOptions)
        {
            OriginOptions = siteOptions;
            ChangedOptions = siteOptions.Clone();
        }
        public LineLiveSiteOptionsViewModel() : this(new LineLiveSiteOptions())
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
