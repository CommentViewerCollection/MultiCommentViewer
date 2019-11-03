using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using System.Windows.Media;

namespace WhowatchSitePlugin
{
    enum CommentType
    {
        Normal,
        Item,
    }
    internal class WhowatchCommentViewModel : CommentViewModelBase
    {
        public override MessageType MessageType { get; protected set; }
        public override SolidColorBrush Background
        {
            get
            {
                if (CommentType == CommentType.Item)
                {
                    return new SolidColorBrush(_siteOptions.ItemBackColor);
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
                if (CommentType == CommentType.Item)
                {
                    return new SolidColorBrush(_siteOptions.ItemForeColor);
                }
                else
                {
                    return base.Foreground;
                }
            }
        }
        public CommentType CommentType { get; }
        private readonly IUser _user;
        private readonly IWhowatchSiteOptions _siteOptions;

        public override string UserId => _user.UserId;
        public WhowatchCommentViewModel(Comment comment,Dictionary<long, PlayItem> itemDict, ICommentOptions options, IWhowatchSiteOptions siteOptions, IUser user, ICommentProvider cp, bool isFirstComment)
            : base(options, user, cp, isFirstComment)
        {
            _siteOptions = siteOptions;
            _user = user;
            if(comment.CommentType == "BY_PLAYITEM")
            {
                MessageType = MessageType.BroadcastInfo;
            }
            else
            {
                MessageType = MessageType.Comment;
            }

            if(comment.CommentType == "BY_PLAYITEM")
            {
                CommentType = CommentType.Item;
                if(itemDict.TryGetValue(comment.PlayItemPatternId.Value, out PlayItem item))
                {
                    Info = $"{item.Name}×{comment.ItemCount}";
                }
            }

            Id = comment.Id.ToString();

            var name = comment.User.Name;
            user.Name = new List<IMessagePart> { MessagePartFactory.CreateMessageText(name) };
            var message = comment.Message;

            if (siteOptions.NeedAutoSubNickname)
            {
                var nick = ExtractNickname(message);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            if (user == null || string.IsNullOrEmpty(user.Nickname))
            {
                NameItemsInternal = new List<IMessagePart> { MessagePartFactory.CreateMessageText(name) };
            }
            else
            {
                NameItemsInternal = new List<IMessagePart> { MessagePartFactory.CreateMessageText(user.Nickname) };
            }
            MessageItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(message) };

            var iconUrl = !string.IsNullOrEmpty(comment.User.IconUrl) ? comment.User.IconUrl : DefaultUserIconUrl;
            Thumbnail = new MessageImage { Url = iconUrl };
        }
        public WhowatchCommentViewModel(Comment comment, ICommentOptions options, IWhowatchSiteOptions siteOptions, IUser user, ICommentProvider cp, bool isFirstComment)
            :base(options, user, cp, isFirstComment)
        {
            _siteOptions = siteOptions;
            _user = user;
            Id = comment.Id.ToString();

            var name = comment.User.Name;
            user.Name = new List<IMessagePart> { MessagePartFactory.CreateMessageText(name) };
            var message = comment.Message;

            if (siteOptions.NeedAutoSubNickname)
            {
                var nick = ExtractNickname(message);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            NameItemsInternal = new List<IMessagePart> { MessagePartFactory.CreateMessageText(name) };
            MessageItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(message) };

            var iconUrl = !string.IsNullOrEmpty(comment.User.IconUrl) ? comment.User.IconUrl : DefaultUserIconUrl;
            Thumbnail = new MessageImage { Url = iconUrl };

            Init();
        }
        private const string DefaultUserIconUrl = "https://whowatch.tv/image/commons/icon_prof_default.png";
    }
}
