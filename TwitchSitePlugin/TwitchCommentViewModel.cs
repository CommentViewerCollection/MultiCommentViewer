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
            ICommentData commentData, LowObject.Emoticons emoticons, bool isFirstComment) : base(connectionName, options)
        {
            Id = commentData.Id;
            UserId = commentData.UserId;
            NameItems = new List<IMessagePart> { new MessageText(commentData.Username) };
            MessageItems = Tools.GetMessageItems(commentData.Message, commentData.Emotes);
            IsFirstComment = isFirstComment;
        }
        public TwitchCommentViewModel(ConnectionName connectionName, IOptions options, TwitchSiteOptions siteOptions,
            Result result, LowObject.Emoticons emoticons, bool isFirstComment) : base(connectionName, options)
        {
            _options = options;
            _siteOptions = siteOptions;
            IsFirstComment = isFirstComment;

            Debug.Assert(result.Command == "PRIVMSG");
            if (!result.Tags.TryGetValue("username", out string name))
            {
                if (!result.Tags.TryGetValue("login", out name))
                {
                    name = result.Prefix.Split('!')[0];
                }
            }
            NameItems = new List<IMessagePart> { new MessageText(name) };

            Debug.Assert(result.Params.Count >= 2);
            var message = result.Params[1];
            MessageItems = Tools.GetMessageItems(result);





            if(result.Tags.TryGetValue("user-id", out string userId))
            {
                UserId = userId;
            }
        }
    }
}
