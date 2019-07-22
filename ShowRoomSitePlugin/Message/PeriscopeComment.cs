using SitePlugin;
using System.Collections.Generic;

namespace ShowRoomSitePlugin
{
    internal class ShowRoomComment : MessageBase, IShowRoomComment
    {
        public override SiteType SiteType { get; } = SiteType.ShowRoom;
        public ShowRoomMessageType ShowRoomMessageType { get; } = ShowRoomMessageType.Connected;
        public long CreatedAt { get; }
        public string Id { get; }
        public string UserId { get; }
        public string PostTime { get; }
        public IMessageImage UserIcon { get; set; }

        public ShowRoomComment(T1 t1) : base(t1.Raw)
        {
            NameItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText(t1.Ac),
            };
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText(t1.Cm),
            };
            CreatedAt = t1.CreatedAt;
            UserId = t1.U.ToString();
            Id = "";
        }
    }
}
