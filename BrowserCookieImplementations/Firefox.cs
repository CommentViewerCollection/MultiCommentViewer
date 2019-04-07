using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;

namespace ryu_s.BrowserCookie
{
    using ryu_s.BrowserCookie.Firefox;
    public class FirefoxManager : IFirefoxManager
    {
        #region IFirefoxManager
        public BrowserType Type { get; }

        public List<IBrowserProfile> GetProfiles()
        {
            var profileFileName = "profiles.ini";
            var list = new List<IBrowserProfile>();
            if(!File.Exists(Path.Combine(_moz_path, profileFileName)))
            {
                //多分Firefoxをインストールしていない
                return list;
            }

            var profiles = FirefoxProfile.GetProfiles(_moz_path, profileFileName);
            foreach (var profile in profiles)
            {
                if (profile.IsDefault)
                    list.Insert(0, new FirefoxCookie(profile));
                else
                    list.Add(new FirefoxCookie(profile));
            }
            return list;
        }
        public FirefoxManager()
        {
            Type = BrowserType.Firefox;
        }
        #endregion
        class FirefoxCookie : IBrowserProfile
        {
            #region IBrowserProfile
            public string Path { get; }

            public string ProfileName { get; }

            public BrowserType Type { get; }

            public Cookie GetCookie(string domain, string name)
            {
                var query = "SELECT value, name, host, path, expiry FROM moz_cookies WHERE host LIKE '%" + domain + "' AND name = '" + name + "'";
                var collection = GetCookieCollectionInternal(query);
                return (collection != null && collection.Count > 0) ? collection[0] : null;
            }

            public List<Cookie> GetCookieCollection(string domain)
            {
                var query = "SELECT value, name, host, path, expiry FROM moz_cookies WHERE host LIKE '%" + domain + "'";
                return GetCookieCollectionInternal(query);
            }
            #endregion

            #region Methods
            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private List<Cookie> GetCookieCollectionInternal(string query)
            {
                //使用中でロックが掛かっている可能性があるため、一旦コピー。
                var tempFile = new TempFileProvider();
                System.IO.File.Copy(Path, tempFile.Path, true);

                var list = new List<Cookie>();
                System.Data.DataTable dt = null;
                using (var conn = SQLiteHelper.CreateConnection(tempFile.Path))
                {
                    conn.Open();
                    dt = SQLiteHelper.ExecuteReader(conn, query);
                }

                if (dt != null)
                {
                    var cc = new CookieContainer();
                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        //カラム名
                        //id,baseDomain,appId,inBrowserElement,name,value,host,path,expiry,lastAccessed,creationTime,isSecure,isHttpOnly
                        var value = row["value"].ToString();

                        //FRESH!で不具合のためコメントアウト。
                        //if (value != null)
                        //    value = Uri.EscapeDataString(value);
                        var name = row["name"].ToString();
                        var host = row["host"].ToString();
                        var path = row["path"].ToString();
                        var expires = long.Parse(row["expiry"].ToString());
                        var cookie = new Cookie(name, value, path, host)
                        {
                            //TODO:expiresは弄らずにそのまま変換して大丈夫だろうか。正しい値を取得できているか確認していない。
                            Expires = Tools.FromUnixTime(expires)
                        };

                        try
                        {
                            //CookieContainerに追加できないようなサイズの大きいvalueが存在したため、適合していることをチェックする。
                            //適合しなかったら例外が投げられ、追加しない。
                            cc.Add(cookie);
                            list.Add(cookie);
                        }
                        catch (CookieException) { }
                    }
                }
                return list;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            private string UnProtect(byte[] source)
            {
                return NativeMethods.CryptUnprotectData(source, Encoding.UTF8);
            }
            #endregion

            public FirefoxCookie(FirefoxProfile profile)
            {
                Path = profile.path + "\\" + _cookieFilename;
                ProfileName = profile.Name;
                Type = BrowserType.Firefox;
            }

            private readonly string _cookieFilename = "cookies.sqlite";
        }
        private readonly string _moz_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Mozilla\Firefox";
    }
}

namespace ryu_s.BrowserCookie.Firefox
{
    /// <summary>
    /// 
    /// </summary>
    internal class FirefoxProfile
    {
        public string Name { get; private set; }
        public bool IsRelative { get; private set; } = false;
        public bool IsDefault { get; private set; } = false;
        public string path = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="moz_path"></param>
        /// <param name="iniFileName"></param>
        /// <returns></returns>
        public static FirefoxProfile GetDefaultProfile(string moz_path, string iniFileName)
        {
            var profiles = GetProfiles(moz_path, iniFileName);
            if (profiles.Count == 1)
                return profiles[0];
            else
            {
                foreach (var profile in profiles)
                {
                    if (profile.IsDefault)
                        return profile;
                }
            }
            return null;
        }
        public static List<FirefoxProfile> GetProfiles(IEnumerable<string> lines, string moz_path)
        {
            var list = new List<FirefoxProfile>();
            FirefoxProfile profile = null;
            foreach (var line in lines)
            {
                if (line.StartsWith("[Profile"))
                {
                    profile = new FirefoxProfile();
                    list.Add(profile);
                    continue;
                }
                if (profile != null)
                {
                    var pair = SplitByEqual(line);
                    switch (pair.Key)
                    {
                        case "Name":
                            profile.Name = pair.Value;
                            break;
                        case "IsRelative":
                            profile.IsRelative = (pair.Value == "1") ? true : false;
                            break;
                        case "Path":
                            profile.path = pair.Value.Replace("/", "\\");
                            if (profile.IsRelative)
                                profile.path = moz_path + "\\" + profile.path;
                            break;
                        case "Default":
                            profile.IsDefault = (pair.Value == "1") ? true : false;
                            break;
                        default:
                            break;
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="moz_path"></param>
        /// <param name="iniFileName"></param>
        /// <returns></returns>
        public static List<FirefoxProfile> GetProfiles(string moz_path, string iniFileName)
        {
            var list = new List<FirefoxProfile>();
            var path = moz_path + "\\" + iniFileName;
            var enc = Encoding.UTF8;
            if (!System.IO.File.Exists(path))
            {
                throw new FirefoxProfileIniNotFoundException($"path={path}");
            }
            using (var sr = new System.IO.StreamReader(path, enc))
            {
                list.AddRange(GetProfiles(ReadLines(sr), moz_path));
            }
            return list;
        }
        public static IEnumerable<string> ReadLines(System.IO.StreamReader sr)
        {
            while (!sr.EndOfStream)
            {
                yield return sr.ReadLine();
            }
        }
        /// <summary>
        /// 文字列を'='で2つに分割する。
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        internal static KeyValuePair<string, string> SplitByEqual(string line)
        {
            var arr = line.Split('=').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            if (arr.Length >= 2)
            {
                var pos = line.IndexOf('=');
                return new KeyValuePair<string, string>(arr[0],line.Substring(pos+1));
            }
            else
            {
                return new KeyValuePair<string, string>();
            }
        }
    }
}
