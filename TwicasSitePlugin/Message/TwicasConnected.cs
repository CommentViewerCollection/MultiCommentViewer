using SitePlugin;
using System.Collections.Generic;

namespace TwicasSitePlugin
{
    internal class TwicasConnected : MessageBase, ITwicasConnected
    {
        public override SiteType SiteType { get; } = SiteType.Twicas;
        public TwicasMessageType TwicasMessageType { get; } = TwicasMessageType.Connected;

        public TwicasConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("接続しました"),
            };
        }
    }
}
