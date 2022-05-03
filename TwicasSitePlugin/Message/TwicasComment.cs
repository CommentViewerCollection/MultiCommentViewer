using Mcv.PluginV2;
using System.Collections.Generic;

namespace TwicasSitePlugin
{
    internal class TwicasComment : MessageBase2, ITwicasComment
    {
        public override SiteType SiteType { get; } = SiteType.Twicas;
        public TwicasMessageType TwicasMessageType { get; } = TwicasMessageType.Comment;
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public string UserName { get; set; }
        public IEnumerable<IMessagePart> CommentItems { get; set; }

        public TwicasComment(string raw) : base(raw)
        {

        }
    }
}
