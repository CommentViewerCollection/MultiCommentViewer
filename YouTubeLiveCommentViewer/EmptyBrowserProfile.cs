using System.Net;
using ryu_s.BrowserCookie;

namespace YouTubeLiveCommentViewer
{
    public class EmptyBrowserProfile : IBrowserProfile
    {
        public string Path => "";

        public string ProfileName => "無し";

        public BrowserType Type { get { return BrowserType.Unknown; } }

        public Cookie GetCookie(string domain, string name)
        {
            return null;
        }

        public CookieCollection GetCookieCollection(string domain)
        {
            return new CookieCollection();
        }
    }
}
