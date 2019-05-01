using SitePlugin;

namespace NicoSitePlugin
{
    internal class NicoInfo: NicoMessageBase, INicoInfo
    {
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Info;

        public string Id { get; set; }
        public int No { get; set; }
        public IMessageImage UserIcon { get; set; }
        public NicoInfo(string raw, INicoSiteOptions siteOptions) : base(raw, siteOptions)
        {
        }
    }
}
