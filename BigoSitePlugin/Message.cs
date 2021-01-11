using Common;
using SitePlugin;
using System;
using System.Collections.Generic;

namespace BigoSitePlugin
{
    internal class BigoConnected : MessageBase2, IBigoConnected
    {
        public override SiteType SiteType { get; } = SiteType.Bigo;
        public BigoMessageType BigoMessageType { get; } = BigoMessageType.Connected;
        public string Text { get; }
        public BigoConnected(string raw) : base(raw)
        {
            Text = "接続しました";
        }
    }
    internal class BigoDisconnected : MessageBase2, IBigoDisconnected
    {
        public override SiteType SiteType { get; } = SiteType.Bigo;
        public BigoMessageType BigoMessageType { get; } = BigoMessageType.Disconnected;
        public string Text { get; }
        public BigoDisconnected(string raw) : base(raw)
        {
            Text = "切断しました";
        }
    }
    class BigoGift : MessageBase2, IBigoGift
    {
        public override SiteType SiteType { get; } = SiteType.Bigo;
        public BigoMessageType BigoMessageType { get; } = BigoMessageType.Gift;
        public string Username { get; set; }
        public string GiftName { get; set; }
        public int GiftCount { get; set; }
        public string GiftImgUrl { get; set; }
        public BigoGift() : base("")
        {
        }
    }
    internal class BigoComment : MessageBase2, IBigoComment
    {
        public override SiteType SiteType { get; } = SiteType.Bigo;
        public BigoMessageType BigoMessageType { get; } = BigoMessageType.Comment;
        public string Id { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public BigoComment() : base("")
        {
            //UserId = commentData.UserId;
            //Id = commentData.Id;
            //Message = commentData.Message;
            //Name = commentData.Name;
            //PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(commentData.Date);
        }
    }
}
