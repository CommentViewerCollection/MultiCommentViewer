using SitePlugin;
using System;
using System.Collections.Generic;

namespace MirrativSitePlugin
{
    internal class MirrativComment : MessageBase2, IMirrativComment
    {
        public override SiteType SiteType { get; } = SiteType.Mirrativ;
        public MirrativMessageType MirrativMessageType { get; } = MirrativMessageType.Comment;
        public string Text { get; }
        public string UserName { get; }
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public MirrativComment(Message commentData, string raw) : base(raw)
        {
            UserId = commentData.UserId;
            Id = commentData.Id;
            Text = commentData.Comment;
            UserName = commentData.Username;
            PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(commentData.CreatedAt);
        }
    }
}
