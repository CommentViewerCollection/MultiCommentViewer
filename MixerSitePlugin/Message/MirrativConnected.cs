using SitePlugin;
using System.Collections.Generic;

namespace MixerSitePlugin
{
    internal class MixerConnected : MessageBase2, IMixerConnected
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MixerMessageType MixerMessageType { get; } = MixerMessageType.Connected;
        public string Text { get; }

        public MixerConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
