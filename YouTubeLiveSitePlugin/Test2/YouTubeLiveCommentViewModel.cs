using System;
using System.Collections.Generic;
using SitePlugin;
using Common;
using System.Windows.Media;

namespace YouTubeLiveSitePlugin.Test2
{
    class YouTubeLiveCommentViewModel : CommentViewModelBase
    {
        bool IsPaidMessage { get; }
        public override string UserId { get; }
        public override SolidColorBrush Background
        {
            get
            {
                return base.Background;
            }
        }
        public YouTubeLiveCommentViewModel(ConnectionName connectionName, IOptions options, CommentData commentData, ICommentProvider commentProvider)
            : base(connectionName, options)
        {
            CommentProvider = commentProvider;
            NameItems = commentData.NameItems;
            if (commentData.IsPaidMessage)
            {
                var list = new List<IMessagePart>();
                list.Add(new MessageText(commentData.PurchaseAmount + Environment.NewLine));
                list.AddRange(commentData.MessageItems);
            }
            else
            {
                MessageItems = commentData.MessageItems;
            }
            Id = commentData.Id;
            UserId = commentData.UserId;
            Thumbnail = commentData.Thumbnail;
        }
    }
}
