using SitePlugin;
using System.Collections.Generic;

namespace MirrativSitePlugin
{
    internal class MirrativDisconnected : MessageBase2, IMirrativDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Mirrativ;
        public MirrativMessageType MirrativMessageType { get; } = MirrativMessageType.Disconnected;
        public string Text { get; }

        public MirrativDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
