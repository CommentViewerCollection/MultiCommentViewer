using System.Net;

namespace ryu_s.BrowserCookie
{
    public interface IBrowserProfile
    {
        string Path { get; }
        string ProfileName { get; }
        BrowserType Type { get; }
        Cookie GetCookie(string domain, string name);
        CookieCollection GetCookieCollection(string domain);
    }
}
