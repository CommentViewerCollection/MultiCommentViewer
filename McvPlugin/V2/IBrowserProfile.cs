using System.Collections.Generic;
using System.Net;

namespace Mcv.PluginV2;

public interface IBrowserProfile
{
    string Path { get; }
    string ProfileName { get; }
    BrowserType Type { get; }
    Cookie GetCookie(string domain, string name);
    List<Cookie> GetCookieCollection(string domain);
}
