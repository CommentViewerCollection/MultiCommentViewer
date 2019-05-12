using SitePlugin;
using System.Collections.Generic;

namespace WhowatchSitePlugin
{
    internal class WhowatchDisconnected : MessageBase, IWhowatchDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Whowatch;
        public WhowatchMessageType WhowatchMessageType { get; } = WhowatchMessageType.Disconnected;
        public WhowatchDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("切断しました"),
            };
        }
    }
}
