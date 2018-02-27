using System.Collections.Generic;
using SitePlugin;
using System.Diagnostics;
using Common;
namespace TwitchSitePlugin
{
    class TwitchCommentViewModel : CommentViewModelBase
    {
        public override string UserId { get; }
        private readonly TwitchSiteOptions _siteOptions;
        public TwitchCommentViewModel(IOptions options, TwitchSiteOptions siteOptions,
            ICommentData commentData, LowObject.Emoticons emoticons, bool isFirstComment, ICommentProvider commentProvider, IUser user) : base(options)
        {
            _siteOptions = siteOptions;
            Id = commentData.Id;
            UserId = commentData.UserId;
            NameItems = new List<IMessagePart> { new MessageText(commentData.Username) };
            MessageItems = Tools.GetMessageItems(commentData.Message, commentData.Emotes);
            IsFirstComment = isFirstComment;
            CommentProvider = commentProvider;
            User = user;
        }
    }
}
