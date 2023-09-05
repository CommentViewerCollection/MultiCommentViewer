using Mcv.PluginV2;
using System;
using System.Collections.Generic;

namespace OpenrecSitePlugin
{
    internal class OpenrecComment : MessageBase2, IOpenrecComment
    {
        public override SiteType SiteType { get; } = SiteType.Openrec;
        public OpenrecMessageType OpenrecMessageType { get; } = OpenrecMessageType.Comment;
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IEnumerable<IMessagePart> MessageItems { get; set; }
        public OpenrecComment(string raw) : base(raw)
        {

        }
    }
    internal class OpenrecStamp : MessageBase2, IOpenrecStamp
    {
        public override SiteType SiteType { get; } = SiteType.Openrec;
        public OpenrecMessageType OpenrecMessageType { get; } = OpenrecMessageType.Stamp;
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IMessageImage Stamp { get; set; }
        public string Message { get; set; }
        public OpenrecStamp(string raw) : base(raw)
        {

        }
    }
    internal class OpenrecYell : MessageBase2, IOpenrecYell
    {
        public override SiteType SiteType { get; } = SiteType.Openrec;
        public OpenrecMessageType OpenrecMessageType { get; } = OpenrecMessageType.Yell;
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public string YellPoints { get; set; }
        public string Message { get; set; }
        public OpenrecYell(string raw) : base(raw)
        {

        }
    }
}
