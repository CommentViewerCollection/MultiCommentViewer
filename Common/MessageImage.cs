using SitePlugin;
namespace Common
{
    public class MessageImage : IMessageImage
    {
        public int? Width { get; set; }

        public int? Height { get; set; }

        public string Url { get; set; }

        public string Alt { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is MessageImage image)
            {
                return string.Equals(this.Url, image.Url) && string.Equals(this.Alt, image.Alt);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Url.GetHashCode() ^ Alt.GetHashCode();
        }
    }
}
