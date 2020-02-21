using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixerSitePlugin
{
    internal class MixerComment : MessageBase2, IMixerComment
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MixerMessageType MixerMessageType { get; } = MixerMessageType.Comment;
        public IEnumerable<IMessagePart> CommentItems { get; set; }
        public string Id { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public MixerComment(ChatMessageData commentData, DateTime createdAt) : base(commentData.Raw)
        {
            UserId = commentData.UserId.ToString();
            Id = commentData.Id;
            CommentItems = commentData.MessageItems;
            UserName = commentData.UserName;
            PostedAt = createdAt;
        }
    }
}
