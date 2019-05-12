using SitePlugin;
using System.Collections.Generic;

namespace WhowatchSitePlugin
{
    internal class WhowatchConnected : MessageBase, IWhowatchConnected
    {
        public override SiteType SiteType { get; } = SiteType.Whowatch;
        public WhowatchMessageType WhowatchMessageType { get; } = WhowatchMessageType.Connected;

        public WhowatchConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("接続しました"),
            };
        }
    }
}
