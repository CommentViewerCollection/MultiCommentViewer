using LineLiveSitePlugin;
using MildomSitePlugin;
using MirrativSitePlugin;
using MixerSitePlugin;
using NicoSitePlugin;
using OpenrecSitePlugin;
using PeriscopeSitePlugin;
using Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using TwicasSitePlugin;
using TwitchSitePlugin;
using WhowatchSitePlugin;
using YouTubeLiveSitePlugin;

namespace BouyomiPlugin
{
    static class MessageParts
    {
        public static string ToText(this IEnumerable<IMessagePart> parts)
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
                    else if (part is IMessageImage image)
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
            if (_settingsView != null)
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
            try
            {
                if (_options.IsExecBouyomiChanAtBoot && !IsExecutingProcess("BouyomiChan"))
                {
                    StartBouyomiChan();
                }
            }
            catch (Exception) { }
        }
        /// <summary>
        /// 指定したプロセス名を持つプロセスが起動中か
        /// </summary>
        /// <param name="processName">プロセス名</param>
        /// <returns></returns>
        private bool IsExecutingProcess(string processName)
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }

        public void OnClosing()
        {
            _settingsView?.ForceClose();
            var s = _options.Serialize();
            Host.SaveOptions(GetSettingsFilePath(), s);
            if (_bouyomiChanProcess != null && _options.IsKillBouyomiChan)
            {
                try
                {
                    _bouyomiChanProcess.Kill();
                }
                catch (Exception) { }
            }
        }
        private void StartBouyomiChan()
        {
            if (_bouyomiChanProcess == null && System.IO.File.Exists(_options.BouyomiChanPath))
            {
                _bouyomiChanProcess = Process.Start(_options.BouyomiChanPath);
                _bouyomiChanProcess.EnableRaisingEvents = true;
                _bouyomiChanProcess.Exited += BouyomiChanProcess_Exited;
            }
        }
        public void OnCommentReceived(ICommentData data)
        {
            if (!_options.IsEnabled || data.IsNgUser || data.IsFirstComment || (data.Is184 && !_options.Want184Read))
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
                    TalkText(nick);
                }
                if (_options.IsReadComment)
                    TalkText(data.Comment);
            }
            catch (System.Runtime.Remoting.RemotingException)
            {
                //多分棒読みちゃんが起動していない。
                StartBouyomiChan();
                //起動するまでの間にコメントが投稿されたらここに来てしまうが諦める。
            }
            catch (Exception)
            {

            }
        }

        private void BouyomiChanProcess_Exited(object sender, EventArgs e)
        {
            try
            {
                _bouyomiChanProcess?.Close();//2018/03/25ここで_bouyomiChanProcessがnullになる場合があった
            }
            catch { }
            _bouyomiChanProcess = null;
        }

        public void OnMessageReceived(IMessage message, IMessageMetadata messageMetadata)
        {
            if (!_options.IsEnabled || messageMetadata.IsNgUser || messageMetadata.IsInitialComment || (messageMetadata.Is184 && !_options.Want184Read))
                return;



            string name = null;
            string comment = null;
            if (false) { }
            else if (message is IYouTubeLiveMessage youTubeLiveMessage)
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
            else if (message is IOpenrecMessage openrecMessage)
            {
                switch (openrecMessage.OpenrecMessageType)
                {
                    case OpenrecMessageType.Connected:
                        if (_options.IsOpenrecConnect)
                        {
                            name = null;
                            comment = (openrecMessage as IOpenrecConnected).CommentItems.ToText();
                        }
                        break;
                    case OpenrecMessageType.Disconnected:
                        if (_options.IsOpenrecDisconnect)
                        {
                            name = null;
                            comment = (openrecMessage as IOpenrecDisconnected).CommentItems.ToText();
                        }
                        break;
                    case OpenrecMessageType.Comment:
                        if (_options.IsOpenrecComment)
                        {
                            if (_options.IsOpenrecCommentNickname)
                            {
                                name = (openrecMessage as IOpenrecComment).NameItems.ToText();
                            }
                            comment = (openrecMessage as IOpenrecComment).CommentItems.ToText();
                        }
                        break;
                }
            }
            else if (message is ITwitchMessage twitchMessage)
            {
                switch (twitchMessage.TwitchMessageType)
                {
                    case TwitchMessageType.Connected:
                        if (_options.IsTwitchConnect)
                        {
                            name = null;
                            comment = (twitchMessage as ITwitchConnected).CommentItems.ToText();
                        }
                        break;
                    case TwitchMessageType.Disconnected:
                        if (_options.IsTwitchDisconnect)
                        {
                            name = null;
                            comment = (twitchMessage as ITwitchDisconnected).CommentItems.ToText();
                        }
                        break;
                    case TwitchMessageType.Comment:
                        if (_options.IsTwitchComment)
                        {
                            if (_options.IsTwitchCommentNickname)
                            {
                                name = (twitchMessage as ITwitchComment).DisplayName;
                            }
                            comment = (twitchMessage as ITwitchComment).CommentItems.ToText();
                        }
                        break;
                }
            }
            else if (message is INicoMessage NicoMessage)
            {
                switch (NicoMessage.NicoMessageType)
                {
                    case NicoMessageType.Connected:
                        if (_options.IsNicoConnect)
                        {
                            name = null;
                            comment = (NicoMessage as INicoConnected).CommentItems.ToText();
                        }
                        break;
                    case NicoMessageType.Disconnected:
                        if (_options.IsNicoDisconnect)
                        {
                            name = null;
                            comment = (NicoMessage as INicoDisconnected).CommentItems.ToText();
                        }
                        break;
                    case NicoMessageType.Comment:
                        if (_options.IsNicoComment)
                        {
                            if (_options.IsNicoCommentNickname)
                            {
                                name = (NicoMessage as INicoComment).NameItems.ToText();
                            }
                            comment = (NicoMessage as INicoComment).CommentItems.ToText();
                        }
                        break;
                    case NicoMessageType.Item:
                        if (_options.IsNicoItem)
                        {
                            if (_options.IsNicoItemNickname)
                            {
                                name = (NicoMessage as INicoItem).NameItems.ToText();
                            }
                            comment = (NicoMessage as INicoItem).CommentItems.ToText();
                        }
                        break;
                    case NicoMessageType.Ad:
                        if (_options.IsNicoAd)
                        {
                            name = null;
                            comment = (NicoMessage as INicoAd).CommentItems.ToText();
                        }
                        break;
                }
            }
            else if (message is ITwicasMessage twicasMessage)
            {
                switch (twicasMessage.TwicasMessageType)
                {
                    case TwicasMessageType.Connected:
                        if (_options.IsTwicasConnect)
                        {
                            name = null;
                            comment = (twicasMessage as ITwicasConnected).CommentItems.ToText();
                        }
                        break;
                    case TwicasMessageType.Disconnected:
                        if (_options.IsTwicasDisconnect)
                        {
                            name = null;
                            comment = (twicasMessage as ITwicasDisconnected).CommentItems.ToText();
                        }
                        break;
                    case TwicasMessageType.Comment:
                        if (_options.IsTwicasComment)
                        {
                            if (_options.IsTwicasCommentNickname)
                            {
                                name = (twicasMessage as ITwicasComment).NameItems.ToText();
                            }
                            comment = (twicasMessage as ITwicasComment).CommentItems.ToText();
                        }
                        break;
                }
            }
            else if (message is ILineLiveMessage lineLiveMessage)
            {
                switch (lineLiveMessage.LineLiveMessageType)
                {
                    case LineLiveMessageType.Connected:
                        if (_options.IsLineLiveConnect)
                        {
                            name = null;
                            comment = (lineLiveMessage as ILineLiveConnected).CommentItems.ToText();
                        }
                        break;
                    case LineLiveMessageType.Disconnected:
                        if (_options.IsLineLiveDisconnect)
                        {
                            name = null;
                            comment = (lineLiveMessage as ILineLiveDisconnected).CommentItems.ToText();
                        }
                        break;
                    case LineLiveMessageType.Comment:
                        if (_options.IsLineLiveComment)
                        {
                            if (_options.IsLineLiveCommentNickname)
                            {
                                name = (lineLiveMessage as ILineLiveComment).NameItems.ToText();
                            }
                            comment = (lineLiveMessage as ILineLiveComment).CommentItems.ToText();
                        }
                        break;
                }
            }
            else if (message is IWhowatchMessage whowatchMessage)
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
                    case MirrativMessageType.Item:
                        if (_options.IsMirrativItem)
                        {
                            name = null;
                            comment = (mirrativMessage as IMirrativItem).CommentItems.ToText();
                        }
                        break;
                }
            }
            else if (message is IPeriscopeMessage PeriscopeMessage)
            {
                switch (PeriscopeMessage.PeriscopeMessageType)
                {
                    case PeriscopeMessageType.Connected:
                        if (_options.IsPeriscopeConnect)
                        {
                            name = null;
                            comment = (PeriscopeMessage as IPeriscopeConnected).CommentItems.ToText();
                        }
                        break;
                    case PeriscopeMessageType.Disconnected:
                        if (_options.IsPeriscopeDisconnect)
                        {
                            name = null;
                            comment = (PeriscopeMessage as IPeriscopeDisconnected).CommentItems.ToText();
                        }
                        break;
                    case PeriscopeMessageType.Comment:
                        if (_options.IsPeriscopeComment)
                        {
                            if (_options.IsPeriscopeCommentNickname)
                            {
                                name = (PeriscopeMessage as IPeriscopeComment).NameItems.ToText();
                            }
                            comment = (PeriscopeMessage as IPeriscopeComment).CommentItems.ToText();
                        }
                        break;
                    case PeriscopeMessageType.Join:
                        if (_options.IsPeriscopeJoin)
                        {
                            name = null;
                            comment = (PeriscopeMessage as IPeriscopeJoin).CommentItems.ToText();
                        }
                        break;
                    case PeriscopeMessageType.Leave:
                        if (_options.IsPeriscopeLeave)
                        {
                            name = null;
                            comment = (PeriscopeMessage as IPeriscopeLeave).CommentItems.ToText();
                        }
                        break;
                }
            }
            else if (message is IMixerMessage MixerMessage)
            {
                switch (MixerMessage.MixerMessageType)
                {
                    case MixerMessageType.Connected:
                        if (_options.IsMixerConnect)
                        {
                            name = null;
                            comment = (MixerMessage as IMixerConnected).CommentItems.ToText();
                        }
                        break;
                    case MixerMessageType.Disconnected:
                        if (_options.IsMixerDisconnect)
                        {
                            name = null;
                            comment = (MixerMessage as IMixerDisconnected).CommentItems.ToText();
                        }
                        break;
                    case MixerMessageType.Comment:
                        if (_options.IsMixerComment)
                        {
                            if (_options.IsMixerCommentNickname)
                            {
                                name = (MixerMessage as IMixerComment).NameItems.ToText();
                            }
                            comment = (MixerMessage as IMixerComment).CommentItems.ToText();
                        }
                        break;
                        //case MixerMessageType.Join:
                        //    if (_options.IsMixerJoin)
                        //    {
                        //        name = null;
                        //        comment = (MixerMessage as IMixerJoin).CommentItems.ToText();
                        //    }
                        //    break;
                        //case MixerMessageType.Leave:
                        //    if (_options.IsMixerLeave)
                        //    {
                        //        name = null;
                        //        comment = (MixerMessage as IMixerLeave).CommentItems.ToText();
                        //    }
                        //    break;
                }
            }
            else if (message is IMildomMessage MildomMessage)
            {
                switch (MildomMessage.MildomMessageType)
                {
                    case MildomMessageType.Connected:
                        if (_options.IsMildomConnect)
                        {
                            name = null;
                            comment = (MildomMessage as IMildomConnected).CommentItems.ToText();
                        }
                        break;
                    case MildomMessageType.Disconnected:
                        if (_options.IsMildomDisconnect)
                        {
                            name = null;
                            comment = (MildomMessage as IMildomDisconnected).CommentItems.ToText();
                        }
                        break;
                    case MildomMessageType.Comment:
                        if (_options.IsMildomComment)
                        {
                            if (_options.IsMildomCommentNickname)
                            {
                                name = (MildomMessage as IMildomComment).NameItems.ToText();
                            }
                            comment = (MildomMessage as IMildomComment).CommentItems.ToText();
                        }
                        break;
                    case MildomMessageType.JoinRoom:
                        if (_options.IsMildomJoin)
                        {
                            name = null;
                            comment = (MildomMessage as IMildomJoinRoom).CommentItems.ToText();
                        }
                        break;
                        //case MildomMessageType.Leave:
                        //    if (_options.IsMildomLeave)
                        //    {
                        //        name = null;
                        //        comment = (MildomMessage as IMildomLeave).CommentItems.ToText();
                        //    }
                        //    break;
                }
            }
            else
            {
                if (_options.IsReadHandleName)
                {
                    name = message.NameItems.ToText();
                }
                if (_options.IsReadComment)
                {
                    comment = message.CommentItems.ToText();
                }
            }
            //nameがnullでは無い場合かつUser.Nicknameがある場合はNicknameを採用
            if (!string.IsNullOrEmpty(name) && messageMetadata.User != null && !string.IsNullOrEmpty(messageMetadata.User.Nickname))
            {
                name = messageMetadata.User.Nickname;
            }
            try
            {
                //棒読みちゃんが事前に起動されていたらそれを使いたい。
                //起動していなかったら起動させて、準備ができ次第それ以降のコメントを読んで貰う

                //とりあえず何も確認せずにコメントを送信する。起動していなかったら例外が起きる。

                var dataToRead = "";//棒読みちゃんに読んでもらう文字列
                if (_options.IsReadHandleName && !string.IsNullOrEmpty(name))
                {
                    dataToRead += name;

                    if (_options.IsAppendNickTitle)
                    {
                        dataToRead += _options.NickTitle;
                    }
                }
                if (_options.IsReadComment && !string.IsNullOrEmpty(comment))
                {
                    if (!string.IsNullOrEmpty(dataToRead))//空欄で無い場合、つまり名前も読み上げる場合は名前とコメントの間にスペースを入れる。こうすると名前とコメントの間で一呼吸入れてくれる
                    {
                        dataToRead += " ";
                    }
                    dataToRead += comment;
                }
                TalkText(dataToRead);
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

        private int TalkText(string text)
        {
            if (_options.IsVoiceTypeSpecfied)
            {
                return _bouyomiChanClient.AddTalkTask2(
                    text,
                    _options.VoiceSpeed,
                    _options.VoiceTone,
                    _options.VoiceVolume,
                    (FNF.Utility.VoiceType)Enum.ToObject(typeof(FNF.Utility.VoiceType), _options.VoiceTypeIndex)
                );
            }
            else
            {
                return _bouyomiChanClient.AddTalkTask2(text);
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
            if (_settingsView == null)
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
