using Plugin;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
namespace BouyomiPlugin
{
    [Export(typeof(IPlugin))]
    public class BouyomiPlugin : IPlugin, IDisposable
    {
        private readonly FNF.Utility.BouyomiChanClient _bouyomiChanClient;
        private Options _options;
        Process _bouyomiChanProcess;
        public string Name => "棒読みちゃん連携";

        public string Description => "棒読みソフトとうまく連携できるか試してみるプラグインです。";

        public void OnLoaded()
        {
            try
            {
                var s = Host.LoadOptions(GetSettingsFilePath());
                _options.Deserialize(s);
            }
            catch (System.IO.FileNotFoundException) { }
        }
        public void OnClosing()
        {
            var s = _options.Serialize();
            Host.SaveOptions(GetSettingsFilePath(), s);
        }
        public void OnCommentReceived(ICommentData data)
        {
            if (!_options.IsEnabled || data.IsNgUser)
                return;
            try
            {
                //棒読みちゃんが事前に起動されていたらそれを使いたい。
                //起動していなかったら起動させて、準備ができ次第それ以降のコメントを読んで貰う

                //とりあえず何も確認せずにコメントを送信する。起動していなかったら例外が起きる。
                if (_options.IsReadHandleName && !string.IsNullOrEmpty(data.Nickname))
                {
                    var nick = data.Nickname;

                    if (_options.IsAppendNickTitle)
                        nick += _options.NickTitle;
                    _bouyomiChanClient.AddTalkTask2(nick);
                }
                if (_options.IsReadComment)
                    _bouyomiChanClient.AddTalkTask2(data.Comment);
            }
            catch (System.Runtime.Remoting.RemotingException)
            {
                //多分棒読みちゃんが起動していない。
                if (_bouyomiChanProcess == null && System.IO.File.Exists(_options.BouyomiChanPath))
                {
                    _bouyomiChanProcess = Process.Start(_options.BouyomiChanPath);
                    _bouyomiChanProcess.EnableRaisingEvents = true;
                    _bouyomiChanProcess.Exited += (s, e) =>
                    {
                        _bouyomiChanProcess.Close();
                        _bouyomiChanProcess = null;
                    };
                }
                //起動するまでの間にコメントが投稿されたらここに来てしまうが諦める。
            }
            catch (Exception)
            {

            }
        }
        public IPluginHost Host { get; set; }
        public string GetSettingsFilePath()
        {
            //ここでRemotingExceptionが発生。終了時の処理だが、既にHostがDisposeされてるのかも。
            var dir = Host.SettingsDirPath;
            return System.IO.Path.Combine(dir, $"{Name}.xml");
        }
        public void ShowSettingView()
        {
            var view = new ConfigView
            {
                Left = Host.MainViewLeft,
                Top = Host.MainViewTop,
                DataContext = new ConfigViewModel(_options)
            };
            view.Show();
        }
        public BouyomiPlugin()
        {
            _bouyomiChanClient = new FNF.Utility.BouyomiChanClient();
            _options = new Options();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                _bouyomiChanClient.Dispose();
                if (_bouyomiChanProcess != null)
                {
                    _bouyomiChanProcess.Close();
                    _bouyomiChanProcess = null;
                }
                disposedValue = true;
            }
        }
        
        ~BouyomiPlugin()
        {
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
    class OptionsLoader
    {
        public Options Load(string path)
        {
            var options = new Options();
            return options;
        }
        public void Save(Options options, string path)
        {

        }
    }
}
