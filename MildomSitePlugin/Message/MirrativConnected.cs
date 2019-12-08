using SitePlugin;
using System.Collections.Generic;

namespace MildomSitePlugin
{
    internal class MildomConnected : MessageBase, IMildomConnected
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MildomMessageType MildomMessageType { get; } = MildomMessageType.Connected;

        public MildomConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("配信に接続しました"),
            };
        }
    }
}
