using SitePlugin;
using System.Collections.Generic;

namespace TwitchSitePlugin
{
    internal class TwitchConnected : MessageBase, ITwitchConnected
    {
        public override SiteType SiteType { get; } = SiteType.Twitch;
        public TwitchMessageType TwitchMessageType { get; } = TwitchMessageType.Connected;

        public TwitchConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("接続しました"),
            };
        }
    }
}
