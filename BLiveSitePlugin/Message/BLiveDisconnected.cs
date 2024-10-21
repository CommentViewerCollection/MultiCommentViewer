using SitePlugin;
using System.Collections.Generic;

namespace BLiveSitePlugin
{
    internal class BLiveDisconnected : MessageBase2, IBLiveDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.BLive;
        public BLiveMessageType BLiveMessageType { get; } = BLiveMessageType.Disconnected;
        public string Text { get; }

        public BLiveDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
