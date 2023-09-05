using Mcv.PluginV2;

namespace TwitchSitePlugin
{
    internal class TwitchConnected : MessageBase2, ITwitchConnected
    {
        public override SiteType SiteType { get; } = SiteType.Twitch;
        public TwitchMessageType TwitchMessageType { get; } = TwitchMessageType.Connected;
        public string Text { get; }

        public TwitchConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
