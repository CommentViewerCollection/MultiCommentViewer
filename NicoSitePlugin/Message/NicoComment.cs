using SitePlugin;
using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace NicoSitePlugin
{
    internal class NicoComment : MessageBase2, INicoComment
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Comment;
        public string Id { get; set; }
        public string ThumbnailUrl { get; set; }
        public int? ChatNo { get; set; }
        public string UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public string Text { get; set; }
        public string RoomName { get; set; }
        public bool Is184 { get; set; }
        public string UserName { get; set; }
        public NicoComment(string raw) : base(raw)
        {
        }
        public void SetUserName(string userName)
        {
            UserName = userName;
        }
    }
}
