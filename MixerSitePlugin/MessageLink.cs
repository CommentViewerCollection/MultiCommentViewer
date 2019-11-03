using SitePlugin;

namespace MixerSitePlugin
{
    class MessageLink : IMessageLink
    {
        public string Text { get; set; }
        public string Url { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is MessageLink text)
            {
                return this.Text.Equals(text.Text) && this.Url.Equals(text.Url);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Text.GetHashCode() ^ Url.GetHashCode();
        }
    }
}
