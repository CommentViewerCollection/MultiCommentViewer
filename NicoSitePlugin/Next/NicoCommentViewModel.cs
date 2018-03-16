using Common;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePlugin.Next
{
    interface INicoCommentViewModel:ICommentViewModel
    {

    }
    internal class NicoCommentViewModel : CommentViewModelBase, INicoCommentViewModel
    {
        //本当はIUserはコンストラクタから入れたい。でもIUserStoreにはサイト毎の実装はいらない（多分）だろうから共通の場所でやってる。
        //NameItemsはそのユーザの全てのコメントに共通のはずだからcvm内ではなくIUserとかに持たせたい

        public override string UserId => _chat.UserId;
        private readonly INicoSiteOptions _siteOptions;
        private readonly Chat _chat;
        public NicoCommentViewModel(ICommentOptions options, INicoSiteOptions siteOptions,
            Chat chat, IXmlWsRoomInfo roomInfo, IUser user, ICommentProvider commentProvider, bool isFirstComment)
            : base(options)
        {
            Debug.Assert(user != null);
            _siteOptions = siteOptions;
            _chat = chat;
            //User = user;
            IsFirstComment = isFirstComment;
            CommentProvider = commentProvider;
            SetName(user);
            if (chat.No.HasValue)
            {
                var shortName = Tools.GetShortRoomName(roomInfo.Name);
                Id = $"{shortName}:{chat.No}";
            }
            else
            {
                Id = roomInfo.Name;
            }
            MessageItems = new List<IMessagePart> { new MessageText(_chat.Text) };
        }
        private void SetName(IUser _user)
        {
            if (_user == null || string.IsNullOrEmpty(_user.Nickname))
            {
                NameItems = new List<IMessagePart> { new MessageText(_chat.UserId) };
            }
            else
            {
                NameItems = new List<IMessagePart> { new MessageText(_user.Nickname) };
            }
            RaisePropertyChanged(nameof(NameItems));
        }
    }
}
