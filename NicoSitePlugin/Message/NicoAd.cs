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
}
