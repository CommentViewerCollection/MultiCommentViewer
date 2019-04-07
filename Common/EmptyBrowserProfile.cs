using System.Collections.Generic;
using System.Net;
using ryu_s.BrowserCookie;

namespace Common
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

        public List<Cookie> GetCookieCollection(string domain)
        {
            return new List<Cookie>();
        }
    }
}
