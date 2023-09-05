using Mcv.PluginV2;

namespace WhowatchSitePlugin
{
    internal class WhowatchConnected : MessageBase2, IWhowatchConnected
    {
        public override SiteType SiteType { get; } = SiteType.Whowatch;
        public WhowatchMessageType WhowatchMessageType { get; } = WhowatchMessageType.Connected;
        public string Text { get; }

        public WhowatchConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
