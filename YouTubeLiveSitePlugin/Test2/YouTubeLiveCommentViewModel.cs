using System;
using System.Collections.Generic;
using SitePlugin;
using Common;
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
    }
}
