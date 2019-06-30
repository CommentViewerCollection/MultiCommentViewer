//using SitePlugin;
//using System.Collections.Generic;

//namespace ShowRoomSitePlugin
//{
//    internal class ShowRoomLeave : MessageBase, IShowRoomLeave
//    {
//        public override SiteType SiteType { get; } = SiteType.ShowRoom;
//        public ShowRoomMessageType ShowRoomMessageType { get; } = ShowRoomMessageType.Leave;
//        public string UserId { get; }
//        public ShowRoomLeave(Kind2Kind2 kind2Kind2) : base(kind2Kind2.Raw)
//        {
//            UserId = kind2Kind2.UserId;
//            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(kind2Kind2.DisplayName) };
//            var comment = $"{kind2Kind2.DisplayName} さんが退出しました";
//            CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(comment) };
//        }
//    }
//}
