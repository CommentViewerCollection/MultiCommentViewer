using SitePlugin;
using System.Collections.Generic;

namespace MirrativSitePlugin
{
    internal class MirrativDisconnected : MessageBase, IMirrativDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Mirrativ;
        public MirrativMessageType MirrativMessageType { get; } = MirrativMessageType.Disconnected;
        public MirrativDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("切断しました"),
            };
        }
    }
}
