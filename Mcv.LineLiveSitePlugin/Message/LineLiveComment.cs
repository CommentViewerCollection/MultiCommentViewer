using Mcv.PluginV2;
using System;
using System.Collections.Generic;

namespace LineLiveSitePlugin
{
    internal class LineLiveComment : MessageBase2, ILineLiveComment
    {
        public override SiteType SiteType { get; } = SiteType.LineLive;
        public LineLiveMessageType LineLiveMessageType { get; } = LineLiveMessageType.Comment;
        public string Text { get; set; }
        public bool IsNgMessage { get; set; }
        public DateTime PostedAt { get; set; }
        public string UserIconUrl { get; set; }
        public long UserId { get; set; }
        public string DisplayName { get; set; }
        public LineLiveComment(string raw) : base(raw)
        {
        }
    }
    internal class LineLiveItem : MessageBase2, ILineLiveItem
    {
        public override SiteType SiteType { get; } = SiteType.LineLive;
        public LineLiveMessageType LineLiveMessageType { get; } = LineLiveMessageType.Item;
        public IEnumerable<IMessagePart> CommentItems { get; set; }
        public long UserId { get; set; }
        public DateTime PostedAt { get; set; }
        public string UserIconUrl { get; set; }
        public string DisplayName { get; set; }
        public LineLiveItem(string raw) : base(raw)
        {
        }
    }
}
