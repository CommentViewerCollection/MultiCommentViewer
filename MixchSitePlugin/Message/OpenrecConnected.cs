using SitePlugin;
using System.Collections.Generic;

namespace MixchSitePlugin
{
    internal class MixchConnected : MessageBase2, IMixchConnected
    {
        public override SiteType SiteType { get; } = SiteType.Mixch;
        public MixchMessageType MixchMessageType { get; } = MixchMessageType.Connected;
        public string Text { get; }

        public MixchConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
