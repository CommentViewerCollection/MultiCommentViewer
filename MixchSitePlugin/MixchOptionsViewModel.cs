using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace MixchSitePlugin
{
    public class MixchOptionsViewModel : INotifyPropertyChanged
    {
        public Color ItemBackColor
        {
            get { return ChangedOptions.ItemBackColor; }
            set { ChangedOptions.ItemBackColor = value; }
        }
        public Color ItemForeColor
        {
            get { return ChangedOptions.ItemForeColor; }
            set { ChangedOptions.ItemForeColor = value; }
        }
        public Color SystemBackColor
        {
            get { return ChangedOptions.SystemBackColor; }
            set { ChangedOptions.SystemBackColor = value; }
        }
        public Color SystemForeColor
        {
            get { return ChangedOptions.SystemForeColor; }
            set { ChangedOptions.SystemForeColor = value; }
        }
        public int PoipoiKeepSeconds
        {
            get { return ChangedOptions.PoipoiKeepSeconds; }
            set { ChangedOptions.PoipoiKeepSeconds = value; }
        }
        private readonly MixchSiteOptions _origin;
        private readonly MixchSiteOptions _changed;
        internal MixchSiteOptions OriginOptions { get { return _origin; } }
        internal MixchSiteOptions ChangedOptions { get { return _changed; } }

        internal MixchOptionsViewModel(MixchSiteOptions siteOptions)
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
