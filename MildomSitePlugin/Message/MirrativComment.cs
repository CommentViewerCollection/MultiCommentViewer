using SitePlugin;
using System;
using System.Collections.Generic;

namespace MildomSitePlugin
{
    internal class MildomComment : MessageBase2, IMildomComment
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MildomMessageType MildomMessageType { get; } = MildomMessageType.Comment;
        public IEnumerable<IMessagePart> CommentItems { get; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public MildomComment(OnChatMessage chat, string raw) : base(raw)
        {
            UserId = chat.UserId.ToString();
            CommentItems = chat.MessageItems;
            UserName = chat.UserName;
            PostedAt = chat.PostedAt;
        }
    }
}
