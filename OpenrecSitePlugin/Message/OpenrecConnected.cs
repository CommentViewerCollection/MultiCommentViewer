using SitePlugin;
using System.Collections.Generic;

namespace OpenrecSitePlugin
{
    internal class OpenrecConnected : MessageBase2, IOpenrecConnected
    {
        public override SiteType SiteType { get; } = SiteType.Openrec;
        public OpenrecMessageType OpenrecMessageType { get; } = OpenrecMessageType.Connected;
        public string Text { get; }

        public OpenrecConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
