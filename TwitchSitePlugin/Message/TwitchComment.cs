using SitePlugin;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TwitchSitePlugin
{
    internal class TwitchComment : MessageBase, ITwitchComment
    {
        public override SiteType SiteType { get; } = SiteType.Twitch;
        public TwitchMessageType TwitchMessageType { get; } = TwitchMessageType.Comment;
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public bool IsDisplayNameSame { get; set; }
        public string DisplayName { get; set; }
        public TwitchComment(string raw) : base(raw)
        {

        }
    }
}
