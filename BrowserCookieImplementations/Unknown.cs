using System.Collections.Generic;
using System.Net;

namespace ryu_s.BrowserCookie
{
    public class UnknownProfile : IBrowserProfile
    {
        public string Path { get; }

        public string ProfileName { get; }

        public BrowserType Type { get; }

        public Cookie GetCookie(string domain, string name)
        {
            return new Cookie("", "", "", "");
        }

        public CookieCollection GetCookieCollection(string domain)
        {
            return new CookieCollection();
        }
        public UnknownProfile()
        {
            Path = null;
            ProfileName = null;
            Type = BrowserType.Unknown;
        }
    }
    public class UnknownManager : IBrowserManager
    {
        public BrowserType Type { get; }

        public List<IBrowserProfile> GetProfiles()
        {
            return new List<IBrowserProfile>()
            {
                new UnknownProfile(),
            };
        }
        public UnknownManager()
        {
            Type = BrowserType.Unknown;
        }
    }
}
