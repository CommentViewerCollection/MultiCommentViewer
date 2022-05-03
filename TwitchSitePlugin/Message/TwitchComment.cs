using Mcv.PluginV2;
using System.Collections.Generic;

namespace TwitchSitePlugin
{
    internal class TwitchComment : MessageBase2, ITwitchComment
    {
        public override SiteType SiteType { get; } = SiteType.Twitch;
        public TwitchMessageType TwitchMessageType { get; } = TwitchMessageType.Comment;
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public bool IsDisplayNameSame { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public IEnumerable<IMessagePart> CommentItems { get; set; }

        public TwitchComment(string raw) : base(raw)
        {

        }
    }
}
