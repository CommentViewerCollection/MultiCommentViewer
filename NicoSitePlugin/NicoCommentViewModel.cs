using System.Collections.Generic;
using SitePlugin;
using Common;
using System.Windows.Media;
using NicoSitePlugin.Next20181012;

namespace NicoSitePlugin
{
    internal class NicoCommentViewModel2 : CommentViewModelBase, INicoCommentViewModel
    {
        public override MessageType MessageType { get; protected set; }
        public override SolidColorBrush Background
        {
            get
            {
                if (Abc == UserType.Operator)
                {
                    return new SolidColorBrush(_siteOptions.OperatorBackColor);
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
                if (Abc == UserType.Operator)
                {
                    return new SolidColorBrush(_siteOptions.OperatorForeColor);
                }
                else
                {
                    return base.Foreground;
                }
            }
        }
        public enum UserType
        {
            Viewer,
            Operator,
        }
        //本当はIUserはコンストラクタから入れたい。でもIUserStoreにはサイト毎の実装はいらない（多分）だろうから共通の場所でやってる。
        //NameItemsはそのユーザの全てのコメントに共通のはずだからcvm内ではなくIUserとかに持たせたい

        public override string UserId { get; }
        private readonly INicoSiteOptions _siteOptions;
        private UserType Abc { get; }

        public override bool IsVisible
        {
            get => base.IsVisible;

            protected set
            {
                if (base.IsVisible == value)
                    return;
                base.IsVisible = value;
                RaisePropertyChanged();
            }
        }
        protected override void SetVisibility()
        {
            if (!_siteOptions.IsShow184)
            {
                IsVisible = !Is184 && !User.IsNgUser;
            }
            else
            {
                IsVisible = !User.IsNgUser;
            }
        }
        protected override void NicknameChanged()
        {
            SetNameItems();
        }
        private void SetNameItems()
        {
            if (!string.IsNullOrEmpty(User.Nickname))
            {
                _nicknameItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(User.Nickname) };
            }
            else
            {
                _userIdItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(UserId) };
                _nicknameItems = null;
            }
        }
        public override IEnumerable<IMessagePart> NameItems
        {
            get
            {
                if (!string.IsNullOrEmpty(User.Nickname))
                {
                    return _nicknameItems;
                }
                else
                {
                    return _userIdItems;
                }
            }
        }
        private IEnumerable<IMessagePart> _nicknameItems;
        private IEnumerable<IMessagePart> _userIdItems;
        public NicoCommentViewModel2(ICommentOptions options, INicoSiteOptions siteOptions,
   Chat chat, string roomName, IUser user, ICommentProvider commentProvider, bool isFirstComment)
   : base(options, user, commentProvider, isFirstComment)
        {
            MessageType = MessageType.Comment;
            _siteOptions = siteOptions;
            UserId = chat.UserId;
            Is184 = Tools.Is184UserId(chat.UserId);
            Init();
            if (chat.Premium == 2 || chat.Premium == 3)
            {
                Abc = UserType.Operator;
            }
            else
            {
                Abc = UserType.Viewer;
            }

            if (siteOptions.IsAutoSetNickname)
            {
                var nick = ExtractNickname(chat.Text);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            SetNameItems();

            if (chat.No.HasValue)
            {
                var shortName = Tools.GetShortRoomName(roomName);
                Id = $"{shortName}:{chat.No}";
            }
            else
            {
                Id = roomName;
            }
            
            MessageItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(chat.Text) };
            siteOptions.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(siteOptions.IsShow184):
                        SetVisibility();
                        break;
                }
            };
        }
    }
}
