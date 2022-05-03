using Mcv.PluginV2;

namespace TwicasSitePlugin
{
    internal class TwicasDisconnected : MessageBase2, ITwicasDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Twicas;
        public TwicasMessageType TwicasMessageType { get; } = TwicasMessageType.Disconnected;
        public string Text { get; }

        public TwicasDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
