using SitePlugin;
using System;
using System.Collections.Generic;

namespace MixchSitePlugin
{
    internal class MixchMessage : MessageBase2, IMixchMessage
    {
        public override SiteType SiteType { get; } = SiteType.Mixch;
        public MixchMessageType MixchMessageType { get; set; }
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime PostTime { get; set; }
        public bool IsFirstComment { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IEnumerable<IMessagePart> MessageItems { get; set; }
        public MixchMessage(string raw) : base(raw)
        {

        }
    }
}
