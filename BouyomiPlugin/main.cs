//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Diagnostics;
//namespace BeamCommentViewer.Plugin
//{
//    public class Bouy
//    {
//        public override void OnCommentReceived(Plugin.CommentData data)
//        {
//            if (!_options.IsEnabled || data.IsNgUser)
//                return;
//            try
//            {
//                //棒読みちゃんが事前に起動されていたらそれを使いたい。
//                //起動していなかったら起動させて、準備ができ次第それ以降のコメントを読んで貰う

//                //とりあえず何も確認せずにコメントを送信する。起動していなかったら例外が起きる。
//                if (_options.IsReadHandleName && !string.IsNullOrEmpty(data.Nickname))
//                {
//                    var nick = data.Nickname;

//                    if (_options.IsAppendNickTitle)
//                        nick += _options.NickTitle;
//                    _bouyomiChanClient.AddTalkTask2(nick);
//                }
//                if (_options.IsReadComment)
//                    _bouyomiChanClient.AddTalkTask2(data.Comment);
//            }
//            catch (System.Runtime.Remoting.RemotingException)
//            {
//                //多分棒読みちゃんが起動していない。
//                if (_bouyomiChanProcess == null && System.IO.File.Exists(_options.BouyomiChanPath))
//                {
//                    _bouyomiChanProcess = Process.Start(_options.BouyomiChanPath);
//                    _bouyomiChanProcess.EnableRaisingEvents = true;
//                    _bouyomiChanProcess.Exited += (s, e) =>
//                    {
//                        _bouyomiChanProcess.Close();
//                        _bouyomiChanProcess = null;
//                    };
//                }
//                //起動するまでの間にコメントが投稿されたらここに来てしまうが諦める。
//            }
//            catch (Exception)
//            {

//            }
//        }
//        public override void OnLoaded()
//        {
//            //base.OnLoaded();
//            _options = Options.Load(GetSettingsFilePath());
//        }
//        public override void OnClosing()
//        {
//            Options.Save(_options, GetSettingsFilePath());
//            base.OnClosing();
//        }

//        public override void Run()
//        {
//        }

//        public override void ShowSettingView()
//        {
//            var left = Host.MainViewLeft;
//            var top = Host.MainViewTop;
//            var view = new Plugin.ConfigView();
//            view.Left = left;
//            view.Top = top;
//            view.DataContext = new Plugin.ConfigViewModel(_options);
//            view.Show();
//        }

//        public string GetSettingsFilePath()
//        {
//            //ここでRemotingExceptionが発生。終了時の処理だが、既にHostがDisposeされてるのかも。
//            var dir = Host.SettingsDirPath;
//            return System.IO.Path.Combine(dir, $"{Name}.xml");
//        }
//        public Bouyomi()
//        {
//            _bouyomiChanClient = new FNF.Utility.BouyomiChanClient();
//        }

//        #region IDisposable Support
//        private bool disposedValue = false; // To detect redundant calls

//        protected override void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//                    // TODO: dispose managed state (managed objects).
//                }

//                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
//                _bouyomiChanClient.Dispose();
//                if (_bouyomiChanProcess != null)
//                {
//                    _bouyomiChanProcess.Close();
//                    _bouyomiChanProcess = null;
//                }

//                // TODO: set large fields to null.

//                disposedValue = true;
//            }
//            base.Dispose(disposing);
//        }
//        #endregion
//    }
//}
