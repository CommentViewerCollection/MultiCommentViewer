using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
namespace ryu_s.BrowserCookie
{
    public class ChromeBetaManager : ChromeManager
    {
        public override BrowserType Type => BrowserType.ChromeBeta;
        protected override string ChromeSettingsDirPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome Beta\User Data\";
    }
    public class ChromeManager : IChromeManager
    {
        #region IChromeManager
        public virtual BrowserType Type => BrowserType.Chrome;
        public List<IBrowserProfile> GetProfiles()
        {
            var list = new List<IBrowserProfile>();
            if (!Directory.Exists(ChromeSettingsDirPath))
            {
                //恐らくChromeがインストールされていない
                return list;
            }
            //2021/12/12
            //仕様変更があった。
            //Cookiesファイルの配置が変わった
            //旧:"PROFILENAME"\Cookies
            //新:"PROFILENAME"\Network\Cookies
            //"Default"プロファイルは新しい配置になっていたけど、他のプロファイルはそのままだった。
            //新しいプロファイルを作成すると新しい配置になっていた。
            var defaultProfilePath = Path.Combine(ChromeSettingsDirPath, _defaultProfileName);
            var defaultDbFilePathOld = GetCookiesFileOldPath(defaultProfilePath, _dbFilename);
            var defaultDbFilePath = GetCookiesFilePath(defaultProfilePath, _dbFilename);
            if (System.IO.File.Exists(defaultDbFilePathOld))
            {
                list.Add(new ChromeProfile(Type, ChromeSettingsDirPath, defaultDbFilePathOld, _defaultProfileName));
            }
            else if (System.IO.File.Exists(defaultDbFilePath))
            {
                list.Add(new ChromeProfile(Type, ChromeSettingsDirPath, defaultDbFilePath, _defaultProfileName));
            }
            var dirs = System.IO.Directory.GetDirectories(ChromeSettingsDirPath);
            foreach (var dir in dirs)
            {
                var dirName = System.IO.Path.GetFileName(dir);
                if (!dirName.StartsWith("Profile", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                var profileCookiesFilePath = GetCookiesFilePath(dir, _dbFilename);
                var profileCookiesFilePathOld = GetCookiesFileOldPath(dir, _dbFilename);
                var profileName = dirName;
                if (File.Exists(profileCookiesFilePath))
                {
                    list.Add(new ChromeProfile(Type, ChromeSettingsDirPath, profileCookiesFilePath, profileName));
                }
                else if (File.Exists(profileCookiesFilePathOld))
                {
                    list.Add(new ChromeProfile(Type, ChromeSettingsDirPath, profileCookiesFilePathOld, profileName));
                }
            }
            return list;
        }
        private static string GetCookiesFilePath(string profilePath, string cookieFileName)
        {
            return Path.Combine(profilePath, "Network", cookieFileName);
        }
        private static string GetCookiesFileOldPath(string profilePath, string cookieFileName)
        {
            return Path.Combine(profilePath, cookieFileName);
        }
        #endregion
        private readonly string _dbFilename = "Cookies";
        private readonly string _defaultProfileName = "Default";
        protected virtual string ChromeSettingsDirPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\"; } }

        public ChromeManager()
        {
        }
        class ChromeProfile : IBrowserProfile
        {
            #region IBrowserProfile
            public string Path { get; }

            public string ProfileName { get; }

            public BrowserType Type { get; }
            ChromeAesGcm _decryptor = new ChromeAesGcm();
            public List<Cookie> GetCookieCollection(string domain)
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
            [Obsolete]
            public ChromeProfile(BrowserType type, string userDataDirPath, string profileName)
            {
                Path = userDataDirPath + "" + profileName + "\\Cookies";
                ProfileName = profileName;
                Type = type;
                _decryptor.LocalStatePath = userDataDirPath + "Local State";
            }
            public ChromeProfile(BrowserType type, string userDataDirPath, string cookiesPath, string profileName)
            {
                Path = cookiesPath;
                ProfileName = profileName;
                Type = type;
                _decryptor.LocalStatePath = userDataDirPath + "Local State";
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
            private List<Cookie> GetCookieCollectionInternal(string query)
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
                var list = new List<Cookie>();
                if (dt != null)
                {
                    var cc = new CookieContainer();
                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        var name = row["name"].ToString();
                        var value = row["value"].ToString();
                        if (string.IsNullOrEmpty(value))//暗号化してるっぽいから復号化してみる。
                        {
                            var encrypted_value = (byte[])row["encrypted_value"];
                            if (IsDPAPIed(encrypted_value))
                            {
                                value = UnProtect(encrypted_value);
                            }
                            else
                            {
                                value = _decryptor.Decrypt(encrypted_value);
                            }
                        }
                        var host_key = row["host_key"].ToString();
                        var path = row["path"].ToString();
                        var expires_utc = long.Parse(row["expires_utc"].ToString());
                        var cookie = new Cookie(name, value, path, host_key)
                        {
                            //TODO:expires_utcの変換はこれで大丈夫だろうか。正しい値を取得できているか確認していない。
                            Expires = Tools.FromUnixTime(expires_utc / 1000000L - 11644473600L),
                        };
                        if (value == null)
                        {
                            continue;
                        }
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

            private static bool IsDPAPIed(byte[] data)
            {
                return data != null && data.Length > 4 && data[0] == 1 && data[1] == 0 && data[2] == 0 && data[3] == 0;
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
