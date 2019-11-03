using SitePlugin;
using System.Collections.Generic;

namespace MirrativSitePlugin
{
    internal class MirrativConnected : MessageBase, IMirrativConnected
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MirrativMessageType MirrativMessageType { get; } = MirrativMessageType.Connected;

        public MirrativConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("配信に接続しました"),
            };
        }
    }
}
