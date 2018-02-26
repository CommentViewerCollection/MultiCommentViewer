using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
            /// <param name="high"></param>
            /// <param name="low"></param>
            /// <returns></returns>
            public DateTime FromFileTime(long high, long low)
            {
                return new DateTime((high << 32) + low).AddYears(1600);
            }
            /// <summary>
            /// 
            /// </summary>
            private class IECookieRaw
            {
                public string name;
                public string value;
                public string path;
                public string domain;
                public string Flags;
                public string ExpirationTimeLow;
                public string ExpirationTimeHigh;
                public string CreationTimeLow;
                public string CreationTimeHigh;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="domain"></param>
            /// <returns></returns>
            private IEnumerable<Cookie> GetCookieCollectionInternal(string domain)
            {
                /*
                 * Name
                 * Value
                 * Path
                 * Flags (optional)
                 * Expiration Time (low)
                 * Expiration Time (high)
                 * Creation Time (low)
                 * Creation Time (high)
                 * Delimiter (used to separate multiple cookie entries in a single file)
                 * The expiration and creation times are in FILETIME format. 
                 * You can find information on the FILETIME format here. Most importantly:
                 * The FILETIME structure represents the number of 100-nanosecond intervals since January 1, 1601.
                 * The structure consists of two 32-bit values that combine to form a single 64-bit value.
                 * 
                 * '*'がデリミタとして使われるが、値の削除の際にも使われるためそれだけでは判断材料としては足りない。
                 * 1つのクッキーは7～8行と決まっており、Creation Time (high)が'*'になっている場所は確認できなかったため、7～8行目にあった場合はそれをデリミタであると断定して良さそう。
                 * 
                 */

                var filePaths = new List<string>();
                //ディレクトリは2種類あって、LowはLow privilegeの場合？
                var filePaths1 = System.IO.Directory.GetFiles(Path, "*.txt", System.IO.SearchOption.TopDirectoryOnly);
                filePaths.AddRange(filePaths1);
                var filePaths2 = System.IO.Directory.GetFiles(Path, "*.cookie", System.IO.SearchOption.TopDirectoryOnly);
                filePaths.AddRange(filePaths2);

                var filePaths3 = System.IO.Directory.GetFiles(Path + @"\Low", "*.txt", System.IO.SearchOption.TopDirectoryOnly);
                filePaths.AddRange(filePaths3);
                var filePaths4 = System.IO.Directory.GetFiles(Path + @"\Low", "*.cookie", System.IO.SearchOption.TopDirectoryOnly);
                filePaths.AddRange(filePaths4);

                foreach (string path in filePaths)
                {
                    List<string> fileLines = new List<string>();
                    using (var sr = new System.IO.StreamReader(path))
                    {
                        while (!sr.EndOfStream)
                        {
                            fileLines.Add(sr.ReadLine());
                        }
                    }
                    var contains = fileLines.Where(s => s.Contains(domain));
                    if (contains.Count() <= 0)
                        continue;//目的のCookieに該当する見込みなし

                    int fileLineNum = 0;
                    while (fileLineNum < fileLines.Count)
                    {
                        var lineNum = 0;
                        var lineList = new List<string>();
                        while (fileLineNum < fileLines.Count)
                        {
                            var line = fileLines[fileLineNum++];
                            lineNum++;
                            if (line == "*" && lineNum >= 7)//7～8行目の'*'はデリミタであると判断。
                                break;
                            lineList.Add(line);
                        }
                        var ieCookie = new IECookieRaw();
                        int n = 0;
                        ieCookie.name = lineList[n++];
                        ieCookie.value = lineList[n++];
                        var domainAndPath = lineList[n++];
                        var arr = domainAndPath.Split('/');
                        if (arr.Length >= 2)//一応チェックしているけど、確実にarr.Length==2であると考えて良いと思う。
                        {
                            ieCookie.domain = arr[0];
                            ieCookie.path = "/" + arr[1];
                        }
                        if (lineList.Count >= 8)//行数が8行ある場合はオプションのFlagsがあるということ。
                            ieCookie.Flags = lineList[n++];
                        ieCookie.ExpirationTimeLow = lineList[n++];
                        ieCookie.ExpirationTimeHigh = lineList[n++];
                        ieCookie.CreationTimeLow = lineList[n++];
                        ieCookie.CreationTimeHigh = lineList[n++];

                        if (ieCookie.domain != null && ieCookie.domain.EndsWith(domain))
                        {
                            //https://msdn.microsoft.com/en-us/library/system.net.cookie.value(v=vs.110).aspx
                            //The Value of a Cookie must not be null. The following characters are reserved and cannot be used for this property: semicolon, comma.
                            if (ieCookie.value.Contains(';') || ieCookie.value.Contains(','))
                                continue;
                            var cookie = new Cookie(ieCookie.name, ieCookie.value, ieCookie.path, ieCookie.domain);
                            //UTCになってるっぽいから+9時間
                            cookie.Expires = FromFileTime(long.Parse(ieCookie.ExpirationTimeHigh), long.Parse(ieCookie.ExpirationTimeLow)).AddHours(9);
                            cookie.Expired = (cookie.Expires < DateTime.Now);
                            yield return cookie;
                        }
                    }
                }
            }
        }
    }
}
