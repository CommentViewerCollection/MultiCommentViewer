using SitePlugin;
using System.Collections.Generic;

namespace PeriscopeSitePlugin
{
    internal class PeriscopeConnected : MessageBase2, IPeriscopeConnected
    {
        public override SiteType SiteType { get; } = SiteType.Periscope;
        public PeriscopeMessageType PeriscopeMessageType { get; } = PeriscopeMessageType.Connected;
        public string Text { get; }

        public PeriscopeConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
}
