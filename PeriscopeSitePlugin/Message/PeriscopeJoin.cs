using SitePlugin;
using System.Collections.Generic;

namespace PeriscopeSitePlugin
{
    internal class PeriscopeJoin : MessageBase2, IPeriscopeJoin
    {
        public override SiteType SiteType { get; } = SiteType.Periscope;
        public PeriscopeMessageType PeriscopeMessageType { get; } = PeriscopeMessageType.Join;
        public string UserId { get; }
        public string DisplayName { get; }
        public string Text { get; }
        public PeriscopeJoin(Kind2Kind1 kind2Kind1) : base(kind2Kind1.Raw)
        {
            UserId = kind2Kind1.UserId;
            DisplayName = kind2Kind1.DisplayName;
            Text = $"{kind2Kind1.DisplayName} さんが参加しました";
        }
    }
}
