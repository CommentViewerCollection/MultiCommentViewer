using System;
using System.Collections.Generic;
using SitePlugin;
namespace YouTubeLiveSitePlugin.Test2
{
    class YouTubeLiveCommentViewModel : CommentViewModelBase
    {
        public override string UserId { get; }

        public YouTubeLiveCommentViewModel(ConnectionName connectionName, IOptions options, CommentData commentData, ICommentProvider commentProvider)
            : base(connectionName, options)
        {
            CommentProvider = commentProvider;
            NameItems = commentData.NameItems;
            MessageItems = commentData.MessageItems;
            Id = commentData.Id;
            UserId = commentData.UserId;
            //Timestamp

            //Thumbnail = commentData.Thumbnail;
        }
            public YouTubeLiveCommentViewModel(ConnectionName connectionName, IOptions options, IPaidMessage paidMessage,ICommentProvider commentProvider)
            : base(connectionName, options)
        {
            var m = paidMessage;
            CommentProvider = commentProvider;
            var nameItems = new List<IMessagePart>
            {
                new MessageText(m.Name),
            };
            if (m.Badges.Count > 0)
            {
                foreach (var badge in m.Badges)
                {
                    nameItems.Add(new MessageImage { Url = badge.Url, Alt = badge.Alt, Height = badge.Height, Width = badge.Width });
                }
            }
            NameItems = nameItems;
            MessageItems = new List<IMessagePart>
            {
                new MessageText(m.PurchaseAmount + Environment.NewLine + m.Message),
            };
        }
        public YouTubeLiveCommentViewModel(ConnectionName connectionName, IOptions options, ITextMessage textMessage, ICommentProvider commentProvider)
            : base(connectionName, options)
        {
            var m = textMessage;
            CommentProvider = commentProvider;
            var nameItems = new List<IMessagePart>
            {
                new MessageText(m.Name),
            };
            if (m.Badges.Count > 0)
            {
                foreach (var badge in m.Badges)
                {
                    nameItems.Add(new MessageImage { Url = badge.Url, Alt = badge.Alt, Height = badge.Height, Width = badge.Width });
                }
            }
            NameItems = nameItems;
            MessageItems = new List<IMessagePart>
            {
                new MessageText(m.Message),
            };
        }
    }
}
