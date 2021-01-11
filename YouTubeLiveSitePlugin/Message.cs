using Common;
using SitePlugin;
using System;
using System.Collections.Generic;

namespace YouTubeLiveSitePlugin
{
    internal class YouTubeLiveConnected : MessageBase2, IYouTubeLiveConnected
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Connected;
        public string Text { get; }
        public YouTubeLiveConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
    internal class YouTubeLiveDisconnected : MessageBase2, IYouTubeLiveDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Disconnected;
        public string Text { get; }
        public YouTubeLiveDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
    internal class YouTubeLiveMembership : MessageBase2, IYouTubeLiveMembership
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Membership;
        //public string Comment { get; set; }
        public string Id { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IEnumerable<IMessagePart> CommentItems { get; set; }
        //public string UserName { get; set; }
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public IMessageImage UserIcon { get; set; }
        public YouTubeLiveMembership(Next.InternalMembership comment) : base("")
        {
            UserId = comment.UserId;
            Id = comment.Id;
            CommentItems = comment.MessageItems;
            NameItems = comment.NameItems;
            UserIcon = new Common.MessageImage
            {
                Height = comment.ThumbnailHeight,
                Width = comment.ThumbnailWidth,
                Url = comment.ThumbnailUrl,
            };
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(comment.TimestampUsec / (1000 * 1000));
        }
    }
    internal class YouTubeLiveSuperchat : MessageBase2, IYouTubeLiveSuperchat
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Superchat;
        //public string Comment { get; set; }
        public string Id { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IEnumerable<IMessagePart> CommentItems { get; set; }
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public IMessageImage UserIcon { get; set; }
        public string PurchaseAmount { get; }
        public YouTubeLiveSuperchat(Test2.CommentData commentData) : base(commentData.Raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;

            var list = new List<IMessagePart>();
            var s = commentData.PurchaseAmount;
            if (commentData.MessageItems.Count > 0)
                s += Environment.NewLine;
            list.Add(MessagePartFactory.CreateMessageText(s));
            list.AddRange(commentData.MessageItems);
            CommentItems = list;

            NameItems = commentData.NameItems;
            PurchaseAmount = commentData.PurchaseAmount;
            UserIcon = commentData.Thumbnail;
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(commentData.TimestampUsec / (1000 * 1000));
        }
        public YouTubeLiveSuperchat(Next.InternalSuperChat comment) : base("")
        {
            UserId = comment.UserId;
            Id = comment.Id;
            CommentItems = comment.MessageItems;
            NameItems = comment.NameItems;
            UserIcon = new Common.MessageImage
            {
                Height = comment.ThumbnailHeight,
                Width = comment.ThumbnailWidth,
                Url = comment.ThumbnailUrl,
            };
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(comment.TimestampUsec / (1000 * 1000));
            PurchaseAmount = comment.PurchaseAmount;
        }
    }
    internal class YouTubeLiveComment : MessageBase2, IYouTubeLiveComment
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Comment;
        //public string Comment { get; set; }
        public string Id { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IEnumerable<IMessagePart> CommentItems { get; set; }
        //public string UserName { get; set; }
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public IMessageImage UserIcon { get; set; }
        public YouTubeLiveComment(Test2.CommentData commentData) : base(commentData.Raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;
            CommentItems = commentData.MessageItems;
            NameItems = commentData.NameItems;
            UserIcon = commentData.Thumbnail;
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(commentData.TimestampUsec / (1000 * 1000));
        }
        public YouTubeLiveComment(Next.InternalComment comment) : base("")
        {
            UserId = comment.UserId;
            Id = comment.Id;
            CommentItems = comment.MessageItems;
            NameItems = comment.NameItems;
            UserIcon = new Common.MessageImage
            {
                Height = comment.ThumbnailHeight,
                Width = comment.ThumbnailWidth,
                Url = comment.ThumbnailUrl,
            };
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(comment.TimestampUsec / (1000 * 1000));
        }
    }
}
