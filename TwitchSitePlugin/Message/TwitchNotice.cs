using SitePlugin;

namespace TwitchSitePlugin
{
    internal class TwitchNotice : MessageBase2, ITwitchNotice
    {
        public override SiteType SiteType { get; } = SiteType.Twitch;
        public TwitchMessageType TwitchMessageType { get; } = TwitchMessageType.Notice;
        public string Message { get; set; }

        public TwitchNotice(string raw) : base(raw)
        {
        }
    }
}
