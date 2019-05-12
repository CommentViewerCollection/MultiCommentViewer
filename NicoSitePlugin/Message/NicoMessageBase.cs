using SitePlugin;
using System;
using System.Collections.Generic;

namespace NicoSitePlugin
{
    internal class NicoMessageBase : MessageBase
    {
        private readonly INicoSiteOptions _siteOptions;
        private string _userId;
        private bool _is184;
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public string UserId
        {
            get
            {
                return _userId;
            }
            set
            {
                _userId = value;
                Set184Name();
                OnValueChanged();
            }
        }
        public string PostTime { get; set; }
        public bool Is184
        {
            get
            {
                return _is184;
            }
            set
            {
                if (_is184 == value) return;

                _is184 = value;
                Set184Name();
                OnValueChanged();
            }
        }
        public string RoomName { get; set; }
        public NicoMessageBase(string raw, INicoSiteOptions siteOptions) : base(raw)
        {
            if (siteOptions == null) throw new ArgumentNullException(nameof(siteOptions));
            _siteOptions = siteOptions;
            siteOptions.PropertyChanged += SiteOptions_PropertyChanged;
        }
        private void Set184Name()
        {
            if (Is184)
            {
                if (_siteOptions.IsShow184Id)
                {
                    NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(UserId) };
                }
                else
                {
                    NameItems = null;
                }
            }
        }
        private void SiteOptions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_siteOptions.IsShow184Id):
                    Set184Name();
                    break;
            }
        }
    }
}
