using System.Collections.Generic;
using SitePlugin;
using System.Diagnostics;
namespace TwitchSitePlugin
{
    class TwitchCommentViewModel : CommentViewModelBase
    {
        public override string UserId { get; }
        private readonly IOptions _options;
        private readonly TwitchSiteOptions _siteOptions;
        public TwitchCommentViewModel(ConnectionName connectionName, IOptions options, TwitchSiteOptions siteOptions,
            ICommentData commentData, LowObject.Emoticons emoticons, bool isFirstComment, ICommentProvider commentProvider) : base(connectionName, options)
        {
            Id = commentData.Id;
            UserId = commentData.UserId;
            NameItems = new List<IMessagePart> { new MessageText(commentData.Username) };
            MessageItems = Tools.GetMessageItems(commentData.Message, commentData.Emotes);
            IsFirstComment = isFirstComment;
            CommentProvider = commentProvider;
        }
    }
}
