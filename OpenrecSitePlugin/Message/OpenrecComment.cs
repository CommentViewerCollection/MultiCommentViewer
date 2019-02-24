using SitePlugin;
using System.Linq;
using System.Text;

namespace OpenrecSitePlugin
{
    internal class OpenrecComment : MessageBase, IOpenrecComment
    {
        public override SiteType SiteType { get; } = SiteType.Openrec;
        public OpenrecMessageType OpenrecMessageType { get; } = OpenrecMessageType.Comment;
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public OpenrecComment(string raw) : base(raw)
        {

        }
    }
    internal class OpenrecStamp : MessageBase, IOpenrecStamp
    {
        public override SiteType SiteType { get; } = SiteType.Openrec;
        public OpenrecMessageType OpenrecMessageType { get; } = OpenrecMessageType.Stamp;
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public OpenrecStamp(string raw) : base(raw)
        {

        }
    }
    internal class OpenrecYell : MessageBase, IOpenrecYell
    {
        public override SiteType SiteType { get; } = SiteType.Openrec;
        public OpenrecMessageType OpenrecMessageType { get; } = OpenrecMessageType.Yell;
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public OpenrecYell(string raw) : base(raw)
        {

        }
    }
}
