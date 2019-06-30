//using SitePlugin;
//using System.Collections.Generic;

//namespace ShowRoomSitePlugin
//{
//    internal class ShowRoomJoin : MessageBase, IShowRoomJoin
//    {
//        public override SiteType SiteType { get; } = SiteType.ShowRoom;
//        public ShowRoomMessageType ShowRoomMessageType { get; } = ShowRoomMessageType.Join;
//        public string UserId { get; }
//        public ShowRoomJoin(Kind2Kind1 kind2Kind1) : base(kind2Kind1.Raw)
//        {
//            UserId = kind2Kind1.UserId;
//            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(kind2Kind1.DisplayName) };
//            var comment = $"{kind2Kind1.DisplayName} さんが参加しました";
//            CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(comment) };
//        }
//    }
//}
