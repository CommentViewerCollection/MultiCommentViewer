using SitePlugin;
using System.Collections.Generic;

namespace TwitchSitePlugin
{
    internal class TwitchDisconnected : MessageBase, ITwitchDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Twitch;
        public TwitchMessageType TwitchMessageType { get; } = TwitchMessageType.Disconnected;
        public TwitchDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("切断しました"),
            };
        }
    }
}
