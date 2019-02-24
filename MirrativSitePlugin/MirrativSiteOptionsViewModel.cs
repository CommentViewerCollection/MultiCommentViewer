using System;
using System.ComponentModel;

namespace MirrativSitePlugin
{
    class MirrativSiteOptionsViewModel : INotifyPropertyChanged
    {
        public bool NeedAutoSubNickname
        {
            get => changed.NeedAutoSubNickname;
            set => changed.NeedAutoSubNickname = value;
        }
        private readonly MirrativSiteOptions _origin;
        private readonly MirrativSiteOptions changed;
        internal MirrativSiteOptions OriginOptions { get { return _origin; } }
        internal MirrativSiteOptions ChangedOptions { get { return changed; } }

        internal MirrativSiteOptionsViewModel(MirrativSiteOptions siteOptions)
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
