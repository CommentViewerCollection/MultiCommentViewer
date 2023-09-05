using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace MirrativSitePlugin
{
    class MirrativSiteOptionsViewModel : INotifyPropertyChanged
    {
        public bool NeedAutoSubNickname
        {
            get => changed.NeedAutoSubNickname;
            set => changed.NeedAutoSubNickname = value;
        }
        public bool IsShowJoinMessage
        {
            get => changed.IsShowJoinMessage;
            set => changed.IsShowJoinMessage = value;
        }
        public bool IsShowLeaveMessage
        {
            get => changed.IsShowLeaveMessage;
            set => changed.IsShowLeaveMessage = value;
        }
        public Color ItemForeColor
        {
            get => changed.ItemForeColor;
            set => changed.ItemForeColor = value;
        }
        public Color ItemBackColor
        {
            get => changed.ItemBackColor;
            set => changed.ItemBackColor = value;
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
        public MirrativSiteOptionsViewModel()
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {
                var options = new MirrativSiteOptions
                {
                    ItemBackColor = Colors.Blue,
                    ItemForeColor = Colors.Red,
                    NeedAutoSubNickname = true,
                    PollingIntervalSec = 30,
                };
                _origin = options;
                changed = options.Clone();
            }
            else
            {
                throw new NotSupportedException();
            }
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
