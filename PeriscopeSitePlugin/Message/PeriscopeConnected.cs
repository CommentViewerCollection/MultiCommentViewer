using SitePlugin;
using System.Collections.Generic;

namespace PeriscopeSitePlugin
{
    internal class PeriscopeConnected : MessageBase, IPeriscopeConnected
    {
        public override SiteType SiteType { get; } = SiteType.Periscope;
        public PeriscopeMessageType PeriscopeMessageType { get; } = PeriscopeMessageType.Connected;

        public PeriscopeConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("接続しました"),
            };
        }
    }
}
