using Common;
using Newtonsoft.Json;
using ryu_s.BrowserCookie;
using ryu_s.YouTubeLive.Message;
using ryu_s.YouTubeLive.Message.Action;
using ryu_s.YouTubeLive.Message.Continuation;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YouTubeLiveSitePlugin.Input;
using YouTubeLiveSitePlugin.Test2;

namespace YouTubeLiveSitePlugin.Input
{
    interface IInput
    {
        string Raw { get; }
    }
    class Vid : IInput
    {
        public Vid(string vid)
        {
            Raw = vid;
        }
        public string Raw { get; }
        public override string ToString()
        {
            return Raw;
        }
    }
    class WatchUrl : IInput
    {
        public string Raw { get; }
        public string Vid { get; }
        public WatchUrl(string watchUrl)
        {
            Raw = watchUrl;
            if (!VidResolver.TryWatch(watchUrl, out var vid))
            {
                throw new ArgumentException(nameof(vid));
            }
            Vid = vid;
        }
    }
    class ChannelUrl : IInput
    {
        public string Raw { get; }
        public string ChannelId { get; }
        public ChannelUrl(string channelUrl)
        {
            Raw = channelUrl;
            ChannelId = VidResolver.ExtractChannelId(channelUrl);
        }
    }
    class UserUrl : IInput
    {
        public string Raw { get; }
        public string UserId { get; }
        public UserUrl(string userUrl)
        {
            Raw = userUrl;
            UserId = VidResolver.ExtractUserId(userUrl);
        }
    }
    class StudioUrl : IInput
    {
        public string Raw { get; }
        public string Vid { get; }
        public StudioUrl(string studioUrl)
        {
            Raw = studioUrl;
            Vid = VidResolver.ExtractVidFromStudioUrl(studioUrl);
        }
    }
    class CustomChannelUrl : IInput
    {
        public string Raw { get; }
        public string CustomChannelId { get; }
        public CustomChannelUrl(string customChannelUrl)
        {
            Raw = customChannelUrl;
            CustomChannelId = VidResolver.ExtractCustomChannelId(customChannelUrl);
        }
    }
    class InvalidInput : IInput
    {
        public string Raw { get; }
        public InvalidInput(string input)
        {
            Raw = input;
        }
    }
}
namespace YouTubeLiveSitePlugin.Next
{
    /// <summary>
    /// 接続が切れた原因
    /// </summary>
    enum ReasonForDisconnection
    {
        /// <summary>
        /// 原因不明
        /// </summary>
        Unknown,
        /// <summary>
        /// ユーザーによって切断された
        /// </summary>
        User,
        /// <summary>
        /// 配信が終了した
        /// </summary>
        Finished,
        /// <summary>
        /// 配信サイトの仕様変更に未対応
        /// </summary>
        SpecChanged,
        /// <summary>
        /// YouTubeサーバーからリロード指示があった
        /// </summary>
        Reload,
        ChatUnavailable,
    }
    //class ChatProviderNext
    //{
    //    private readonly YtCfg _ytCfg;
    //    private readonly YtInitialData _ytInitialData;
    //    public event EventHandler<IInternalMessage> MessageReceived;
    //    string _continuation;
    //    public async Task ReceiveAsync(string vid, CookieContainer cc)
    //    {
    //        _continuation = _ytInitialData.ChatContinuation.AllChatContinuation;

    //    }
    //    public void Disconnect()
    //    {

    //    }
    //    public ChatProviderNext(YtCfg ytCfg, YtInitialData ytInitialData)
    //    {
    //        _ytCfg = ytCfg;
    //        _ytInitialData = ytInitialData;
    //    }
    //}
    /// <summary>
    /// ログイン状態で処理を分ける場面で活躍するやつ
    /// </summary>
    /// ログイン状態といえばCookieだからCookieContainerを持たせようか考えたけど、ログインの有無とは関係無いからやめた。
    /// 特殊化したbool
    /// データを持たせないのならenumでも良いかも
    interface ILoginState { }
    class LoggedIn : ILoginState { }
    class NotLoggedin : ILoginState { }
    class ChatProvider2
    {
        //public event EventHandler<IInternalMessage> MessageReceived;
        public event EventHandler<IAction> MessageReceived;
        public event EventHandler<bool> LoggedInStateChanged;
        public event EventHandler<InfoData> InfoReceived;
        private void SendSystemInfo(string message, InfoType type)
        {
            InfoReceived?.Invoke(this, new InfoData { Comment = message, Type = type });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vid"></param>
        /// <param name="ytCfg"></param>
        /// <param name="cc"></param>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        /// <exception cref="ReloadException"></exception>
        /// <exception cref="SpecChangedException"></exception>
        public async Task ReceiveAsync(string vid, YtInitialData ytInitialData1, YtCfg ytCfg, CookieContainer cc, ILoginState loginInfo)
        {
            if (_cts != null)
            {
                throw new InvalidOperationException("receiving");
            }
            _cts = new CancellationTokenSource();
            try
            {
                await ReceiveInternalAsync(ytInitialData1, ytCfg, cc, loginInfo);
            }
            finally
            {
                _cts = null;
            }
        }
        public async Task ReceiveInternalAsync(YtInitialData ytInitialData1, YtCfg ytCfg, CookieContainer cc, ILoginState loginInfo)
        {
            var dataToPost = new DataToPost(ytCfg);
            string initialContinuation;
            if (_siteOptions.IsAllChat)
            {
                initialContinuation = ytInitialData1.AllChatContinuation;// ytInitialData.ChatContinuation().AllChatContinuation;
            }
            else
            {
                initialContinuation = ytInitialData1.JouiChatContinuation;// ytInitialData.ChatContinuation().JouiChatContinuation;
            }
            dataToPost.SetContinuation(initialContinuation);

            while (!_cts.IsCancellationRequested)
            {
                //例外はここではcatchしない。
                var getLiveChat = await Tools.GetGetLiveChat(dataToPost, ytCfg.InnertubeApiKey, cc, loginInfo, _logger);
                var actions = getLiveChat.Actions;
                var continuation = getLiveChat.Continuation;// s.GetContinuation();
                if (continuation is null)
                {
                    throw new ContinuationNotExistsException();
                }
                if (continuation is ReloadContinuationData reload)
                {
                    throw new ReloadException();
                }
                else if (continuation is TimedContinuationData timed)
                {
                    dataToPost.SetContinuation(timed.Continaution);
                    await ProcessAction(actions, timed.TimeoutMs);
                }
                else if (continuation is InvalidationContinuationData invalid)
                {
                    dataToPost.SetContinuation(invalid.Continaution);
                    await ProcessAction(actions, invalid.TimeoutMs);
                }
                else if (continuation is UnknownContinuationData unknown)
                {
                    throw new SpecChangedException(unknown.Raw);
                }
                else
                {
                    //ここには来ない予定
                    throw new SpecChangedException("");
                }
                //if (continuation is ReloadContinuation reload)
                //{
                //    throw new ReloadException();
                //}
                //else if (continuation is UnknownContinuation unknown)
                //{
                //    throw new SpecChangedException(unknown.Raw);
                //}
                //dataToPost.SetContinuation(continuation.Continuation);
                //var timeoutMs = Math.Max(continuation.TimeoutMs, 1000);
                //if (actions.Count > 0)
                //{
                //    var waitTime = timeoutMs / actions.Count;
                //    foreach (var action in actions)
                //    {
                //        ProcessAction(action);
                //        try
                //        {
                //            await Task.Delay(waitTime, _cts.Token);
                //        }
                //        catch (TaskCanceledException)
                //        {
                //            return;
                //        }
                //    }
                //}
                //else
                //{
                //    var waitTime = timeoutMs;
                //    try
                //    {
                //        await Task.Delay(waitTime, _cts.Token);
                //    }
                //    catch (TaskCanceledException)
                //    {
                //        return;
                //    }
                //}
            }
        }
        private async Task ProcessAction(List<IAction> actions, int timeoutMs)
        {
            var timeoutMs_ = Math.Max(timeoutMs, 1000);
            if (actions.Count > 0)
            {
                var waitTime = timeoutMs_ / actions.Count;
                foreach (var action in actions)
                {
                    ProcessAction(action);
                    try
                    {
                        await Task.Delay(waitTime, _cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                }
            }
            else
            {
                var waitTime = timeoutMs_;
                try
                {
                    await Task.Delay(waitTime, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }
        private void ProcessAction(IAction action)
        {
            MessageReceived?.Invoke(this, action);
        }
        //private void ProcessAction(IInternalMessage action)
        //{
        //    switch (action)
        //    {
        //        case InternalComment comment:
        //            MessageReceived?.Invoke(this, comment);
        //            break;
        //        case InternalSuperChat superChat:
        //            MessageReceived?.Invoke(this, superChat);
        //            break;
        //        case InternalMembership membership:
        //            MessageReceived?.Invoke(this, membership);
        //            break;
        //        default:
        //            break;
        //    }
        //}
        public void Disconnect()
        {
            _cts?.Cancel();
        }
        public ChatProvider2(IYouTubeLiveSiteOptions siteOptions, ILogger logger)
        {
            _siteOptions = siteOptions;
            _logger = logger;
        }
        private CancellationTokenSource _cts;
        private readonly IYouTubeLiveSiteOptions _siteOptions;
        private readonly ILogger _logger;
    }
    class CommentProviderNext : CommentProviderBase, IYouTubeCommentProvider
    {
        ChatProvider2 _chatProvider;
        DateTime? _startedAt;
        System.Timers.Timer _elapsedTimer = new System.Timers.Timer();
        private async Task InitElapsedTimer(string vid)
        {
            var html = await _server.GetAsync($"https://www.youtube.com/watch?v={vid}");
            var liveBroadcastDetails = Test2.Tools.ExtractLiveBroadcastDetailsFromLivePage(html);
            if (liveBroadcastDetails != null)
            {
                dynamic d = Newtonsoft.Json.JsonConvert.DeserializeObject(liveBroadcastDetails);
                if (d.ContainsKey("startTimestamp"))
                {
                    var startedStr = (string)d.startTimestamp;
                    _startedAt = DateTime.Parse(startedStr);
                    _elapsedTimer.Interval = 500;
                    _elapsedTimer.Elapsed += (s, e) =>
                    {
                        if (!_startedAt.HasValue) return;
                        var elapsed = DateTime.Now - _startedAt.Value;
                        RaiseMetadataUpdated(new Test2.Metadata
                        {
                            Elapsed = Tools.ToElapsedString(elapsed),
                        });
                    };
                    _elapsedTimer.Enabled = true;
                }
            }
        }
        private static async Task<string> GetLiveChat(string vid, CookieContainer cc)
        {
            var url = $"https://www.youtube.com/live_chat?is_popout=1&v={vid}";
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cc,
            };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36");
            var res = await client.GetAsync(url);
            return await res.Content.ReadAsStringAsync();
        }
        ReasonForDisconnection? _reason;
        /// <summary>
        /// 何らかの理由で切断/中断されるまでコメントを取得し続ける
        /// 例外は投げないようにしたい-.
        /// </summary>
        /// <returns>再接続すべきか</returns>
        private async Task<ReasonForDisconnection> ConnectOnceAsync(string vid, CookieContainer cc,ChatProvider2 chatProvider, MetaDataYoutubeiProvider metaProvider)
        {
            _reason = null;

            var liveChatHtml = await GetLiveChat(vid, cc);
            var liveChat = LiveChat.Parse(liveChatHtml);

            var ytCfgStr = Tools.ExtractYtCfg(liveChatHtml);
            //var ytCfg = new YtCfgOld(ytCfgStr);
            var ytCfg = liveChat.YtCfg;
            var ytInitialData = Tools.ExtractYtInitialData(liveChatHtml);
            if (!ytInitialData.CanChat)
            {
                SendSystemInfo("このライブストリームではチャットは無効です。", InfoType.Notice);
                return ReasonForDisconnection.ChatUnavailable;
            }
            var loginInfo = Tools.CreateLoginInfo(liveChat.YtCfg.IsLoggedIn);
            //ログイン済みユーザの正常にコメントが取得できるようになったら以下のコードは不要
            //---ここから---
            if (loginInfo is LoggedIn)
            {
                var k = Tools.GetSapiSid(cc);
                if (k == null)
                {
                    //SIDが無い。ログイン済み判定なのにSIDが無い場合が散見されるが原因不明。強制的に未ログインにする。
                    var cookies = Tools.ExtractCookies(cc);
                    var cver = ytInitialData.Cver;
                    var keys = string.Join(",", cookies.Select(c => c.Name));
                    _logger.LogException(new Exception(), "", $"cver={cver},keys={keys}");
                    cc = new CookieContainer();
                }
            }
            //---ここまで---
            SetLoggedInState(liveChat.YtCfg.IsLoggedIn);
            _postCommentCoodinator = new DataCreator(ytInitialData, liveChat.YtCfg.InnertubeApiKey, liveChat.YtCfg.DelegatedSessionId, cc);
            foreach (var action in liveChat.YtInitialData.Actions)
            {
                OnMessageReceived(action, true);
            }

            var chatTask = chatProvider.ReceiveAsync(vid, liveChat.YtInitialData, ytCfg, cc, loginInfo);
            var metaTask = metaProvider.ReceiveAsync(ytCfg, vid, cc);

            var tasks = new List<Task>
            {
                chatTask,
                metaTask
            };
            while (tasks.Count > 0)
            {
                var t = await Task.WhenAny(tasks);
                if (t == chatTask)
                {
                    metaProvider.Disconnect();
                    try
                    {
                        await metaTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    tasks.Remove(metaTask);
                    try
                    {
                        await chatTask;
                    }
                    catch (ContinuationNotExistsException)
                    {
                        _reason = ReasonForDisconnection.Finished;
                        break;
                    }
                    catch (ChatUnavailableException ex)
                    {
                        _logger.LogException(ex);
                        _reason = ReasonForDisconnection.Finished;
                    }
                    catch (ReloadException)
                    {
                        _reason = ReasonForDisconnection.Reload;
                    }
                    catch (SpecChangedException ex)
                    {
                        _logger.LogException(ex);
                        _reason = ReasonForDisconnection.SpecChanged;
                    }
                    catch (Exception ex)
                    {
                        SendSystemInfo(ex.Message, InfoType.Error);
                        //意図しない切断
                        //ただし、サーバーからReloadメッセージが来た場合と違って、単純にリロードすれば済む問題ではない。
                        _logger.LogException(ex);
                        await Task.Delay(1000);
                        _reason = ReasonForDisconnection.Unknown;
                    }
                    tasks.Remove(chatTask);

                    return _reason == null ? ReasonForDisconnection.Unknown : _reason.Value;
                }
                else
                {
                    try
                    {
                        await metaTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    tasks.Remove(metaTask);
                }
            }
            //ここに来ることは無いと思う。
            return _reason==null ? ReasonForDisconnection.Unknown : _reason.Value;
        }
        private async Task ConnectInternalAsync(IInput input, IBrowserProfile browserProfile)
        {
            var resolver = new VidResolver();
            var vidResult = await resolver.GetVid(_server, input);
            string vid;
            switch (vidResult)
            {
                case VidResult single:
                    vid = single.Vid;
                    break;
                case MultiVidsResult multi:
                    SendSystemInfo("このチャンネルでは複数のライブが配信中です。", InfoType.Notice);
                    foreach (var v in multi.Vids)
                    {
                        SendSystemInfo(v, InfoType.Notice);//titleも欲しい
                    }
                    return;
                case NoVidResult no:
                    SendSystemInfo("このチャンネルでは生放送をしていないようです", InfoType.Error);
                    return;
                default:
                    throw new NotImplementedException();
            }
            _cc = CreateCookieContainer(browserProfile);
            await InitElapsedTimer(vid);
            _chatProvider = new ChatProvider2(_siteOptions, _logger);
            _chatProvider.MessageReceived += ChatProvider_MessageReceived;
            _chatProvider.LoggedInStateChanged += _chatProvider_LoggedInStateChanged;
            _chatProvider.InfoReceived += ChatProvider_InfoReceived;

            var metaProvider = new MetaDataYoutubeiProvider(_server, _logger);
            metaProvider.InfoReceived += MetaProvider_InfoReceived;
            metaProvider.MetadataReceived += MetaProvider_MetadataReceived;

        reload:
            var reason= ReasonForDisconnection.Unknown;
            try
            {
                reason = await ConnectOnceAsync(vid,_cc,_chatProvider,metaProvider);
            }
            catch(Exception ex)
            {
                _logger.LogException(ex);
                //TODO: do something
            }
            if(reason == ReasonForDisconnection.User)
            {
                //何もしないでこのまま終了。
            }
            else if(reason == ReasonForDisconnection.Finished)
            {
                SendSystemInfo("配信が終了しました", InfoType.Notice);
            }
            else if(reason == ReasonForDisconnection.SpecChanged)
            {
                SendSystemInfo("YouTubeの仕様変更があったため、コメント取得を継続できません", InfoType.Error);
            }
            else if(reason == ReasonForDisconnection.ChatUnavailable)
            {
                SendSystemInfo("この配信ではチャットが無効になっています", InfoType.Error);
            }
            else
            {
                SendSystemInfo("エラーが発生したためサーバーとの接続が切断されましたが、自動的に再接続します", InfoType.Error);
                goto reload;
            }

            _chatProvider.MessageReceived -= ChatProvider_MessageReceived;
            _chatProvider.LoggedInStateChanged -= _chatProvider_LoggedInStateChanged;
            _chatProvider.InfoReceived -= ChatProvider_InfoReceived;
            metaProvider.InfoReceived -= MetaProvider_InfoReceived;
            metaProvider.MetadataReceived -= MetaProvider_MetadataReceived;
        }

        private void ChatProvider_InfoReceived(object sender, InfoData e)
        {
            SendSystemInfo(e.Comment, e.Type);
        }

        private void MetaProvider_MetadataReceived(object sender, SitePlugin.IMetadata e)
        {
            RaiseMetadataUpdated(e);
        }

        private void MetaProvider_InfoReceived(object sender, InfoData e)
        {
            SendSystemInfo(e.Comment, e.Type);
        }

        private void _chatProvider_LoggedInStateChanged(object sender, bool e)
        {
            SetLoggedInState(e);
        }
        private readonly SynchronizedCollection<string> _receivedCommentIds = new SynchronizedCollection<string>();
        private bool IsDuplicate(string id)
        {
            if (_receivedCommentIds.Contains(id))
            {
                return true;
            }
            else
            {
                _receivedCommentIds.Add(id);
                return false;
            }
        }
        private void OnMessageReceived(IAction action, bool isInitialComment)
        {
            try
            {
                switch (action)
                {
                    case TextMessage text:
                        {
                            if (IsDuplicate(text.Id))
                            {
                                return;
                            }
                            RaiseMessageReceived(CreateMessageContext2(text, isInitialComment));
                        }
                        break;
                    case SuperChat superChat:
                        {
                            if (IsDuplicate(superChat.Id))
                            {
                                return;
                            }
                            RaiseMessageReceived(CreateMessageContext2(superChat, isInitialComment));
                        }
                        break;
                    case ParseError parseError:
                        {
                            _logger.LogException(new Exception(), "ParseError", parseError.Raw);
                        }
                        break;
                    default:
                        {

                        }
                        break;
                }
            }
            catch(Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private YouTubeLiveMessageContext CreateMessageContext2(TextMessage text, bool isInitialComment)
        {
            IYouTubeLiveMessage message;
            IEnumerable<SitePlugin.IMessagePart> commentItems;
            IEnumerable<SitePlugin.IMessagePart> nameItems;

            var a = new YouTubeLiveComment(text);
            message = a;
            nameItems = a.NameItems;
            commentItems = a.CommentItems;

            var metadata = CreateMetadata(message, isInitialComment);
            var methods = new YouTubeLiveMessageMethods();
            if (_siteOptions.IsAutoSetNickname)
            {
                var user = metadata.User;
                var messageText = Common.MessagePartsTools.ToText(commentItems);
                var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            metadata.User.Name = nameItems;
            return new YouTubeLiveMessageContext(message, metadata, methods);
        }
        private YouTubeLiveMessageContext CreateMessageContext2(SuperChat text, bool isInitialComment)
        {
            IYouTubeLiveMessage message;
            IEnumerable<SitePlugin.IMessagePart> commentItems;
            IEnumerable<SitePlugin.IMessagePart> nameItems;

            var a = new YouTubeLiveSuperchat(text);
            message = a;
            nameItems = a.NameItems;
            commentItems = a.CommentItems;

            var metadata = CreateMetadata(message, isInitialComment);
            var methods = new YouTubeLiveMessageMethods();
            if (_siteOptions.IsAutoSetNickname)
            {
                var user = metadata.User;
                var messageText = Common.MessagePartsTools.ToText(commentItems);
                var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            metadata.User.Name = nameItems;
            return new YouTubeLiveMessageContext(message, metadata, methods);
        }
        //private void OnMessageReceived(IInternalMessage e, bool isInitialComment)
        //{
        //    switch (e)
        //    {
        //        case InternalSuperChat superChat:
        //            {
        //                if (IsDuplicate(superChat.Id))
        //                {
        //                    return;
        //                }
        //                var context = CreateMessageContext2(superChat, isInitialComment);
        //                RaiseMessageReceived(context);
        //            }
        //            break;
        //        case InternalComment comment:
        //            {
        //                if (IsDuplicate(comment.Id))
        //                {
        //                    return;
        //                }
        //                var context = CreateMessageContext2(comment, isInitialComment);
        //                RaiseMessageReceived(context);
        //            }
        //            break;
        //        case InternalMembership membership:
        //            {
        //                if (IsDuplicate(membership.Id))
        //                {
        //                    return;
        //                }
        //                var context = CreateMessageContext2(membership, isInitialComment);
        //                RaiseMessageReceived(context);
        //            }
        //            break;
        //    }
        //}
        //private void ChatProvider_MessageReceived(object sender, IInternalMessage e)
        //{
        //    OnMessageReceived(e, false);
        //}
        private void ChatProvider_MessageReceived(object sender, IAction e)
        {
            OnMessageReceived(e, false);
        }
        //private YouTubeLiveMessageContext CreateMessageContext2(InternalMembership comment, bool isInitialComment)
        //{
        //    IYouTubeLiveMessage message;
        //    IEnumerable<IMessagePart> commentItems;
        //    IEnumerable<IMessagePart> nameItems;

        //    var a = new YouTubeLiveMembership(comment);
        //    message = a;
        //    nameItems = a.NameItems;
        //    commentItems = a.CommentItems;

        //    var metadata = CreateMetadata(message, isInitialComment);
        //    var methods = new YouTubeLiveMessageMethods();
        //    if (_siteOptions.IsAutoSetNickname)
        //    {
        //        var user = metadata.User;
        //        var messageText = Common.MessagePartsTools.ToText(commentItems);
        //        var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
        //        if (!string.IsNullOrEmpty(nick))
        //        {
        //            user.Nickname = nick;
        //        }
        //    }
        //    metadata.User.Name = nameItems;
        //    return new YouTubeLiveMessageContext(message, metadata, methods);
        //}
        //private YouTubeLiveMessageContext CreateMessageContext2(InternalSuperChat superChat, bool isInitialComment)
        //{
        //    IYouTubeLiveMessage message;
        //    IEnumerable<IMessagePart> commentItems;
        //    IEnumerable<IMessagePart> nameItems;

        //    var a = new YouTubeLiveSuperchat(superChat);
        //    message = a;
        //    nameItems = a.NameItems;
        //    commentItems = a.CommentItems;

        //    var metadata = CreateMetadata(message, isInitialComment);
        //    var methods = new YouTubeLiveMessageMethods();
        //    if (_siteOptions.IsAutoSetNickname)
        //    {
        //        var user = metadata.User;
        //        var messageText = Common.MessagePartsTools.ToText(commentItems);
        //        var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
        //        if (!string.IsNullOrEmpty(nick))
        //        {
        //            user.Nickname = nick;
        //        }
        //    }
        //    metadata.User.Name = nameItems;
        //    return new YouTubeLiveMessageContext(message, metadata, methods);
        //}
        //private YouTubeLiveMessageContext CreateMessageContext2(InternalComment comment, bool isInitialComment)
        //{
        //    IYouTubeLiveMessage message;
        //    IEnumerable<IMessagePart> commentItems;
        //    IEnumerable<IMessagePart> nameItems;

        //    var a = new YouTubeLiveComment(comment);
        //    message = a;
        //    nameItems = a.NameItems;
        //    commentItems = a.CommentItems;

        //    var metadata = CreateMetadata(message, isInitialComment);
        //    var methods = new YouTubeLiveMessageMethods();
        //    if (_siteOptions.IsAutoSetNickname)
        //    {
        //        var user = metadata.User;
        //        var messageText = Common.MessagePartsTools.ToText(commentItems);
        //        var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
        //        if (!string.IsNullOrEmpty(nick))
        //        {
        //            user.Nickname = nick;
        //        }
        //    }
        //    metadata.User.Name = nameItems;
        //    return new YouTubeLiveMessageContext(message, metadata, methods);
        //}
        //private YouTubeLiveMessageContext CreateMessageContext(CommentData commentData, bool isInitialComment)
        //{
        //    IYouTubeLiveMessage message;
        //    IEnumerable<IMessagePart> commentItems;
        //    IEnumerable<IMessagePart> nameItems;

        //    if (commentData.IsPaidMessage)
        //    {
        //        var superchat = new YouTubeLiveSuperchat(commentData);
        //        message = superchat;
        //        nameItems = superchat.NameItems;
        //        commentItems = superchat.CommentItems;
        //    }
        //    else
        //    {
        //        var comment = new YouTubeLiveComment(commentData);
        //        message = comment;
        //        nameItems = comment.NameItems;
        //        commentItems = comment.CommentItems;
        //    }
        //    var metadata = CreateMetadata(message, isInitialComment);
        //    var methods = new YouTubeLiveMessageMethods();
        //    if (_siteOptions.IsAutoSetNickname)
        //    {
        //        var user = metadata.User;
        //        var messageText = Common.MessagePartsTools.ToText(commentItems);
        //        var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
        //        if (!string.IsNullOrEmpty(nick))
        //        {
        //            user.Nickname = nick;
        //        }
        //    }
        //    metadata.User.Name = nameItems;
        //    return new YouTubeLiveMessageContext(message, metadata, methods);
        //}
        private YouTubeLiveMessageMetadata CreateMetadata(IYouTubeLiveMessage message, bool isInitialComment)
        {
            string userId = null;
            if (message is IYouTubeLiveComment comment)
            {
                userId = comment.UserId;
            }
            else if (message is IYouTubeLiveSuperchat superchat)
            {
                userId = superchat.UserId;
            }
            else if (message is IYouTubeLiveMembership membership)
            {
                userId = membership.UserId;
            }
            bool isFirstComment;
            IUser user;
            if (userId != null)
            {
                if (_userCommentCountDict.ContainsKey(userId))
                {
                    _userCommentCountDict[userId]++;
                    isFirstComment = false;
                }
                else
                {
                    _userCommentCountDict.Add(userId, 1);
                    isFirstComment = true;
                }
                user = GetUser(userId);
            }
            else
            {
                isFirstComment = false;
                user = null;
            }
            var metadata = new YouTubeLiveMessageMetadata(message, _options, _siteOptions, user, this, isFirstComment)
            {
                IsInitialComment = isInitialComment,
                SiteContextGuid = SiteContextGuid,
            };
            return metadata;
        }
        private readonly Dictionary<string, int> _userCommentCountDict = new Dictionary<string, int>();
        public override async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            BeforeConnect();
            try
            {
                var p = Tools.ParseInput(input);
                switch (p)
                {
                    case InvalidInput _:
                        {
                            SendSystemInfo("入力されたURLは未対応の形式です", InfoType.Error);
                            _logger.LogException(new Test2.ParseException(input));
                            return;
                        }
                }
                await ConnectInternalAsync(p, browserProfile);
            }
            finally
            {
                AfterDisconnected();
            }
        }
        /// <summary>
        /// 切断ボタンが押されたか
        /// </summary>
        bool _isDisconnectionButtonPushed;
        public override void Disconnect()
        {
            _chatProvider?.Disconnect();
            _isDisconnectionButtonPushed = true;
            _reason = ReasonForDisconnection.User;
        }
        protected override void BeforeConnect()
        {
            _userCommentCountDict.Clear();
            _receivedCommentIds.Clear();
            _isDisconnectionButtonPushed = false;
            base.BeforeConnect();
        }
        protected override void AfterDisconnected()
        {
            _elapsedTimer.Enabled = false;
            base.AfterDisconnected();
            SendSystemInfo("切断しました", InfoType.Notice);
        }
        public override async Task<SitePlugin.ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var currentUserInfo = new CurrentUserInfo();
            var cc = CreateCookieContainer(browserProfile);
            var url = "https://www.youtube.com/embed";
            var html = await _server.GetAsync(url, cc);
            //"user_display_name":"Ryu"
            var match = Regex.Match(html, "\"user_display_name\":\"([^\"]+)\"");
            if (match.Success)
            {
                var name = match.Groups[1].Value;
                currentUserInfo.Username = name;
                currentUserInfo.IsLoggedIn = true;
            }
            else
            {
                currentUserInfo.IsLoggedIn = false;
            }
            return currentUserInfo;
        }

        public override SitePlugin.IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SitePlugin.SiteType.YouTubeLive, userId);
        }
        public override void SetMessage(string raw)
        {
            throw new NotImplementedException();
        }
        protected virtual CookieContainer CreateCookieContainer(IBrowserProfile browserProfile)
        {
            var cc = new CookieContainer();//まずCookieContainerのインスタンスを作っておく。仮にCookieの取得で失敗しても/live_chatで"YSC"と"VISITOR_INFO1_LIVE"が取得できる。これらは/service_ajaxでメタデータを取得する際に必須。
            try
            {
                var cookies = browserProfile.GetCookieCollection("youtube.com");
                foreach (var cookie in cookies)
                {
                    cc.Add(cookie);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            return cc;
        }
        public override async Task PostCommentAsync(string text)
        {
            var b = await ((IYouTubeCommentProvider)this).PostCommentAsync(text);
        }
        private DataCreator _postCommentCoodinator;
        async Task<bool> IYouTubeCommentProvider.PostCommentAsync(string comment)
        {
            if (!_isLoggedIn) return false;
            var k = _postCommentCoodinator.Create(comment);
            var innerTubeApiKey = _postCommentCoodinator.InnerTubeApiKey;
            var url = "https://www.youtube.com/youtubei/v1/live_chat/send_message?key=" + innerTubeApiKey;
            var headers = new Dictionary<string, string>
            {
                {"Authorization",$"SAPISIDHASH {k.Hash}" },
                {"Origin", "https://www.youtube.com" }
            };
            var res = await _server.PostJsonNoThrowAsync(url, headers, k.Payload, _cc);
            var ret = await res.Content.ReadAsStringAsync();
            return true;
        }

        public CommentProviderNext(ICommentOptions options, IYouTubeLiveServer server, YouTubeLiveSiteOptions siteOptions, ILogger logger, IUserStoreManager userStoreManager)
            : base(logger, options)
        {
            _options = options;
            _siteOptions = siteOptions;
            _logger = logger;
            _userStoreManager = userStoreManager;
            _server = server;

            CanConnect = true;
            CanDisconnect = false;
        }

        public event EventHandler LoggedInStateChanged;

        CookieContainer _cc;
        private readonly ICommentOptions _options;
        private readonly YouTubeLiveSiteOptions _siteOptions;
        private readonly ILogger _logger;
        private readonly IUserStoreManager _userStoreManager;
        private readonly IYouTubeLiveServer _server;
        void SetLoggedInState(bool isLoggedIn)
        {
            _isLoggedIn = isLoggedIn;
            LoggedInStateChanged?.Invoke(this, EventArgs.Empty);
        }
        bool _isLoggedIn;
        bool IYouTubeCommentProvider.IsLoggedIn => _isLoggedIn;
    }
    class DataCreator
    {
        private readonly string _ytInitialData;
        private readonly CookieContainer _cc;
        public string GetClientIdPrefix()
        {
            return _ytInitialDataT.GetClientIdPrefix();
        }
        private int _commentCounter = 0;
        public void AddCommentCounter()
        {
            _commentCounter++;
        }
        public void ResetCommentCounter()
        {
            _commentCounter = 0;
        }
        private string GetClientVersion()
        {
            return "2.20201202.06.01";
        }
        public PostCommentContext2 Create(string message)
        {
            var context = "{\"context\":{\"client\":{\"clientName\":\"WEB\",\"clientVersion\":\"" + GetClientVersion() + "\"}}}";
            dynamic d = JsonConvert.DeserializeObject(context);
            d.context.user = JsonConvert.DeserializeObject("{\"onBehalfOfUser\":\"" + _delegatedSessionId + "\"}");
            d.@params = GetParams();
            d.clientMessageId = GetClientIdPrefix() + _commentCounter;
            d.richMessage = JsonConvert.DeserializeObject("{\"textSegments\":[{\"text\":\"" + message + "\"}]}");
            var payload = (string)JsonConvert.SerializeObject(d, Formatting.None);
            var hash = SapiSidHashGenerator.CreateHash(_cc, DateTime.Now);
            //var url = GetUrl();
            return new PostCommentContext2(payload, hash);
        }
        public bool IsLoggedIn()
        {
            return _ytInitialDataT.IsLoggedIn;
        }
        public string GetParams()
        {
            dynamic d = JsonConvert.DeserializeObject(_ytInitialData);
            if (d == null)
            {
                throw new SpecChangedException(_ytInitialData);
            }
            string @params;
            try
            {
                @params = (string)d.contents.liveChatRenderer.actionPanel.liveChatMessageInputRenderer.sendButton.buttonRenderer.serviceEndpoint.sendLiveChatMessageEndpoint.@params;
            }
            catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException ex)
            {
                throw new SpecChangedException(_ytInitialData, ex);
            }
            return @params;
        }
        public DataCreator(YtInitialDataOld ytInitialData, string innerTubeApiLey, string delegatedSessionId, CookieContainer cc)
        {
            InnerTubeApiKey = innerTubeApiLey;
            _delegatedSessionId = delegatedSessionId;
            _cc = cc;
            _ytInitialData = ytInitialData.Raw;
            _ytInitialDataT = ytInitialData;
        }
        private readonly YtInitialDataOld _ytInitialDataT;

        public string InnerTubeApiKey { get; }
        private readonly string _delegatedSessionId;
    }
    class PostCommentContext2
    {
        public PostCommentContext2(string payload, string hash)
        {
            Payload = payload;
            Hash = hash;
        }
        public string Payload { get; }
        public string Hash { get; }
    }
}
