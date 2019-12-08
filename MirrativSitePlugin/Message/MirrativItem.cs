using SitePlugin;
using System.Collections.Generic;

namespace MirrativSitePlugin
{
    internal class MirrativPhotoGift : MessageBase, IMirrativItem
    {
        public override SiteType SiteType { get; } = SiteType.Mirrativ;
        public MirrativMessageType MirrativMessageType { get; } = MirrativMessageType.Item;
        public string UserId { get; }
        public string PostTime { get; }
        public string Id { get; }
        public string GiftTitle { get; set; }
        public string PhotoGiftId { get; set; }
        public string BUrl { get; set; }
        public int Coins { get; set; }
        public string GiftSmallImageUrl { get; set; }

        public string ShareText { get; set; }
        public MirrativPhotoGift(Message commentData, string raw) : base(raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;
            CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.Comment) };
            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.Username) };
            //UserIcon = null;
            PostTime = Tools.UnixTime2DateTime(commentData.CreatedAt).ToString("HH:mm:ss");
        }
    }
    internal class MirrativGift : MessageBase, IMirrativItem
    {
        public override SiteType SiteType { get; } = SiteType.Mirrativ;
        public MirrativMessageType MirrativMessageType { get; } = MirrativMessageType.Item;
        public string UserId { get; }
        public string PostTime { get; }
        public string Id { get; }
        public string GiftTitle { get; set; }
        public string PhotoGiftId { get; set; }
        public string BUrl { get; set; }
        public int Coins { get; set; }
        public string GiftSmallImageUrl { get; set; }

        public int Count { get; set; }

        public MirrativGift(Message commentData, string raw) : base(raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;
            CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.Comment) };
            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.Username) };
            //UserIcon = null;
            PostTime = Tools.UnixTime2DateTime(commentData.CreatedAt).ToString("HH:mm:ss");
        }
    }
}
