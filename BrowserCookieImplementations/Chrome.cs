using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
namespace ryu_s.BrowserCookie
{
    public class ChromeManager : IChromeManager
    {
        #region IChromeManager
        public BrowserType Type { get; }
        public List<IBrowserProfile> GetProfiles()
        {
            var list = new List<IBrowserProfile>();
            if (!Directory.Exists(ChromeSettingsDirPath))
            {
                //恐らくChromeがインストールされていない
                return list;
            }
            var defaultDbFilePath = ChromeSettingsDirPath + _defaultProfileName + "\\" + _dbFilename;
            if (System.IO.File.Exists(defaultDbFilePath))
            {
                list.Add(new ChromeProfile(defaultDbFilePath, _defaultProfileName));
            }
            var dirs = System.IO.Directory.GetDirectories(ChromeSettingsDirPath);
            foreach (var dir in dirs)
            {
                var dirName = System.IO.Path.GetFileName(dir);
                var testPath = dir + "\\" + _dbFilename;
                if (dirName.StartsWith("Profile", StringComparison.CurrentCultureIgnoreCase) && System.IO.File.Exists(testPath))
                {
                    var profileName = dirName;
                    list.Add(new ChromeProfile(ChromeSettingsDirPath + profileName + "\\" + _dbFilename, profileName));
                }
            }
            return list;
        }
        #endregion
        private readonly string _dbFilename = "Cookies";
        private readonly string _defaultProfileName = "Default";
        string ChromeSettingsDirPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\"; } }

        public ChromeManager()
        {
            Type = BrowserType.Chrome;
        }
        class ChromeProfile : IBrowserProfile
        {
            #region IBrowserProfile
            public string Path { get; }

            public string ProfileName { get; }

            public BrowserType Type { get; }

            public CookieCollection GetCookieCollection(string domain)
            {
                var query = "SELECT value, name, host_key, path, expires_utc, encrypted_value FROM cookies WHERE host_key LIKE '%" + domain + "'";
                return GetCookieCollectionInternal(query);
            }

            public Cookie GetCookie(string domain, string name)
            {
                var query = "SELECT value, name, host_key, path, expires_utc, encrypted_value FROM cookies WHERE host_key LIKE '%" + domain + "' AND name = '" + name + "'";
                var collection = GetCookieCollectionInternal(query);
                return (collection != null && collection.Count > 0) ? collection[0] : null;
            }
            #endregion

            #region Constructors
            public ChromeProfile(string path, string profileName)
            {
                Path = path;
                ProfileName = profileName;
                Type = BrowserType.Chrome;
            }
            #endregion

            #region Methods
            private string GetFiles(string path)
            {
                var defaultDir = System.IO.Path.GetDirectoryName(path);
                var userDataDir = System.IO.Directory.GetParent(defaultDir);
                var list = GetChildren(userDataDir.FullName);
                return userDataDir.FullName + "=\"" + string.Join(",", list) + "\"";
            }
            private List<string> GetChildren(string path)
            {
                if (System.IO.Directory.Exists(path))
                {
                    var entries = System.IO.Directory.GetFileSystemEntries(path);
                    var t = entries.SelectMany(s => GetChildren(s)).ToList();
                    return t;
                }
                else
                {
                    return new List<string> { path };
                }
            }
            private System.Net.CookieCollection GetCookieCollectionInternal(string query)
            {
                System.Data.DataTable dt = null;
                using (var tempFile = new TempFileProvider())
                {
                    try
                    {
                        System.IO.File.Copy(Path, tempFile.Path, true);
                    }
                    catch (System.IO.FileNotFoundException ex)
                    {
                        var str = "";
                        try
                        {
                            var defaultDir = System.IO.Path.GetDirectoryName(Path);
                            var userDataDir = System.IO.Directory.GetParent(defaultDir);
                            if (System.IO.Directory.Exists(userDataDir.FullName))
                            {
                                str = GetFiles(userDataDir.FullName);
                            }
                        }
                        catch (Exception)
                        {
                            str = "(error)";
                        }
                        throw new ChromeCookiesFileNotFoundException(str, ex);
                    }
                    using (var conn = SQLiteHelper.CreateConnection(tempFile.Path))
                    {
                        dt = SQLiteHelper.ExecuteReader(conn, query);
                    }
                }
                var collection = new CookieCollection();
                if (dt != null)
                {
                    var cc = new CookieContainer();
                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        var value = row["value"].ToString();
                        if (string.IsNullOrEmpty(value))//暗号化してるっぽいから復号化してみる。
                        {
                            var encrypted_value = (byte[])row["encrypted_value"];
                            value = UnProtect(encrypted_value);
                        }
                        //ここでURLエンコードをやると、ふわっちでAPIが取得できなかった。ここではやるべきではないのかも。
                        //if (value != null)
                        //    value = Uri.EscapeDataString(value);
                        var name = row["name"].ToString();
                        var host_key = row["host_key"].ToString();
                        var path = row["path"].ToString();
                        var expires_utc = long.Parse(row["expires_utc"].ToString());
                        var cookie = new Cookie(name, value, path, host_key)
                        {
                            //TODO:expires_utcの変換はこれで大丈夫だろうか。正しい値を取得できているか確認していない。
                            Expires = Tools.FromUnixTime(expires_utc / 1000000L - 11644473600L),
                        };
                        try
                        {
                            //CookieContainerに追加できないようなサイズの大きいvalueが存在したため、適合していることをチェックする。
                            //適合しなかったら例外が投げられ、追加しない。
                            cc.Add(cookie);
                            collection.Add(cookie);
                        }
                        catch (CookieException) { }
                    }
                }
                return collection;
            }
            /// <summary>
            /// 復号化する。
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            private string UnProtect(byte[] source)
            {
                return NativeMethods.CryptUnprotectData(source, Encoding.UTF8);
            }
            #endregion
        }
    }
}
