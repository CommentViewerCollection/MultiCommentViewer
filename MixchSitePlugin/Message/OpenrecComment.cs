using SitePlugin;
using System;
using System.Collections.Generic;

namespace MixchSitePlugin
{
    internal class MixchComment : MessageBase2, IMixchComment
    {
        public override SiteType SiteType { get; } = SiteType.Mixch;
        public MixchMessageType MixchMessageType { get; } = MixchMessageType.Comment;
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IEnumerable<IMessagePart> MessageItems { get; set; }
        public MixchComment(string raw) : base(raw)
        {

        }
    }
    internal class MixchStamp : MessageBase2, IMixchStamp
    {
        public override SiteType SiteType { get; } = SiteType.Mixch;
        public MixchMessageType MixchMessageType { get; } = MixchMessageType.Stamp;
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IMessageImage Stamp { get; set; }
        public string Message { get; set; }
        public MixchStamp(string raw) : base(raw)
        {

        }
    }
    internal class MixchYell : MessageBase2, IMixchYell
    {
        public override SiteType SiteType { get; } = SiteType.Mixch;
        public MixchMessageType MixchMessageType { get; } = MixchMessageType.Yell;
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public string YellPoints { get; set; }
        public string Message { get; set; }
        public MixchYell(string raw) : base(raw)
        {

        }
    }
}
