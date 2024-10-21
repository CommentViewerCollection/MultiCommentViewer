using SitePlugin;
using System.Collections.Generic;

namespace BLiveSitePlugin
{
    internal class BLiveConnected : MessageBase2, IBLiveConnected
    {
        public override SiteType SiteType { get; } = SiteType.BLive;
        public BLiveMessageType BLiveMessageType { get; } = BLiveMessageType.Connected;
        public string Text { get; }

        public BLiveConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
