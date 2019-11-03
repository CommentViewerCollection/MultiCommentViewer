using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixerSitePlugin
{
    internal class MixerComment : MessageBase, IMixerComment
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MixerMessageType MixerMessageType { get; } = MixerMessageType.Comment;
        //public string Comment { get; set; }
        public string Id { get; set; }
        //public string UserName { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public MixerComment(ChatMessageData commentData, DateTime createdAt) : base(commentData.Raw)
        {
            UserId = commentData.UserId.ToString();
            Id = commentData.Id;
            CommentItems = commentData.MessageItems.ToList();
            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.UserName) };
            UserIcon = null;
            PostTime = createdAt.ToString("HH:mm:ss");
        }
    }
}
