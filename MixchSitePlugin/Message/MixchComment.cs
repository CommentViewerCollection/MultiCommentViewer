using SitePlugin;
using System;
using System.Collections.Generic;

namespace MixchSitePlugin
{
    internal class MixchComment : MessageBase2, IMixchComment
    {
        public override SiteType SiteType { get; } = SiteType.Mixch;
        public MixchMessageType MixchMessageType { get; set; }
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
}
