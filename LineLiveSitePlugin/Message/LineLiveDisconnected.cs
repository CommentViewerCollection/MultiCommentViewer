using SitePlugin;
using System.Collections.Generic;

namespace LineLiveSitePlugin
{
    internal class LineLiveDisconnected : MessageBase, ILineLiveDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.LineLive;
        public LineLiveMessageType LineLiveMessageType { get; } = LineLiveMessageType.Disconnected;
        public LineLiveDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("切断しました"),
            };
        }
    }
}
