using Mcv.PluginV2;

namespace MirrativSitePlugin
{
    internal class MirrativConnected : MessageBase2, IMirrativConnected
    {
        public override SiteType SiteType { get; } = SiteType.Mirrativ;
        public MirrativMessageType MirrativMessageType { get; } = MirrativMessageType.Connected;
        public string Text { get; }

        public MirrativConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
