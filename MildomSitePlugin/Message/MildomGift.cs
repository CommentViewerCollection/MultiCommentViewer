using SitePlugin;
using System;
using System.Collections.Generic;

namespace MildomSitePlugin
{
    internal class MildomGift : MessageBase2, IMildomGift
    {
        public override SiteType SiteType { get; } = SiteType.Mildom;
        public string GiftName { get; }
        public int Coins { get; }
        public int Count { get; }
        public string GiftId { get; }
        public string GiftUrl { get; }
        public long UserId { get; }
        public string UserName { get; }
        public DateTime PostedAt { get; }
        public MildomMessageType MildomMessageType { get; } = MildomMessageType.Gift;

        public MildomGift(OnGiftMessage low, GiftItem item) : base(low.Raw)
        {
            GiftName = item.Name;
            //Coins = item.Coins;
            Count = low.Count;
            PostedAt = low.PostedAt;
            UserId = low.UserId;
            GiftUrl = item.Url;
            UserName = low.UserName;
            UserId = low.UserId;
        }
    }
}
