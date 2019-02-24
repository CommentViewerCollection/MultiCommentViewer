using SitePlugin;
using System.Linq;
using System.Text;

namespace LineLiveSitePlugin
{
    internal class LineLiveComment : MessageBase, ILineLiveComment
    {
        public override SiteType SiteType { get; } = SiteType.LineLive;
        public LineLiveMessageType LineLiveMessageType { get; } = LineLiveMessageType.Comment;
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public LineLiveComment(string raw) : base(raw)
        {

        }
    }
    internal class LineLiveItem : MessageBase, ILineLiveItem
    {
        public override SiteType SiteType { get; } = SiteType.LineLive;
        public LineLiveMessageType LineLiveMessageType { get; } = LineLiveMessageType.Item;
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public LineLiveItem(string raw) : base(raw)
        {

        }
    }
}
