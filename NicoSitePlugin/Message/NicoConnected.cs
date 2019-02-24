using SitePlugin;
using System.Collections.Generic;

namespace NicoSitePlugin
{
    internal class NicoConnected : MessageBase, INicoConnected
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Connected;

        public NicoConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("接続しました"),
            };
        }
    }
}
