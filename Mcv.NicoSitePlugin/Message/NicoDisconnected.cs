using Mcv.PluginV2;

namespace NicoSitePlugin
{
    internal class NicoDisconnected : MessageBase2, INicoDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Disconnected;
        public string Text { get; }

        public NicoDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
