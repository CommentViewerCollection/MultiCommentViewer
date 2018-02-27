using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SitePlugin;
using Common;
namespace NicoSitePlugin.Old
{
    internal class NicoCommentViewModel2 : CommentViewModelBase, INicoCommentViewModel
    {
        //本当はIUserはコンストラクタから入れたい。でもIUserStoreにはサイト毎の実装はいらない（多分）だろうから共通の場所でやってる。
        //NameItemsはそのユーザの全てのコメントに共通のはずだからcvm内ではなくIUserとかに持たせたい
        
        public override string UserId => _chat.UserId;
        private readonly NicoSiteOptions _siteOptions;
        private readonly Chat _chat;
        [Obsolete]
        public NicoCommentViewModel2(Chat chat, ICommentOptions options, NicoSiteOptions siteOptions) 
            : base(options)
        {
            _siteOptions = siteOptions;
            _chat = chat;

            SetName(null);
            MessageItems = new List<IMessagePart> { new MessageText { Text = _chat.Text } };
        }
        public NicoCommentViewModel2(ICommentOptions options, NicoSiteOptions siteOptions, 
            Chat chat, RoomInfo roomInfo, IUser user, ICommentProvider commentProvider, bool isFirstComment)
            :base(options)
        {
            _siteOptions = siteOptions;
            _chat = chat;
            IsFirstComment = isFirstComment;
            CommentProvider = commentProvider;
            SetName(user);
            Id = $"{roomInfo.RoomLabel}:{chat.No}";
            MessageItems = new List<IMessagePart> { new MessageText { Text = _chat.Text } };
        }
        private void SetName(IUser _user)
        {
            if (_user == null || string.IsNullOrEmpty(_user.Nickname))
            {
                NameItems = new List<IMessagePart> { new MessageText { Text = _chat.UserId } };
            }
            else
            {
                NameItems = new List<IMessagePart> { new MessageText { Text = _user.Nickname } };
            }
            RaisePropertyChanged(nameof(NameItems));
        }
    }
    class MessageText : IMessageText
    {
        public string Text { get; set; }
    }
}
