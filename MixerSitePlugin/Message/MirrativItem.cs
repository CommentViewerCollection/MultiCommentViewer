using SitePlugin;
using System.Collections.Generic;

namespace MixerSitePlugin
{
    internal class MixerPhotoGift : MessageBase2, IMixerPhotoGift
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MixerMessageType MixerMessageType { get; } = MixerMessageType.Item;
        public string UserId { get; }
        public string PostTime { get; }
        public string Id { get; }
        public string GiftTitle { get; set; }
        public string PhotoGiftId { get; set; }
        public string BUrl { get; set; }
        public int Coins { get; set; }
        public string GiftSmallImageUrl { get; set; }
        public string Text { get; set; }
        public string UserName { get; set; }
        public string ShareText { get; set; }
        public MixerPhotoGift(Message commentData, string raw) : base(raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;
            Text = commentData.Comment;
            UserName = commentData.Username;
            //UserIcon = null;
            PostTime = null;
        }
    }
    internal class MixerGift : MessageBase2, IMixerGift
    {
        public override SiteType SiteType { get; } = SiteType.Mixer;
        public MixerMessageType MixerMessageType { get; } = MixerMessageType.Item;
        public string UserId { get; }
        public string PostTime { get; }
        public string Id { get; }
        public string GiftTitle { get; set; }
        public string PhotoGiftId { get; set; }
        public string BUrl { get; set; }
        public int Coins { get; set; }
        public string GiftSmallImageUrl { get; set; }
        public string Text { get; set; }
        public string UserName { get; set; }
        public int Count { get; set; }

        public MixerGift(Message commentData, string raw) : base(raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;
            Text = commentData.Comment;
            UserName = commentData.Username;
            //UserIcon = null;
            PostTime = null;
        }
    }
}
