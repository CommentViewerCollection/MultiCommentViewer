using SitePlugin;
namespace Common
{
    public class MessageSvgData : IMessageSvg
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string Data { get; set; }
        public string Alt { get; set; }
    }
}
