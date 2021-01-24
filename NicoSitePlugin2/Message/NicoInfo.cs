using SitePlugin;
using System;

namespace NicoSitePlugin
{
    internal class NicoInfo : MessageBase2, INicoInfo
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Info;
        public string Text { get; set; }
        public DateTime PostedAt { get; set; }
        public string UserId { get; set; }
        public string RoomName { get; set; }
        public int No { get; set; }
        public NicoInfo(string raw) : base(raw)
        {
        }
    }
}
