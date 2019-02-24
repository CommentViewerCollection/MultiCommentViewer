using SitePlugin;
using System.Collections.Generic;

namespace OpenrecSitePlugin
{
    internal class OpenrecDisconnected : MessageBase, IOpenrecDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Openrec;
        public OpenrecMessageType OpenrecMessageType { get; } = OpenrecMessageType.Disconnected;
        public OpenrecDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("切断しました"),
            };
        }
    }
}
