using SitePlugin;
using System;
using System.Collections.Generic;

namespace ShowRoomSitePlugin
{
    internal class ShowRoomComment : MessageBase2, IShowRoomComment
    {
        public override SiteType SiteType { get; } = SiteType.ShowRoom;
        public ShowRoomMessageType ShowRoomMessageType { get; } = ShowRoomMessageType.Comment;
        public string UserId { get; }
        public DateTime PostedAt { get; }
        public string UserName { get; }
        public string Text { get; }
        public ShowRoomComment(T1 t1) : base(t1.Raw)
        {
            UserName = t1.UserName;
            Text = t1.Comment;
            PostedAt = Common.UnixTimeConverter.FromUnixTime(t1.CreatedAt).ToLocalTime();
            UserId = t1.UserId.ToString();
        }
    }
}
