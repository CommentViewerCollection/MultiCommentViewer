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

namespace CommentViewer.Plugin
{
    [Export(typeof(IPlugin))]
    public class CommentGeneratorPlugin: IPlugin,IDisposable
    {
        private Options _options;
        string _hcgPath = "";
        private System.Timers.Timer _writeTimer;
        private System.Timers.Timer _deleteTimer;
        private SynchronizedCollection<ICommentData> _commentCollection = new SynchronizedCollection<ICommentData>();

        private string CommentXmlPath
        {
            get
            {
                return _hcgPath + "\\comment.xml";
            }
        }
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


        public void OnCommentReceived(ICommentData data)
        {
            if (!_options.IsEnabled || data.IsNgUser)
                return;

            _commentCollection.Add(data);
        }
        public void OnLoaded()
        {
            _options = Options.Load(GetSettingsFilePath());

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
            if (!_options.IsEnabled)
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
                if(count > 1000)
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

        private static string GetHcgPath(string hcgSettingsFilePath)
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

            if (!_options.IsEnabled || _commentCollection.Count == 0)
                return;

            //TODO:各ファイルが存在しなかった時のエラー表示
            if (string.IsNullOrEmpty(_hcgPath) && File.Exists(_options.HcgSettingFilePath))
            {
                _hcgPath = GetHcgPath(_options.HcgSettingFilePath);
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
            lock (_commentCollection)
            {
                var arr = _commentCollection.ToArray();
                _commentCollection.Clear();
                foreach (var data in arr)
                {
                    var item = new XElement("comment", data.Comment);
                    item.SetAttributeValue("no", "0");
                    item.SetAttributeValue("time", ToUnixTime(DateTime.Now));
                    item.SetAttributeValue("owner", 0);
                    item.SetAttributeValue("service", "");
                    if (!string.IsNullOrEmpty(data.Nickname))
                    {
                        item.SetAttributeValue("handle", data.Nickname);
                    }
                    xml.Add(item);
                }                
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
            }
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
            _writeTimer?.Stop();
            _deleteTimer?.Stop();
            if (_options != null)
            {
                Options.Save(_options, GetSettingsFilePath());
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
                    DataContext = new ConfigViewModel(_options)
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
            _writeTimer.Dispose();
            _deleteTimer.Dispose();
        }

        public void OnTopmostChanged(bool isTopmost)
        {
            if (_settingsView != null)
            {
                _settingsView.Topmost = isTopmost;
            }
        }
    }
}
