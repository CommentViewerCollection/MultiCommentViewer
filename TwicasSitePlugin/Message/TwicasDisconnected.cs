using SitePlugin;
using System.Collections.Generic;

namespace TwicasSitePlugin
{
    internal class TwicasDisconnected : MessageBase, ITwicasDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Twicas;
        public TwicasMessageType TwicasMessageType { get; } = TwicasMessageType.Disconnected;
        public TwicasDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("切断しました"),
            };
        }
    }
}
