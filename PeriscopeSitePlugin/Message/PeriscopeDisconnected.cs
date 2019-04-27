using SitePlugin;
using System.Collections.Generic;

namespace PeriscopeSitePlugin
{
    internal class PeriscopeDisconnected : MessageBase, IPeriscopeDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Periscope;
        public PeriscopeMessageType PeriscopeMessageType { get; } = PeriscopeMessageType.Disconnected;
        public PeriscopeDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("切断しました"),
            };
        }
    }
}
