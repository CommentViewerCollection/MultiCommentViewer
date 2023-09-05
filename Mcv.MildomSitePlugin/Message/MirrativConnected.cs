using Mcv.PluginV2;

namespace MildomSitePlugin
{
    internal class MildomConnected : MessageBase2, IMildomConnected
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MildomMessageType MildomMessageType { get; } = MildomMessageType.Connected;
        public string Text { get; }

        public MildomConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
