using SitePlugin;
using System.Collections.Generic;

namespace MixchSitePlugin
{
    internal class MixchDisconnected : MessageBase2, IMixchDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Mixch;
        public MixchMessageType MixchMessageType { get; } = MixchMessageType.Disconnected;
        public string Text { get; }

        public MixchDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
