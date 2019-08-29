using Common;
using Common.Wpf;
using SitePlugin;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MixerSitePlugin
{
    class MixerCommentViewModel : CommentViewModelBase
    {
        public override MessageType MessageType { get; protected set; }
        public override SolidColorBrush Background
        {
            get
            {
                //if (MessageType == MessageType.BroadcastInfo)
                //{
                //    return new SolidColorBrush(Colors.Black);
                //}
                //else
                //{
                    return base.Background;
                //}
            }
        }
        public override SolidColorBrush Foreground
        {
            get
            {
                if (MessageType == MessageType.BroadcastInfo)
                {
                    return new SolidColorBrush(Colors.Red);
                }
                else
                {
                    return base.Foreground;
                }
            }
        }
        public override string UserId { get; }
        private readonly IMixerSiteOptions _siteOptions;
        public MixerCommentViewModel(ICommentOptions options, IMixerSiteOptions siteOptions, Message message, bool isFirstComment, ICommentProvider commentProvider, IUser user)
            : base(options, user, commentProvider, isFirstComment)
        {
            MessageType = message.Type;
            _siteOptions = siteOptions;
            Id = message.Id;
            UserId = message.UserId;
            PostTime = Tools.UnixTime2DateTime(message.CreatedAt).ToString("HH:mm:ss");

            var name = new List<IMessagePart> { MessagePartFactory.CreateMessageText(message.Username) };
            user.Name = name;
            var comment = message.Comment;

            if (siteOptions.NeedAutoSubNickname && message.Type == MessageType.Comment)
            {
                var nick = ExtractNickname(comment);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            NameItemsInternal = name;
            MessageItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(comment) };

            Init();
        }
    }
}
