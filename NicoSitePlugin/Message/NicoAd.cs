using SitePlugin;

namespace NicoSitePlugin
{
    internal class NicoAd : NicoMessageBase, INicoAd
    {
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Ad;
        public int? ChatNo { get; set; }
        public NicoAd(string raw, INicoSiteOptions siteOptions) : base(raw, siteOptions)
        {

        }
    }
    internal class NicoItem : NicoMessageBase, INicoItem
    {
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Item;
        public int? ChatNo { get; set; }
        public string ItemName { get; }
        public int ItemCount { get; }

        public NicoItem(string raw, INicoSiteOptions siteOptions) : base(raw, siteOptions)
        {

        }
    }
}
