using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using Plugin;
using System.ComponentModel.Composition;
using SitePlugin;

namespace CommentViewer.Plugin
{
    [Export(typeof(IPlugin))]
    public class CommentGeneratorPlugin : IPlugin, IDisposable
    {
        protected virtual Options Options { get; set; }
        //string _hcgPath = "";
        private System.Timers.Timer _writeTimer;
        private System.Timers.Timer _deleteTimer;
        private SynchronizedCollection<Data> _commentCollection = new SynchronizedCollection<Data>();

        protected virtual string CommentXmlPath { get; private set; }
        public string Name
        {
            get
            {
                return "コメジェネ連携";
            }
        }
        public string Description
        {
            get
            {
                return "";
            }
        }
        public IPluginHost Host { get; set; }
        class Data
        {
            public string IconUrl { get; internal set; }
            public string Comment { get; internal set; }
            public string SiteName { get; internal set; }
            public string Nickname { get; internal set; }
        }
        public void OnMessageReceived(ISiteMessage message, IMessageMetadata messageMetadata)
        {
            //if (!(message is IMessageComment comment)) return;
            if (!Options.IsEnabled || messageMetadata.IsNgUser || messageMetadata.IsInitialComment)
                return;

            //各サイトのサービス名
            //YouTubeLive:youtubelive
            //ニコ生:nicolive
            //Twitch:twitch
            //Twicas:twicas
            //ふわっち:whowatch
            //OPENREC:openrec
            //Mirrativ:mirrativ
            //LINELIVE:linelive
            //Periscope:periscope
            //Mixer:mixer


            if(message is MirrativSitePlugin.IMirrativJoinRoom && !Options.IsMirrativeJoin)
            {
                return;
            }
            //string name;
            //if (HasNickname(messageMetadata.User))
            //{
            //    name = messageMetadata.User.Nickname;
            //}
            //else
            //{
            //    name = comment.NameItems.ToText();
            //}
            var siteName = Tools.GetSiteName(message);
            var (name, comment) = PluginCommon.Tools.GetData(message);
            if (HasNickname(messageMetadata.User))
            {
                name = messageMetadata.User.Nickname;
            }

            // 2021/06/12 HTML5コメジェネ上でアイコンを表示できるパラメータを追加
            var iconUrl = EmbedIconUrl(message);

            //var data = new Data
            //{
            //    Comment = comment.CommentItems.ToText(),
            //    Nickname = name,
            //    SiteName = siteName,
            //};
            var data = new Data
            {
                IconUrl = iconUrl,
                Comment = comment,
                Nickname = name,
                SiteName = siteName,
            };
            _commentCollection.Add(data);
        }

        private static bool HasNickname(IUser user)
        {
            return user != null && !string.IsNullOrEmpty(user.Nickname);
        }

        public virtual void OnLoaded()
        {
            Options = Options.Load(GetSettingsFilePath());

            _writeTimer = new System.Timers.Timer
            {
                Interval = 500
            };
            _writeTimer.Elapsed += _writeTimer_Elapsed;
            _writeTimer.Start();

            _deleteTimer = new System.Timers.Timer
            {
                Interval = 5 * 60 * 1000
            };
            _deleteTimer.Elapsed += _deleteTimer_Elapsed;
            _deleteTimer.Start();
        }

        private readonly object _xmlWriteLockObj = new object();
        /// <summary>
        /// XMLファイルに書き込む
        /// </summary>
        /// <param name="xmlRootElement"></param>
        /// <param name="path"></param>
        private void WriteXml(XElement xmlRootElement, string path)
        {
            lock (_xmlWriteLockObj)
            {
                xmlRootElement.Save(path);
            }
        }
        private void _deleteTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Options.IsEnabled)
                return;

            //comment.xmlの要素を定期的に削除する
            XElement xml;
            try
            {
                if (!File.Exists(CommentXmlPath))
                    return;
                xml = XElement.Load(CommentXmlPath);
                var arr = xml.Elements().ToArray();
                var count = arr.Length;
                if (count > 1000)
                {
                    //1000件以上だったら、最後の100件以外を全て削除
                    xml.RemoveAll();
                    for (int i = count - 100; i < count; i++)
                    {
                        xml.Add(arr[i]);
                    }
                    WriteXml(xml, CommentXmlPath);
                }
            }
            catch (IOException ex)
            {
                //being used in another process
                Debug.WriteLine(ex.Message);
                return;
            }
        }

        protected virtual string GetHcgPath(string hcgSettingsFilePath)
        {
            string settingXml;
            using (var sr = new StreamReader(hcgSettingsFilePath))
            {
                settingXml = sr.ReadToEnd();
            }
            var xmlParser = DynamicXmlParser.Parse(settingXml);
            if (!xmlParser.HasElement("HcgPath"))
                return "";
            return xmlParser.HcgPath;
        }
        private void _writeTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //定期的にcomment.xmlに書き込む。

            Write();
        }
        protected virtual bool IsHcgSettingFileExists()
        {
            return File.Exists(Options.HcgSettingFilePath);
        }
        /// <summary>
        /// _commentCollectionの内容をファイルに書き出す
        /// </summary>
        public void Write()
        {
            if (!Options.IsEnabled || _commentCollection.Count == 0)
                return;

            //TODO:各ファイルが存在しなかった時のエラー表示
            if (string.IsNullOrEmpty(CommentXmlPath) && IsHcgSettingFileExists())
            {
                var hcgPath = GetHcgPath(Options.HcgSettingFilePath);
                CommentXmlPath = hcgPath + "\\comment.xml";
                //TODO:パスがxmlファイルで無かった場合の対応。ディレクトリの可能性も。
            }
            if (!File.Exists(CommentXmlPath))
            {
                var doc = new XmlDocument();
                var root = doc.CreateElement("log");

                doc.AppendChild(root);
                doc.Save(CommentXmlPath);
            }
            XElement xml;
            try
            {
                xml = XElement.Load(CommentXmlPath);
            }
            catch (IOException ex)
            {
                //being used in another process
                Debug.WriteLine(ex.Message);
                return;
            }
            catch (XmlException)
            {
                //Root element is missing.
                xml = new XElement("log");
            }
            lock (_lockObj)
            {
                var arr = _commentCollection.ToArray();

                foreach (var data in arr)
                {
                    var item = new XElement("comment", data.Comment);
                    item.SetAttributeValue("icon_url", data.IconUrl);
                    item.SetAttributeValue("no", "0");
                    item.SetAttributeValue("time", ToUnixTime(GetCurrentDateTime()));
                    item.SetAttributeValue("owner", 0);
                    item.SetAttributeValue("service", data.SiteName);
                    //2019/08/25 コメジェネの仕様で、handleタグが無いと"0コメ"に置換されてしまう。だから空欄でも良いからhandleタグは必須。
                    var handle = string.IsNullOrEmpty(data.Nickname) ? "" : data.Nickname;
                    item.SetAttributeValue("handle", handle);
                    xml.Add(item);
                }
                try
                {
                    WriteXml(xml, CommentXmlPath);
                }
                catch (IOException ex)
                {
                    //コメントの流れが早すぎるとused in another processが来てしまう。
                    //この場合、コメントが書き込まれずに消されてしまう。
                    Debug.WriteLine(ex.Message);
                    return;
                }
                _commentCollection.Clear();
            }
        }
        private static readonly object _lockObj = new object();
        protected virtual DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }

        public static long ToUnixTime(DateTime dateTime)
        {
            // 時刻をUTCに変換
            dateTime = dateTime.ToUniversalTime();

            // unix epochからの経過秒数を求める
            return (long)dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }
        public void OnClosing()
        {
            _settingsView?.ForceClose();
            _writeTimer?.Stop();
            _deleteTimer?.Stop();
            if (Options != null)
            {
                Options.Save(Options, GetSettingsFilePath());
            }
        }
        SettingsView _settingsView;
        public void ShowSettingView()
        {
            var left = Host.MainViewLeft;
            var top = Host.MainViewTop;
            if (_settingsView == null)
            {
                _settingsView = new SettingsView
                {
                    DataContext = new ConfigViewModel(Options)
                };
            }
            _settingsView.Topmost = Host.IsTopmost;
            _settingsView.Left = left;
            _settingsView.Top = top;
            _settingsView.Show();
        }

        public string GetSettingsFilePath()
        {
            var dir = Host.SettingsDirPath;
            return Path.Combine(dir, $"{Name}.xml");
        }
        public CommentGeneratorPlugin()
        {

        }
        public void Dispose()
        {
            Dispose(true);
        }
        private bool _disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _writeTimer.Dispose();
                    _deleteTimer.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void OnTopmostChanged(bool isTopmost)
        {
            if (_settingsView != null)
            {
                _settingsView.Topmost = isTopmost;
            }
        }

        private string EmbedIconUrl(ISiteMessage message)
        {
            string str = "";

            if (message is YouTubeLiveSitePlugin.IYouTubeLiveMessage youtubeMessage)
            {
                if (youtubeMessage is YouTubeLiveSitePlugin.IYouTubeLiveComment simpleComment)
                {
                    str = simpleComment.UserIcon.Url;
                }
            }
            else if (message is TwitchSitePlugin.ITwitchMessage twitchMessage)
            {
                if (twitchMessage is TwitchSitePlugin.ITwitchComment simpleComment)
                {
                    str = simpleComment.UserIcon.Url;
                }
            }
            else if (message is OpenrecSitePlugin.IOpenrecMessage openrecMessage)
            {
                if (openrecMessage is OpenrecSitePlugin.IOpenrecComment simpleComment)
                {
                    str = "https://dqd0jw5gvbchn.cloudfront.net/tv/v9.18.0/static/images/favicons/favicon.ico";
                }
            }
            else if (message is NicoSitePlugin.INicoMessage nicoMessage)
            {
                if (nicoMessage is NicoSitePlugin.INicoComment simpleComment)
                {
                    str = simpleComment.ThumbnailUrl;
                }
            }
            else if (message is TwicasSitePlugin.ITwicasMessage twicasMessage)
            {
                if (twicasMessage is TwicasSitePlugin.ITwicasComment simpleComment)
                {
                    str = simpleComment.UserIcon.Url;
                }
            }
            else if (message is MildomSitePlugin.IMildomMessage mildomMessage)
            {
                if (mildomMessage is MildomSitePlugin.IMildomComment simpleComment)
                {
                    str = "https://wia.mildom.com/assets/static/default_avatar.png";
                }
            }
            else if (message is WhowatchSitePlugin.IWhowatchMessage whowatchMessage)
            {
                if (whowatchMessage is WhowatchSitePlugin.IWhowatchComment simpleComment)
                {
                    str = simpleComment.UserIcon.Url;
                }
            }
            else if (message is MirrativSitePlugin.IMirrativMessage mirrativMessage)
            {
                if (mirrativMessage is MirrativSitePlugin.IMirrativComment simpleComment)
                {
                    str = "https://www.mirrativ.co.jp/images/favicon.ico";
                }
            }
            else if (message is LineLiveSitePlugin.ILineLiveMessage lineliveMessage)
            {
                if (lineliveMessage is LineLiveSitePlugin.ILineLiveComment simpleComment)
                {
                    str = simpleComment.UserIconUrl;
                }
            }
            else if (message is ShowRoomSitePlugin.IShowRoomMessage showRoomMessage)
            {
                if (showRoomMessage is ShowRoomSitePlugin.IShowRoomComment simpleComment)
                {
                    str = "https://www.showroom-live.com/assets/img/v3/apple-touch-icon.png";
                }
            }
            else if (message is BigoSitePlugin.IBigoMessage bigoMessage)
            {
                if (bigoMessage is BigoSitePlugin.IBigoComment simpleComment)
                {
                    str = "https://www.bigo.tv/favicon.ico";
                }
            }

            return str;
        }
    }
}
