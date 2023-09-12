using Mcv.PluginV2;

namespace LineLiveSitePlugin
{
    internal class LineLiveDisconnected : MessageBase2, ILineLiveDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.LineLive;
        public LineLiveMessageType LineLiveMessageType { get; } = LineLiveMessageType.Disconnected;
        public string Text { get; }

        public LineLiveDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
