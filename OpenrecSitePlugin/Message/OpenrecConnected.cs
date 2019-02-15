using SitePlugin;
using System.Collections.Generic;

namespace OpenrecSitePlugin
{
    internal class OpenrecConnected : MessageBase, IOpenrecConnected
    {
        public override SiteType SiteType { get; } = SiteType.Openrec;
        public OpenrecMessageType OpenrecMessageType { get; } = OpenrecMessageType.Connected;

        public OpenrecConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("接続しました"),
            };
        }
    }
}
