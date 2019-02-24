using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BrowserCookieImplementations.IE;
namespace ryu_s.BrowserCookie
{
    public class IEManager : IIEManager
    {
        public BrowserType Type => BrowserType.IE;

        public List<IBrowserProfile> GetProfiles()
        {
            return new List<IBrowserProfile> { new IEProfile() };
        }
        class IEProfile : IBrowserProfile
        {
            public string Path
            {
                get
                {
                    //Windows 7: C:\Users\username\AppData\Roaming
                    //Windows 8, 8.1, 10: C:\Users\username\AppData\Local\Microsoft\Windows\INetCookies
                    //それぞれのバージョン番号
                    //Windows 7: 6.1
                    //Windows 8: 6.2
                    //Windows 8.1: 6.3
                    //Windows 10: 10.0

                    //ただ、OSVersionは、Windows 8以降は全て6.2を返すらしい。Build番号とかは違うっぽいが。
                    var os = Environment.OSVersion;
                    if (double.Parse($"{os.Version.Major}.{os.Version.Minor}") >= 6.2)
                    {
                        //Windows 8以降
                        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Microsoft\Windows\INetCookies";
                    }
                    else
                    {
                        //Windows 7以前
                        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Microsoft\Windows\Cookies";
                    }
                }
            }

            public string ProfileName => "";

            public BrowserType Type => BrowserType.IE;

            public Cookie GetCookie(string domain, string name)
            {
                foreach (var cookie in GetCookieCollectionInternal(domain))
                {
                    if (cookie.Name == name)
                        return cookie;
                }
                return null;
            }

            public CookieCollection GetCookieCollection(string domain)
            {
                var collection = new CookieCollection();
                foreach (var cookie in GetCookieCollectionInternal(domain))
                {
                    collection.Add(cookie);
                }
                return collection;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="domain"></param>
            /// <returns></returns>
            private IEnumerable<Cookie> GetCookieCollectionInternal(string domain)
            {
                var path = "/";
                var cookieData = BrowserCookieImplementations.IE.Tools.GetCookieData("https://" + domain);
                var cookies = CookieData2Cookies(cookieData, domain, path);
                var cookieDataProtected = BrowserCookieImplementations.IE.Tools.GetProtectedModeCookieData("https://" + domain);
                var cookiesProtected = CookieData2Cookies(cookieDataProtected, domain, path);
                return cookies.Concat(cookiesProtected);
            }
            private IEnumerable<Cookie> CookieData2Cookies(string cookieData, string domain, string path)
            {
                var dict = BrowserCookieImplementations.IE.Tools.GetCookieDict(cookieData);
                foreach (var kv in dict)
                {
                    var cookie = new Cookie(kv.Key, kv.Value) { Domain = domain, Path = path };
                    yield return cookie;
                }
            }
        }
    }
}
