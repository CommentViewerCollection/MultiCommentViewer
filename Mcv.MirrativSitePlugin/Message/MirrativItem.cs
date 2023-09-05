using Mcv.PluginV2;
using System;

namespace MirrativSitePlugin
{
    internal class MirrativPhotoGift : MessageBase2, IMirrativItem
    {
        public override SiteType SiteType { get; } = SiteType.Mirrativ;
        public MirrativMessageType MirrativMessageType { get; } = MirrativMessageType.Item;
        public string Text { get; set; }
        public string UserName { get; set; }
        public string UserId { get; }
        public DateTime PostedAt { get; }
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
            Text = commentData.Comment;
            UserName = commentData.Username;
            //UserIcon = null;
            PostedAt = Tools.UnixTime2DateTime(commentData.CreatedAt);
        }
    }
    internal class MirrativGift : MessageBase2, IMirrativItem
    {
        public override SiteType SiteType { get; } = SiteType.Mirrativ;
        public MirrativMessageType MirrativMessageType { get; } = MirrativMessageType.Item;
        public string Text { get; }
        public string UserName { get; }
        public string UserId { get; }
        public DateTime PostedAt { get; }
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
            Text = commentData.Comment;
            UserName = commentData.Username;
            //UserIcon = null;
            PostedAt = Tools.UnixTime2DateTime(commentData.CreatedAt);
        }
    }
}
