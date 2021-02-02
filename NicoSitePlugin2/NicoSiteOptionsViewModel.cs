using System;
using System.ComponentModel;
using System.Windows.Media;

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
        public Color AdBackColor
        {
            get => ChangedOptions.AdBackColor;
            set => ChangedOptions.AdBackColor = value;
        }
        public Color AdForeColor
        {
            get => ChangedOptions.AdForeColor;
            set => ChangedOptions.AdForeColor = value;
        }
        public Color ItemBackColor
        {
            get => ChangedOptions.ItemBackColor;
            set => ChangedOptions.ItemBackColor = value;
        }
        public Color ItemForeColor
        {
            get => ChangedOptions.ItemForeColor;
            set => ChangedOptions.ItemForeColor = value;
        }
        public Color SpiBackColor
        {
            get => ChangedOptions.SpiBackColor;
            set => ChangedOptions.SpiBackColor = value;
        }
        public Color SpiForeColor
        {
            get => ChangedOptions.SpiForeColor;
            set => ChangedOptions.SpiForeColor = value;
        }
        public bool IsShowEmotion
        {
            get => ChangedOptions.IsShowEmotion;
            set => ChangedOptions.IsShowEmotion = value;
        }
        public Color EmotionBackColor
        {
            get => ChangedOptions.EmotionBackColor;
            set => ChangedOptions.EmotionBackColor = value;
        }
        public Color EmotionForeColor
        {
            get => ChangedOptions.EmotionForeColor;
            set => ChangedOptions.EmotionForeColor = value;
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
