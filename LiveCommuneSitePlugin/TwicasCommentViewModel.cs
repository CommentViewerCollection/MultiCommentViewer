using Common;
using SitePlugin;
using System.Collections.Generic;

namespace LiveCommuneSitePlugin
{
    class TwicasCommentViewModel : CommentViewModelBase
    {
        public override string UserId { get; }
        public TwicasCommentViewModel(ICommentOptions options, ICommentData data, IUser user) :
            base(options)
        {
            UserId = data.UserId;
            Id = data.Id.ToString();
            NameItems = new List<IMessagePart> { new MessageText(data.Name) };
            MessageItems = data.Message;
            Thumbnail = new MessageImage { Url = data.ThumbnailUrl, Height = data.ThumbnailHeight, Width = data.ThumbnailWidth };
            //User = user;
        }
    }
}
