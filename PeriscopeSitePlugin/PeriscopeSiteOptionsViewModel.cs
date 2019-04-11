using System;
using System.ComponentModel;
using System.Windows.Media;

namespace PeriscopeSitePlugin
{
    internal class PeriscopeSiteOptionsViewModel : INotifyPropertyChanged
    {
        public bool IsAutoSetNickname
        {
            get { return ChangedOptions.IsAutoSetNickname; }
            set { ChangedOptions.IsAutoSetNickname = value; }
        }

        internal IPeriscopeSiteOptions OriginOptions { get; }
        internal IPeriscopeSiteOptions ChangedOptions { get; }

        internal PeriscopeSiteOptionsViewModel(IPeriscopeSiteOptions siteOptions)
        {
            OriginOptions = siteOptions;
            ChangedOptions = siteOptions.Clone();
        }
        public PeriscopeSiteOptionsViewModel() : this(new PeriscopeSiteOptions())
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
