using SitePlugin;
using System.Collections.Generic;

namespace PeriscopeSitePlugin
{
    internal class PeriscopeLeave : MessageBase2, IPeriscopeLeave
    {
        public override SiteType SiteType { get; } = SiteType.Periscope;
        public PeriscopeMessageType PeriscopeMessageType { get; } = PeriscopeMessageType.Leave;
        public string UserId { get; }
        public string DisplayName { get; }
        public string Text { get; }
        public PeriscopeLeave(Kind2Kind2 kind2Kind2) : base(kind2Kind2.Raw)
        {
            UserId = kind2Kind2.UserId;
            DisplayName = kind2Kind2.DisplayName;
            Text = $"{kind2Kind2.DisplayName} さんが退出しました";
        }
    }
}
