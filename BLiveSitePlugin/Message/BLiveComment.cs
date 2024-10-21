using SitePlugin;
using System;
using System.Collections.Generic;

namespace BLiveSitePlugin
{
    internal class BLiveComment : MessageBase2, IBLiveComment
    {
        public override SiteType SiteType { get; } = SiteType.BLive;
        public BLiveMessageType BLiveMessageType { get; } = BLiveMessageType.Comment;
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IEnumerable<IMessagePart> MessageItems { get; set; }
        public BLiveComment(string raw) : base(raw)
        {

        }
    }
    internal class BLiveStamp : MessageBase2, IBLiveStamp
    {
        public override SiteType SiteType { get; } = SiteType.BLive;
        public BLiveMessageType BLiveMessageType { get; } = BLiveMessageType.Stamp;
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IMessageImage Stamp { get; set; }
        public string Message { get; set; }
        public BLiveStamp(string raw) : base(raw)
        {

        }
    }
    internal class BLiveYell : MessageBase2, IBLiveYell
    {
        public override SiteType SiteType { get; } = SiteType.BLive;
        public BLiveMessageType BLiveMessageType { get; } = BLiveMessageType.Yell;
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public string YellPoints { get; set; }
        public string Message { get; set; }
        public BLiveYell(string raw) : base(raw)
        {

        }
    }
}
