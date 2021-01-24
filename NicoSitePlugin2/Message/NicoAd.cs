using SitePlugin;
using System;
using System.Collections.Generic;

namespace NicoSitePlugin
{
    internal class NicoAd : MessageBase2, INicoAd
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Ad;
        public string Text { get; set; }
        public DateTime PostedAt { get; set; }
        public string UserId { get; set; }
        public string RoomName { get; set; }
        public NicoAd(string raw) : base(raw)
        {

        }
    }
    internal class NicoItem : MessageBase2, INicoItem
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Item;
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public string Text { get; set; }
        public string RoomName { get; set; }
        public int? ChatNo { get; set; }
        public string ItemName { get; }
        public int ItemCount { get; }

        public NicoItem(string raw) : base(raw)
        {

        }
    }
}
