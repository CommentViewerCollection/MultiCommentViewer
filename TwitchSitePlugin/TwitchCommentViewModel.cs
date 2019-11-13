using System.Collections.Generic;
using SitePlugin;
using System.Diagnostics;
using Common;
namespace TwitchSitePlugin
{
    class TwitchCommentViewModel : CommentViewModelBase
    {
        public override MessageType MessageType { get; protected set; }
        public override string UserId { get; }
        public string DisplayName { get; }
        private readonly ITwitchSiteOptions _siteOptions;
        public TwitchCommentViewModel(ICommentOptions options, ITwitchSiteOptions siteOptions,
            ICommentData commentData, bool isFirstComment, ICommentProvider commentProvider, IUser user)
            : base(options,user,commentProvider,isFirstComment)
        {
            MessageType = MessageType.Comment;
            _siteOptions = siteOptions;
            Id = commentData.Id;
            UserId = commentData.UserId;
            DisplayName = commentData.DisplayName;
            PostTime = commentData.SentAt.ToString("HH:mm:ss");

            var name = commentData.Username;
            user.Name = new List<IMessagePart> { MessagePartFactory.CreateMessageText(name) };
            var message = commentData.Message;

            if (siteOptions.NeedAutoSubNickname)
            {
                var nick = ExtractNickname(message);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            NameItemsInternal = new List<IMessagePart> { MessagePartFactory.CreateMessageText(name) };
            MessageItems = Tools.GetMessageItems(commentData.Message, commentData.Emotes);

            Init();
        }
    }
}
