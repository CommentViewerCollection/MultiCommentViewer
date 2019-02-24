using SitePlugin;
using System.Collections.Generic;

namespace NicoSitePlugin
{
    internal class NicoDisconnected : MessageBase, INicoDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Disconnected;
        public NicoDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("切断しました"),
            };
        }
    }
}
