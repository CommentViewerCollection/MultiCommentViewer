using SitePlugin;
using System.Linq;
using System.Text;

namespace TwicasSitePlugin
{
    internal class TwicasComment : MessageBase, ITwicasComment
    {
        public override SiteType SiteType { get; } = SiteType.Twicas;
        public TwicasMessageType TwicasMessageType { get; } = TwicasMessageType.Comment;
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public TwicasComment(string raw) : base(raw)
        {

        }
    }
}
