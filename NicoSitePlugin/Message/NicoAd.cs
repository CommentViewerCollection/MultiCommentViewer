using SitePlugin;

namespace NicoSitePlugin
{
    internal class NicoAd : MessageBase, INicoAd
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Ad;
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public bool Is184 { get; set; }
        public string RoomName { get; set; }
        public int? ChatNo { get; set; }

        public NicoAd(string raw) : base(raw)
        {

        }
    }
}
