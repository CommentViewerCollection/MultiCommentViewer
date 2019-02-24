using System;
using System.Collections.Generic;
using SitePlugin;
using Common;
using System.Windows.Media;

namespace YouTubeLiveSitePlugin.Test2
{
    class YouTubeLiveCommentViewModel : CommentViewModelBase
    {
        public override MessageType MessageType { get; protected set; }
        private readonly IYouTubeLiveSiteOptions _siteOptions;

        bool IsPaidMessage { get; }
        public override string UserId { get; }
        public override SolidColorBrush Background
        {
            get
            {
                if (IsPaidMessage)
                {
                    return new SolidColorBrush(_siteOptions.PaidCommentBackColor);
                }
                else
                {
                    return base.Background;
                }
            }
        }
        public override SolidColorBrush Foreground
        {
            get
            {
                if (IsPaidMessage)
                {
                    return new SolidColorBrush(_siteOptions.PaidCommentForeColor);
                }
                else
                {
                    return base.Foreground;
                }
            }
        }
        public YouTubeLiveCommentViewModel(ICommentOptions options, IYouTubeLiveSiteOptions siteOptions, CommentData commentData, ICommentProvider commentProvider, bool isFirstComment,IUser user)
            : base(options, user, commentProvider, isFirstComment)
        {
            var messageText = commentData.MessageItems.ToText();
            IsPaidMessage = commentData.IsPaidMessage;
            if (siteOptions.IsAutoSetNickname)
            {
                var nick = ExtractNickname(messageText);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            NameItemsInternal = commentData.NameItems;
            user.Name = commentData.NameItems;
            if (commentData.IsPaidMessage)
            {
                var list = new List<IMessagePart>();
                var s = commentData.PurchaseAmount;
                if (commentData.MessageItems.Count > 0)
                    s += Environment.NewLine;
                list.Add(MessagePartFactory.CreateMessageText(s));
                list.AddRange(commentData.MessageItems);
                MessageItems = list;
                MessageType = MessageType.BroadcastInfo;
            }
            else
            {
                MessageItems = commentData.MessageItems;
                MessageType = MessageType.Comment;
            }
            Id = commentData.Id;
            UserId = commentData.UserId;
            Thumbnail = commentData.Thumbnail;
            PostTime = Tools.ParseTimestampUsec(commentData.TimestampUsec).ToLocalTime().ToString("HH:mm:ss");
            _siteOptions = siteOptions;

            Init();
        }
    }
}
