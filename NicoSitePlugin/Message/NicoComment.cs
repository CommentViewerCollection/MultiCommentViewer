using SitePlugin;
using System.Linq;
using System.Text;

namespace NicoSitePlugin
{
    internal class NicoComment : MessageBase, INicoComment
    {
        public override SiteType SiteType { get; } = SiteType.NicoLive;
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Comment;
        public string Id { get; set; }
        public string UserId { get; set; }
        public string PostTime { get; set; }
        public IMessageImage UserIcon { get; set; }
        public bool Is184 { get; set; }
        public string RoomName { get; set; }
        public int? ChatNo { get; set; }

        public NicoComment(string raw) : base(raw)
        {

        }
    }
}
