using SitePlugin;
using System.Collections.Generic;

namespace MildomSitePlugin
{
    internal class MildomDisconnected : MessageBase2, IMildomDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MildomMessageType MildomMessageType { get; } = MildomMessageType.Disconnected;
        public string Text { get; }

        public MildomDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
