using Common;
using SitePlugin;
using System;
using System.Collections.Generic;

namespace YouTubeLiveSitePlugin
{
    internal class YouTubeLiveConnected : MessageBase, IYouTubeLiveConnected
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Connected;

        public YouTubeLiveConnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("接続しました"),
            };
        }
    }
    internal class YouTubeLiveDisconnected : MessageBase, IYouTubeLiveDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Disconnected;
        public YouTubeLiveDisconnected(string raw) : base(raw)
        {
            CommentItems = new List<IMessagePart>
            {
                Common.MessagePartFactory.CreateMessageText("切断しました"),
            };
        }
    }
    internal class YouTubeLiveSuperchat : MessageBase, IYouTubeLiveSuperchat
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Superchat;
        //public string Comment { get; set; }
        public string Id { get; set; }
        //public string UserName { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
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
            PostTime = SitePluginCommon.Utils.UnixtimeToDateTime(commentData.TimestampUsec / (1000 * 1000)).ToString("HH:mm:ss");
        }
    }
    internal class YouTubeLiveComment : MessageBase, IYouTubeLiveComment
    {
        public override SiteType SiteType { get; } = SiteType.YouTubeLive;
        public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Comment;
        //public string Comment { get; set; }
        public string Id { get; set; }
        //public string UserName { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public YouTubeLiveComment(Test2.CommentData commentData) : base(commentData.Raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;
            CommentItems = commentData.MessageItems;
            NameItems=commentData.NameItems;
            UserIcon = commentData.Thumbnail;
            PostTime = SitePluginCommon.Utils.UnixtimeToDateTime(commentData.TimestampUsec / (1000 * 1000)).ToString("HH:mm:ss");
        }
    }
}
