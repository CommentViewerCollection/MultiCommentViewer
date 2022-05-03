using Mcv.PluginV2;

namespace OpenrecSitePlugin
{
    internal class OpenrecDisconnected : MessageBase2, IOpenrecDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Openrec;
        public OpenrecMessageType OpenrecMessageType { get; } = OpenrecMessageType.Disconnected;
        public string Text { get; }

        public OpenrecDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
