using Mcv.PluginV2;

namespace LineLiveSitePlugin
{
    internal class LineLiveConnected : MessageBase2, ILineLiveConnected
    {
        public override SiteType SiteType { get; } = SiteType.LineLive;
        public LineLiveMessageType LineLiveMessageType { get; } = LineLiveMessageType.Connected;
        public string Text { get; }

        public LineLiveConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
