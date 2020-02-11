using SitePlugin;
using System.Collections.Generic;

namespace MixerSitePlugin
{
    internal class MixerDisconnected : MessageBase2, IMixerDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MixerMessageType MixerMessageType { get; } = MixerMessageType.Disconnected;
        public string Text { get; }

        public MixerDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
