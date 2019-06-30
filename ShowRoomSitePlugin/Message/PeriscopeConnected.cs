using SitePlugin;
using System.Collections.Generic;

namespace ShowRoomSitePlugin
{
    internal class ShowRoomConnected : MessageBase, IShowRoomConnected
    {
        public override SiteType SiteType { get; } = SiteType.ShowRoom;
        public ShowRoomMessageType ShowRoomMessageType { get; } = ShowRoomMessageType.Connected;

        public ShowRoomConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("接続しました"),
            };
        }
    }
}
