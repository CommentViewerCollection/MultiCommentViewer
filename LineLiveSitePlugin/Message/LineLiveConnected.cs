using SitePlugin;
using System.Collections.Generic;

namespace LineLiveSitePlugin
{
    internal class LineLiveConnected : MessageBase, ILineLiveConnected
    {
        public override SiteType SiteType { get; } = SiteType.LineLive;
        public LineLiveMessageType LineLiveMessageType { get; } = LineLiveMessageType.Connected;

        public LineLiveConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("接続しました"),
            };
        }
    }
}
