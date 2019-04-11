using SitePlugin;
using System.Collections.Generic;

namespace PeriscopeSitePlugin
{
    internal class PeriscopeJoin : MessageBase, IPeriscopeJoin
    {
        public override SiteType SiteType { get; } = SiteType.Periscope;
        public PeriscopeMessageType PeriscopeMessageType { get; } = PeriscopeMessageType.Join;
        public string UserId { get; }
        public PeriscopeJoin(Kind2Kind1 kind2Kind1) : base(kind2Kind1.Raw)
        {
            UserId = kind2Kind1.UserId;
            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(kind2Kind1.DisplayName) };
            var comment = $"{kind2Kind1.DisplayName} さんが参加しました";
            CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(comment) };
        }
    }
}
