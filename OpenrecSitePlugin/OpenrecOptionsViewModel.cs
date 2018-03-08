using System;
using System.ComponentModel;

namespace OpenrecSitePlugin
{
    public class OpenrecOptionsViewModel : INotifyPropertyChanged
    {
        public int StampSize
        {
            get { return _changed.StampSize; }
            set { _changed.StampSize = value; }
        }
        private readonly OpenrecSiteOptions _origin;
        private readonly OpenrecSiteOptions _changed;
        internal OpenrecSiteOptions OriginOptions { get { return _origin; } }
        internal OpenrecSiteOptions ChangedOptions { get { return _changed; } }

        internal OpenrecOptionsViewModel(OpenrecSiteOptions siteOptions)
        {
            _origin = siteOptions;
            _changed = siteOptions.Clone();
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
