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
    internal class NicoSpi : MessageBase2, INicoSpi
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Spi;
        public string Text { get; set; }
        public DateTime PostedAt { get; set; }
        public string UserId { get; set; }
        public NicoSpi(string raw) : base(raw)
        {

        }
    }
    internal class NicoGift : MessageBase2, INicoGift
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Item;
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public string Text { get; set; }
        public string RoomName { get; set; }
        public int? ChatNo { get; set; }
        public string ItemName { get; set; }
        public int ItemCount { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public NicoGift(string raw) : base(raw)
        {

        }
    }
    internal class NicoEmotion : MessageBase2, INicoEmotion
    {
        public NicoEmotion(string raw) : base(raw)
        {

        }

        public string Content { get; set; }
        public DateTime PostedAt { get; set; }
        public int? ChatNo { get; set; }
        public int Vpos { get; set; }
        public string UserId { get; set; }
        public int Premium { get; set; }
        public int Anonymity { get; set; }
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Emotion;
        public override SiteType SiteType { get; } = SiteType.NicoLive;
    }
}
