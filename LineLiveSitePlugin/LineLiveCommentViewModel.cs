using Common;
using SitePlugin;
using System.Collections.Generic;
using System.Windows.Media;

namespace LineLiveSitePlugin
{
    class LineLiveCommentViewModel : CommentViewModelBase
    {
        public override MessageType MessageType { get; protected set; }
        public override string UserId { get; }
        private readonly ILineLiveSiteOptions _siteOptions;
        //ParseMessage.IMessage _message;
        //     public LineLiveCommentViewModel(ICommentOptions options, ILineLiveSiteOptions siteOptions, ParseMessage.ILove data, ParseMessage.IUser sender, IUser user, ICommentProvider cp) :
        //         base(options, user, cp, false)
        //     {
        //         MessageType = MessageType.BroadcastInfo;
        //         _siteOptions = siteOptions;
        //         _message = data;
        //         CommentProvider = cp;
        //         var msg = sender.DisplayName + "さんがハートを送りました！";
        //         MessageItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(msg) };
        //         Init();
        //     }
        //     public LineLiveCommentViewModel(ICommentOptions options, ILineLiveSiteOptions siteOptions, ParseMessage.IGiftMessage data, ParseMessage.IUser sender, IUser user, ICommentProvider cp) :
        //base(options, user, cp, false)
        //     {
        //         MessageType = MessageType.BroadcastInfo;
        //         _siteOptions = siteOptions;
        //         _message = data;
        //         CommentProvider = cp;
        //         //2018/07/13
        //         if (data.ItemId == "limited-love-gift" || string.IsNullOrEmpty(data.Url))
        //         {
        //             //{"type":"giftMessage","data":{"message":"","type":"LOVE","itemId":"limited-love-gift","quantity":1,"displayName":"limited.love.gift.item","sender":{"id":2903515,"hashedId":"715i4MKqyv","displayName":"上杉The Times","iconUrl":"https://scdn.line-apps.com/obs/0hmNs42D-0MmFOTR9H8JtNNnYQNBY3YzEpNmkpRHdEbQI3LnYxIX97UGIdaVdjKXVjd3ktVGNEP1VjenU1ew/f64x64","hashedIconId":"0hmNs42D-0MmFOTR9H8JtNNnYQNBY3YzEpNmkpRHdEbQI3LnYxIX97UGIdaVdjKXVjd3ktVGNEP1VjenU1ew","isGuest":false,"isBlocked":false},"isNGGift":false,"sentAt":1531445716,"key":"2426265.29035150000000000000","blockedByCms":false}}
        //             var msg = sender.DisplayName + "さんがハートで応援ポイントを送りました！";
        //             MessageItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(msg) };
        //         }
        //         else
        //         {
        //             var msg = sender.DisplayName + "さんが" + data.Quantity + "コインプレゼントしました！";
        //             MessageItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(msg), new MessageImage { Url = data.Url } };
        //         }
        //         Init();
        //     }
        //     public LineLiveCommentViewModel(ICommentOptions options, ILineLiveSiteOptions siteOptions, ParseMessage.IFollowStartData data, ParseMessage.IUser sender, IUser user, ICommentProvider cp) :
        //         base(options, user, cp, false)
        //     {
        //         MessageType = MessageType.BroadcastInfo;
        //         _siteOptions = siteOptions;
        //         _message = data;
        //         CommentProvider = cp;
        //         var msg = sender.DisplayName + "さんがフォローしました！";
        //         MessageItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(msg) };
        //         Init();
        //     }
        public LineLiveCommentViewModel(ICommentOptions options, ILineLiveSiteOptions siteOptions, ParseMessage.IMessageData data, ParseMessage.IUser sender, IUser user, ICommentProvider cp) :
            base(options, user, cp, false)
        {
            MessageType = MessageType.Comment;
            _siteOptions = siteOptions;
            //_message = data;
            UserId = sender.Id.ToString();
            Id = "";
            if (siteOptions.IsAutoSetNickname)
            {
                var nick = ExtractNickname(data.Message);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            SetNameItems();
            MessageItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(data.Message) };
            Thumbnail = new MessageImage { Url = sender.IconUrl };
            PostTime = Tools.FromUnixTime(data.SentAt).ToString("HH:mm:ss");
            Init();
            NameItemsInternal = new List<IMessagePart> { MessagePartFactory.CreateMessageText(sender.DisplayName) };
            User.Name = NameItemsInternal;
        }
        protected override void NicknameChanged()
        {
            SetNameItems();
        }
        private void SetNameItems()
        {
            if (!string.IsNullOrEmpty(User.Nickname))
            {
                NickItemsInternal = new List<IMessagePart> { MessagePartFactory.CreateMessageText(User.Nickname) };
            }
        }
    }
    public class MessageImage : IMessageImage
    {
        public int? Width { get; set; }

        public int? Height { get; set; }

        public string Url { get; set; }

        public string Alt { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is MessageImage image)
            {
                return this.Url.Equals(image.Url) && this.Alt.Equals(image.Alt);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Url.GetHashCode() ^ Alt.GetHashCode();
        }
    }
}
