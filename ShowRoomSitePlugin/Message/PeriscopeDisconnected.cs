using SitePlugin;
using System.Collections.Generic;

namespace ShowRoomSitePlugin
{
    internal class ShowRoomDisconnected : MessageBase, IShowRoomDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.ShowRoom;
        public ShowRoomMessageType ShowRoomMessageType { get; } = ShowRoomMessageType.Disconnected;
        public ShowRoomDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("切断しました"),
            };
        }
    }
}
