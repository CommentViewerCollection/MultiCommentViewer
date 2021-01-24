using SitePlugin;
using System.Collections.Generic;

namespace NicoSitePlugin
{
    internal class NicoConnected : MessageBase2, INicoConnected
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Connected;
        public string Text { get; }

        public NicoConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
