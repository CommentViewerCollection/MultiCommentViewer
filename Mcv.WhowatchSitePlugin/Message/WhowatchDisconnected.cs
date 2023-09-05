using Mcv.PluginV2;
using System.Collections.Generic;

namespace WhowatchSitePlugin
{
    internal class WhowatchDisconnected : MessageBase2, IWhowatchDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Whowatch;
        public WhowatchMessageType WhowatchMessageType { get; } = WhowatchMessageType.Disconnected;
        public string Text { get; }

        public WhowatchDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
