using BigoSitePlugin;
using LineLiveSitePlugin;
using MildomSitePlugin;
using MirrativSitePlugin;
using NicoSitePlugin;
using OpenrecSitePlugin;
using BLiveSitePlugin;
using MixchSitePlugin;
using PeriscopeSitePlugin;
using Plugin;
using PluginCommon;
using ShowRoomSitePlugin;
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
                    else if (part is IMessageRemoteSvg remoteSvg)
                    {
                        s += remoteSvg.Alt;
                    }
                    else
                    {

                    }
                }
            }
            return s;
        }
    }
    interface ITalker : IDisposable
    {
        void TalkText(string text);

        void TalkText(string text, Int16 voiceSpeed, Int16 voiceTone, Int16 voiceVolume, FNF.Utility.VoiceType voiceType);
    }

    [Serializable]
    public class TalkException : Exception
    {
        public TalkException() { }
        public TalkException(string message) : base(message) { }
        public TalkException(string message, Exception inner) : base(message, inner) { }
        protected TalkException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    [Export(typeof(IPlugin))]
    public class BouyomiPlugin : IPlugin, IDisposable
    {
        //private readonly FNF.Utility.BouyomiChanClient _bouyomiChanClient;
        private readonly ITalker _talker;
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

        private void BouyomiChanProcess_Exited(object sender, EventArgs e)
        {
            try
            {
                _bouyomiChanProcess?.Close();//2018/03/25ここで_bouyomiChanProcessがnullになる場合があった
            }
            catch { }
            _bouyomiChanProcess = null;
        }
        private static (string name, string comment) GetData(ISiteMessage message, Options options)
        {
            string name = null;
            string comment = null;
            if (false) { }
            else if (message is IYouTubeLiveMessage youTubeLiveMessage)
            {
                switch (youTubeLiveMessage.YouTubeLiveMessageType)
                {
                    case YouTubeLiveMessageType.Connected:
                        if (options.IsYouTubeLiveConnect)
                        {
                            name = null;
                            comment = (youTubeLiveMessage as IYouTubeLiveConnected).Text;
                        }
                        break;
                    case YouTubeLiveMessageType.Disconnected:
                        if (options.IsYouTubeLiveDisconnect)
                        {
                            name = null;
                            comment = (youTubeLiveMessage as IYouTubeLiveDisconnected).Text;
                        }
                        break;
                    case YouTubeLiveMessageType.Comment:
                        if (options.IsYouTubeLiveComment)
                        {
                            if (options.IsYouTubeLiveCommentNickname)
                            {
                                name = (youTubeLiveMessage as IYouTubeLiveComment).NameItems.ToText();
                            }
                            if (options.IsYouTubeLiveCommentStamp)
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
                        if (options.IsYouTubeLiveSuperchat)
                        {
                            var superChat = youTubeLiveMessage as IYouTubeLiveSuperchat;
                            if (options.IsYouTubeLiveSuperchatNickname)
                            {
                                name = superChat.NameItems.ToText();
                            }
                            //TODO:superchat中のスタンプも読ませるべきでは？
                            var text = superChat.CommentItems.ToText();
                            var amount = superChat.PurchaseAmount;
                            comment = amount + Environment.NewLine + text;
                        }
                        break;
                    case YouTubeLiveMessageType.Membership:
                        if (options.IsYouTubeLiveMembership)
                        {
                            var membership = youTubeLiveMessage as IYouTubeLiveMembership;
                            if (options.IsYouTubeLiveMembershipNickname)
                            {
                                name = membership.NameItems.ToText();
                            }
                            comment = membership.CommentItems.ToText();
                        }
                        break;
                }
            }
            else if (message is IOpenrecMessage openrecMessage)
            {
                switch (openrecMessage.OpenrecMessageType)
                {
                    case OpenrecMessageType.Connected:
                        if (options.IsOpenrecConnect)
                        {
                            name = null;
                            comment = (openrecMessage as IOpenrecConnected).Text;
                        }
                        break;
                    case OpenrecMessageType.Disconnected:
                        if (options.IsOpenrecDisconnect)
                        {
                            name = null;
                            comment = (openrecMessage as IOpenrecDisconnected).Text;
                        }
                        break;
                    case OpenrecMessageType.Comment:
                        if (options.IsOpenrecComment)
                        {
                            if (options.IsOpenrecCommentNickname)
                            {
                                name = (openrecMessage as IOpenrecComment).NameItems.ToText();
                            }
                            comment = (openrecMessage as IOpenrecComment).MessageItems.ToText();
                        }
                        break;
                }
            }
            else if (message is IBLiveMessage bliveMessage)
            {
                switch (bliveMessage.BLiveMessageType)
                {
                    case BLiveMessageType.Connected:
                        if (options.IsBLiveConnect)
                        {
                            name = null;
                            comment = (bliveMessage as IBLiveConnected).Text;
                        }
                        break;
                    case BLiveMessageType.Disconnected:
                        if (options.IsBLiveDisconnect)
                        {
                            name = null;
                            comment = (bliveMessage as IBLiveDisconnected).Text;
                        }
                        break;
                    case BLiveMessageType.Comment:
                        if (options.IsBLiveComment)
                        {
                            if (options.IsBLiveCommentNickname)
                            {
                                name = (bliveMessage as IBLiveComment).NameItems.ToText();
                            }
                            comment = (bliveMessage as IBLiveComment).MessageItems.ToText();
                        }
                        break;
                }
            }
            else if (message is IMixchMessage mixchMessage)
            {
                switch (mixchMessage.MixchMessageType)
                {
                    case MixchMessageType.Comment:
                        if (options.IsMixchComment && (!options.IsMixchCommentOnlyFirst || mixchMessage.IsFirstComment))
                        {
                            if (options.IsMixchCommentNickname)
                            {
                                name = mixchMessage.NameItems.ToText();
                            }
                            comment = mixchMessage.MessageItems.ToText();
                        }
                        break;
                    case MixchMessageType.SuperComment:
                    case MixchMessageType.Stamp:
                    case MixchMessageType.PoiPoi:
                    case MixchMessageType.Item:
                    case MixchMessageType.CoinBox:
                        if (options.IsMixchItem)
                        {
                            if (options.IsMixchItemNickname)
                            {
                                name = mixchMessage.NameItems.ToText();
                            }
                            comment = mixchMessage.MessageItems.ToText();
                        }
                        break;
                    case MixchMessageType.Share:
                    case MixchMessageType.EnterNewbie:
                    case MixchMessageType.EnterLevel:
                    case MixchMessageType.Follow:
                    case MixchMessageType.EnterFanclub:
                        if (options.IsMixchSystem)
                        {
                            comment = mixchMessage.MessageItems.ToText();
                        }
                        break;
                }
            }
            else if (message is ITwitchMessage twitchMessage)
            {
                switch (twitchMessage.TwitchMessageType)
                {
                    case TwitchMessageType.Connected:
                        if (options.IsTwitchConnect)
                        {
                            name = null;
                            comment = (twitchMessage as ITwitchConnected).Text;
                        }
                        break;
                    case TwitchMessageType.Disconnected:
                        if (options.IsTwitchDisconnect)
                        {
                            name = null;
                            comment = (twitchMessage as ITwitchDisconnected).Text;
                        }
                        break;
                    case TwitchMessageType.Comment:
                        if (options.IsTwitchComment)
                        {
                            if (options.IsTwitchCommentNickname)
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
                        if (options.IsNicoConnect)
                        {
                            name = null;
                            comment = (NicoMessage as INicoConnected).Text;
                        }
                        break;
                    case NicoMessageType.Disconnected:
                        if (options.IsNicoDisconnect)
                        {
                            name = null;
                            comment = (NicoMessage as INicoDisconnected).Text;
                        }
                        break;
                    case NicoMessageType.Comment:
                        if (options.IsNicoComment)
                        {
                            if (options.IsNicoCommentNickname)
                            {
                                name = (NicoMessage as INicoComment).UserName;
                            }
                            comment = (NicoMessage as INicoComment).Text;
                        }
                        break;
                    case NicoMessageType.Item:
                        if (options.IsNicoItem)
                        {
                            if (options.IsNicoItemNickname)
                            {
                                //name = (NicoMessage as INicoItem).NameItems.ToText();
                            }
                            comment = (NicoMessage as INicoGift).Text;
                        }
                        break;
                    case NicoMessageType.Ad:
                        if (options.IsNicoAd)
                        {
                            name = null;
                            comment = (NicoMessage as INicoAd).Text;
                        }
                        break;
                    case NicoMessageType.Spi:
                        if (options.IsNicoSpi)
                        {
                            name = null;
                            comment = (NicoMessage as INicoSpi).Text;
                        }
                        break;
                }
            }
            else if (message is ITwicasMessage twicasMessage)
            {
                switch (twicasMessage.TwicasMessageType)
                {
                    case TwicasMessageType.Connected:
                        if (options.IsTwicasConnect)
                        {
                            name = null;
                            comment = (twicasMessage as ITwicasConnected).Text;
                        }
                        break;
                    case TwicasMessageType.Disconnected:
                        if (options.IsTwicasDisconnect)
                        {
                            name = null;
                            comment = (twicasMessage as ITwicasDisconnected).Text;
                        }
                        break;
                    case TwicasMessageType.Comment:
                        if (options.IsTwicasComment)
                        {
                            if (options.IsTwicasCommentNickname)
                            {
                                name = (twicasMessage as ITwicasComment).UserName;
                            }
                            comment = (twicasMessage as ITwicasComment).CommentItems.ToText();
                        }
                        break;
                    case TwicasMessageType.Item:
                        if (options.IsTwicasItem)
                        {
                            if (options.IsTwicasItemNickname)
                            {
                                name = (twicasMessage as ITwicasItem).UserName;
                            }
                            comment = (twicasMessage as ITwicasItem).CommentItems.ToTextWithImageAlt();
                        }
                        break;
                }
            }
            else if (message is ILineLiveMessage lineLiveMessage)
            {
                switch (lineLiveMessage.LineLiveMessageType)
                {
                    case LineLiveMessageType.Connected:
                        if (options.IsLineLiveConnect)
                        {
                            name = null;
                            comment = (lineLiveMessage as ILineLiveConnected).Text;
                        }
                        break;
                    case LineLiveMessageType.Disconnected:
                        if (options.IsLineLiveDisconnect)
                        {
                            name = null;
                            comment = (lineLiveMessage as ILineLiveDisconnected).Text;
                        }
                        break;
                    case LineLiveMessageType.Comment:
                        if (options.IsLineLiveComment)
                        {
                            if (options.IsLineLiveCommentNickname)
                            {
                                name = (lineLiveMessage as ILineLiveComment).DisplayName;
                            }
                            comment = (lineLiveMessage as ILineLiveComment).Text;
                        }
                        break;
                }
            }
            else if (message is IWhowatchMessage whowatchMessage)
            {
                switch (whowatchMessage.WhowatchMessageType)
                {
                    case WhowatchMessageType.Connected:
                        if (options.IsWhowatchConnect)
                        {
                            name = null;
                            comment = (whowatchMessage as IWhowatchConnected).Text;
                        }
                        break;
                    case WhowatchMessageType.Disconnected:
                        if (options.IsWhowatchDisconnect)
                        {
                            name = null;
                            comment = (whowatchMessage as IWhowatchDisconnected).Text;
                        }
                        break;
                    case WhowatchMessageType.Comment:
                        if (options.IsWhowatchComment)
                        {
                            if (options.IsWhowatchCommentNickname)
                            {
                                name = (whowatchMessage as IWhowatchComment).UserName;
                            }
                            comment = (whowatchMessage as IWhowatchComment).Comment;
                        }
                        break;
                    case WhowatchMessageType.Item:
                        if (options.IsWhowatchItem)
                        {
                            if (options.IsWhowatchItemNickname)
                            {
                                name = (whowatchMessage as IWhowatchItem).UserName;
                            }
                            comment = (whowatchMessage as IWhowatchItem).Comment;
                        }
                        break;
                }
            }
            else if (message is IMirrativMessage mirrativMessage)
            {
                switch (mirrativMessage.MirrativMessageType)
                {
                    case MirrativMessageType.Connected:
                        if (options.IsMirrativConnect)
                        {
                            name = null;
                            comment = (mirrativMessage as IMirrativConnected).Text;
                        }
                        break;
                    case MirrativMessageType.Disconnected:
                        if (options.IsMirrativDisconnect)
                        {
                            name = null;
                            comment = (mirrativMessage as IMirrativDisconnected).Text;
                        }
                        break;
                    case MirrativMessageType.Comment:
                        if (options.IsMirrativComment)
                        {
                            if (options.IsMirrativCommentNickname)
                            {
                                name = (mirrativMessage as IMirrativComment).UserName;
                            }
                            comment = (mirrativMessage as IMirrativComment).Text;
                        }
                        break;
                    case MirrativMessageType.JoinRoom:
                        if (options.IsMirrativJoinRoom)
                        {
                            name = null;
                            comment = (mirrativMessage as IMirrativJoinRoom).Text;
                        }
                        break;
                    case MirrativMessageType.Item:
                        if (options.IsMirrativItem)
                        {
                            name = null;
                            comment = (mirrativMessage as IMirrativItem).Text;
                        }
                        break;
                }
            }
            else if (message is IPeriscopeMessage PeriscopeMessage)
            {
                switch (PeriscopeMessage.PeriscopeMessageType)
                {
                    case PeriscopeMessageType.Connected:
                        if (options.IsPeriscopeConnect)
                        {
                            name = null;
                            comment = (PeriscopeMessage as IPeriscopeConnected).Text;
                        }
                        break;
                    case PeriscopeMessageType.Disconnected:
                        if (options.IsPeriscopeDisconnect)
                        {
                            name = null;
                            comment = (PeriscopeMessage as IPeriscopeDisconnected).Text;
                        }
                        break;
                    case PeriscopeMessageType.Comment:
                        if (options.IsPeriscopeComment)
                        {
                            if (options.IsPeriscopeCommentNickname)
                            {
                                name = (PeriscopeMessage as IPeriscopeComment).DisplayName;
                            }
                            comment = (PeriscopeMessage as IPeriscopeComment).Text;
                        }
                        break;
                    case PeriscopeMessageType.Join:
                        if (options.IsPeriscopeJoin)
                        {
                            name = null;
                            comment = (PeriscopeMessage as IPeriscopeJoin).Text;
                        }
                        break;
                    case PeriscopeMessageType.Leave:
                        if (options.IsPeriscopeLeave)
                        {
                            name = null;
                            comment = (PeriscopeMessage as IPeriscopeLeave).Text;
                        }
                        break;
                }
            }
            else if (message is IMildomMessage MildomMessage)
            {
                switch (MildomMessage.MildomMessageType)
                {
                    case MildomMessageType.Connected:
                        if (options.IsMildomConnect)
                        {
                            name = null;
                            comment = (MildomMessage as IMildomConnected).Text;
                        }
                        break;
                    case MildomMessageType.Disconnected:
                        if (options.IsMildomDisconnect)
                        {
                            name = null;
                            comment = (MildomMessage as IMildomDisconnected).Text;
                        }
                        break;
                    case MildomMessageType.Comment:
                        if (options.IsMildomComment)
                        {
                            if (options.IsMildomCommentNickname)
                            {
                                name = (MildomMessage as IMildomComment).UserName;
                            }
                            if (options.IsMildomCommentStampId)
                            {
                                comment = (MildomMessage as IMildomComment).CommentItems.ToTextWithImageAlt();
                            }
                            else
                            {
                                comment = (MildomMessage as IMildomComment).CommentItems.ToText();
                            }
                        }
                        break;
                    case MildomMessageType.JoinRoom:
                        if (options.IsMildomJoin)
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
                    case MildomMessageType.Gift:
                        if (options.IsMildomGift)
                        {
                            if (options.IsMildomGiftNickname)
                            {
                                name = (MildomMessage as IMildomGift).UserName;
                            }
                            var giftName = ((IMildomGift)MildomMessage).GiftName;
                            comment = $"{giftName}を贈りました";
                        }
                        break;
                }
            }
            else if (message is IShowRoomMessage showroomMessage)
            {
                switch (showroomMessage.ShowRoomMessageType)
                {
                    case ShowRoomMessageType.Comment:
                        if (options.IsShowRoomComment)
                        {
                            if (options.IsShowRoomCommentNickname)
                            {
                                name = (showroomMessage as IShowRoomComment).UserName;
                            }
                            comment = (showroomMessage as IShowRoomComment).Text;
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (message is IBigoMessage bigoMessage)
            {
                switch (bigoMessage.BigoMessageType)
                {
                    case BigoMessageType.Comment:
                        if (options.IsBigoLiveComment)
                        {
                            if (options.IsBigoLiveCommentNickname)
                            {
                                name = (bigoMessage as IBigoComment).Name;
                            }
                            comment = (bigoMessage as IBigoComment).Message;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {

            }
            return (name, comment);
        }
        public void OnMessageReceived(ISiteMessage message, IMessageMetadata messageMetadata)
        {
            if (!_options.IsEnabled || messageMetadata.IsNgUser || messageMetadata.IsInitialComment || (messageMetadata.Is184 && !_options.Want184Read))
                return;

            var (name, comment) = GetData(message, _options);

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
                if (string.IsNullOrEmpty(dataToRead))
                {
                    return;
                }
                TalkText(dataToRead);
            }
            catch (TalkException)
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

        private void TalkText(string text)
        {
            if (_options.IsVoiceTypeSpecfied)
            {
                _talker.TalkText(text, (Int16)_options.VoiceSpeed,
                   (Int16)_options.VoiceTone,
                   (Int16)_options.VoiceVolume, (FNF.Utility.VoiceType)Enum.ToObject(typeof(FNF.Utility.VoiceType), _options.VoiceTypeIndex));
            }
            else
            {
                _talker.TalkText(text);
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
            _talker = new IpcTalker();
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
                _talker.Dispose();
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
