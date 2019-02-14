using MirrativSitePlugin;
using Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using WhowatchSitePlugin;
using YouTubeLiveSitePlugin;

namespace BouyomiPlugin
{
    static class MessageParts
    {
        public static string ToText(this IEnumerable<IMessagePart> parts)
        {
            string s="";
            if (parts != null)
            {
                foreach (var part in parts)
                {
                    if (part is IMessageText text)
                    {
                        s += text;
                    }
                }
            }
            return s;
        }
        public static string ToTextWithImageAlt(this IEnumerable<IMessagePart> parts)
        {
            string s = "";
            if (parts != null)
            {
                foreach (var part in parts)
                {
                    if (part is IMessageText text)
                    {
                        s += text;
                    }
                    else if(part is IMessageImage image)
                    {
                        s += image.Alt;
                    }
                }
            }
            return s;
        }
    }
    [Export(typeof(IPlugin))]
    public class BouyomiPlugin : IPlugin, IDisposable
    {
        private readonly FNF.Utility.BouyomiChanClient _bouyomiChanClient;
        private Options _options;
        Process _bouyomiChanProcess;
        public string Name => "棒読みちゃん連携";

        public string Description => "棒読みソフトとうまく連携できるか試してみるプラグインです。";
        public void OnTopmostChanged(bool isTopmost)
        {
            if(_settingsView != null)
            {
                _settingsView.Topmost = isTopmost;
            }
        }
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
            _settingsView?.ForceClose();
            var s = _options.Serialize();
            Host.SaveOptions(GetSettingsFilePath(), s);
        }
        public void OnCommentReceived(ICommentData data)
        {
            if (!_options.IsEnabled || data.IsNgUser || data.IsFirstComment || (data.Is184&& !_options.Want184Read))
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
                        try
                        {
                            _bouyomiChanProcess?.Close();//2018/03/25ここで_bouyomiChanProcessがnullになる場合があった
                        }
                        catch { }
                        _bouyomiChanProcess = null;
                    };
                }
                //起動するまでの間にコメントが投稿されたらここに来てしまうが諦める。
            }
            catch (Exception)
            {

            }
        }
        public void OnMessageReceived(IMessage message, IMessageMetadata messageMetadata)
        {
            if (!_options.IsEnabled || messageMetadata.IsNgUser || messageMetadata.IsInitialComment || (messageMetadata.Is184 && !_options.Want184Read))
                return;



            string name = null;
            string comment = null;
            if(message is IWhowatchMessage whowatchMessage)
            {
                switch (whowatchMessage.WhowatchMessageType)
                {
                    case WhowatchMessageType.Connected:
                        if (_options.IsWhowatchConnect)
                        {
                            name = null;
                            comment = (whowatchMessage as IWhowatchConnected).CommentItems.ToText();
                        }
                        break;
                    case WhowatchMessageType.Disconnected:
                        if (_options.IsWhowatchDisconnect)
                        {
                            name = null;
                            comment = (whowatchMessage as IWhowatchDisconnected).CommentItems.ToText();
                        }
                        break;
                    case WhowatchMessageType.Comment:
                        if (_options.IsWhowatchComment)
                        {
                            if (_options.IsWhowatchCommentNickname)
                            {
                                name = (whowatchMessage as IWhowatchComment).NameItems.ToText();
                            }
                            comment = (whowatchMessage as IWhowatchComment).CommentItems.ToText();
                        }
                        break;
                    case WhowatchMessageType.Item:
                        if (_options.IsWhowatchItem)
                        {
                            if (_options.IsWhowatchItemNickname)
                            {
                                name = (whowatchMessage as IWhowatchItem).NameItems.ToText();
                            }
                            comment = (whowatchMessage as IWhowatchItem).CommentItems.ToText();
                        }
                        break;
                }
            }
            else if(message is IYouTubeLiveMessage youTubeLiveMessage)
            {
                switch (youTubeLiveMessage.YouTubeLiveMessageType)
                {
                    case YouTubeLiveMessageType.Connected:
                        if (_options.IsYouTubeLiveConnect)
                        {
                            name = null;
                            comment = (youTubeLiveMessage as IYouTubeLiveConnected).CommentItems.ToText();
                        }
                        break;
                    case YouTubeLiveMessageType.Disconnected:
                        if (_options.IsYouTubeLiveDisconnect)
                        {
                            name = null;
                            comment = (youTubeLiveMessage as IYouTubeLiveDisconnected).CommentItems.ToText();
                        }
                        break;
                    case YouTubeLiveMessageType.Comment:
                        if (_options.IsYouTubeLiveComment)
                        {
                            if (_options.IsYouTubeLiveCommentNickname)
                            {
                                name = (youTubeLiveMessage as IYouTubeLiveComment).NameItems.ToText();
                            }
                            if (_options.IsYouTubeLiveCommentStamp)
                            {
                                comment = (youTubeLiveMessage as IYouTubeLiveComment).CommentItems.ToTextWithImageAlt();
                            }
                            else
                            {
                                comment = (youTubeLiveMessage as IYouTubeLiveComment).CommentItems.ToText();
                            }
                        }
                        break;
                    case YouTubeLiveMessageType.Superchat:
                        if (_options.IsYouTubeLiveSuperchat)
                        {
                            if (_options.IsYouTubeLiveSuperchatNickname)
                            {
                                name = (youTubeLiveMessage as IYouTubeLiveSuperchat).NameItems.ToText();
                            }
                            //TODO:superchat中のスタンプも読ませるべきでは？
                            comment = (youTubeLiveMessage as IYouTubeLiveSuperchat).CommentItems.ToText();
                        }
                        break;
                }
            }
            else if (message is IMirrativMessage mirrativMessage)
            {
                switch (mirrativMessage.MirrativMessageType)
                {
                    case MirrativMessageType.Connected:
                        if (_options.IsMirrativConnect)
                        {
                            name = null;
                            comment = (mirrativMessage as IMirrativConnected).CommentItems.ToText();
                        }
                        break;
                    case MirrativMessageType.Disconnected:
                        if (_options.IsMirrativDisconnect)
                        {
                            name = null;
                            comment = (mirrativMessage as IMirrativDisconnected).CommentItems.ToText();
                        }
                        break;
                    case MirrativMessageType.Comment:
                        if (_options.IsMirrativComment)
                        {
                            if (_options.IsMirrativCommentNickname)
                            {
                                name = (mirrativMessage as IMirrativComment).NameItems.ToText();
                            }
                            comment = (mirrativMessage as IMirrativComment).CommentItems.ToText();
                        }
                        break;
                    case MirrativMessageType.JoinRoom:
                        if (_options.IsMirrativJoinRoom)
                        {
                            name = null;
                            comment = (mirrativMessage as IMirrativJoinRoom).CommentItems.ToText();
                        }
                        break;
                }
            }

            try
            {
                //棒読みちゃんが事前に起動されていたらそれを使いたい。
                //起動していなかったら起動させて、準備ができ次第それ以降のコメントを読んで貰う

                //とりあえず何も確認せずにコメントを送信する。起動していなかったら例外が起きる。
                if (_options.IsReadHandleName && !string.IsNullOrEmpty(name))
                {
                    var nick = name;

                    if (_options.IsAppendNickTitle)
                        nick += _options.NickTitle;
                    _bouyomiChanClient.AddTalkTask2(nick);
                }
                if (_options.IsReadComment && !string.IsNullOrEmpty(comment))
                    _bouyomiChanClient.AddTalkTask2(comment);
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
                        try
                        {
                            _bouyomiChanProcess?.Close();//2018/03/25ここで_bouyomiChanProcessがnullになる場合があった
                        }
                        catch { }
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
        ConfigView _settingsView;
        public void ShowSettingView()
        {
            if(_settingsView == null)
            {
                _settingsView = new ConfigView
                {
                    DataContext = new ConfigViewModel(_options)
                };
            }
            _settingsView.Topmost = Host.IsTopmost;
            _settingsView.Left = Host.MainViewLeft;
            _settingsView.Top = Host.MainViewTop;
            
            _settingsView.Show();
        }
        public BouyomiPlugin()
        {
            _bouyomiChanClient = new FNF.Utility.BouyomiChanClient();
            _options = new Options();
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
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
                _disposedValue = true;
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
