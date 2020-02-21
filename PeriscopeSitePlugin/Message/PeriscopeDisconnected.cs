using SitePlugin;
using System.Collections.Generic;

namespace PeriscopeSitePlugin
{
    internal class PeriscopeDisconnected : MessageBase2, IPeriscopeDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Periscope;
        public PeriscopeMessageType PeriscopeMessageType { get; } = PeriscopeMessageType.Disconnected;
        public string Text { get; }

        public PeriscopeDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
}
