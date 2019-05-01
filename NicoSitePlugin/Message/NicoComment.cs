using SitePlugin;
using System.Linq;
using System.Text;

namespace NicoSitePlugin
{
    internal class NicoComment : NicoMessageBase, INicoComment
    {
        public NicoMessageType NicoMessageType { get; } = NicoMessageType.Comment;
        public string Id { get; set; }
        public IMessageImage UserIcon { get; set; }
        public int? ChatNo { get; set; }

        public NicoComment(string raw, INicoSiteOptions siteOptions) : base(raw, siteOptions)
        {
        }
    }
}
