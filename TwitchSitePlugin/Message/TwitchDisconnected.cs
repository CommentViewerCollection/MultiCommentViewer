using SitePlugin;
using System.Collections.Generic;

namespace TwitchSitePlugin
{
    internal class TwitchDisconnected : MessageBase2, ITwitchDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Twitch;
        public TwitchMessageType TwitchMessageType { get; } = TwitchMessageType.Disconnected;
        public string Text { get; }

        public TwitchDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
