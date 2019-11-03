using SitePlugin;
using System.Collections.Generic;

namespace MixerSitePlugin
{ 
    internal class MixerPhotoGift : MessageBase, IMixerPhotoGift
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

        public string ShareText { get; set; }
        public MixerPhotoGift(Message commentData, string raw) : base(raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;
            CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.Comment) };
            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.Username) };
            //UserIcon = null;
            PostTime = null;
        }
    }
    internal class MixerGift : MessageBase, IMixerGift
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

        public int Count { get; set; }

        public MixerGift(Message commentData, string raw) : base(raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;
            CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.Comment) };
            NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(commentData.Username) };
            //UserIcon = null;
            PostTime = null;
        }
    }
}
