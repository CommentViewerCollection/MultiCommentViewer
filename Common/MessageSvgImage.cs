using SitePlugin;
namespace Common
{
    public class MessageSvgImage : IMessageRemoteSvg
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string Url { get; set; }
        public string Alt { get; set; }
    }
}
