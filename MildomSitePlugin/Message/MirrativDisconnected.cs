using SitePlugin;
using System.Collections.Generic;

namespace MildomSitePlugin
{
    internal class MildomDisconnected : MessageBase, IMildomDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MildomMessageType MildomMessageType { get; } = MildomMessageType.Disconnected;
        public MildomDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("配信が終了しました"),
            };
        }
    }
}
