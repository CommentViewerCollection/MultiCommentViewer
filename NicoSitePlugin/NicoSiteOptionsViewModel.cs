using System;
using System.ComponentModel;

namespace NicoSitePlugin
{
    public class NicoSiteOptionsViewModel : INotifyPropertyChanged
    {
        public int OfficialRoomsRetrieveCount
        {
            get { return _changed.OfficialRoomsRetrieveCount; }
            set { _changed.OfficialRoomsRetrieveCount = value; }
        }
        public bool IsShow184
        {
            get => _changed.IsShow184;
            set => _changed.IsShow184 = value;
        }
        public bool IsShow184Id
        {
            get => _changed.IsShow184Id;
            set => _changed.IsShow184Id = value;
        }
        public bool IsAutoSetNickname
        {
            get { return ChangedOptions.IsAutoSetNickname; }
            set { ChangedOptions.IsAutoSetNickname = value; }
        }
        public bool IsAutoGetUsername
        {
            get => ChangedOptions.IsAutoGetUsername;
            set => ChangedOptions.IsAutoGetUsername = value;
        }
        private readonly NicoSiteOptions _origin;
        private readonly NicoSiteOptions _changed;
        internal NicoSiteOptions OriginOptions { get { return _origin; } }
        internal NicoSiteOptions ChangedOptions { get { return _changed; } }

        public NicoSiteOptionsViewModel()
        {
            //if(IsInDesigner)
            _changed = new NicoSiteOptions { OfficialRoomsRetrieveCount = 5 };

        }
        internal NicoSiteOptionsViewModel(NicoSiteOptions siteOptions)
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
