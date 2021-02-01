using System;
using System.Threading.Tasks;
using System.Net;
using SitePlugin;
using ryu_s.BrowserCookie;
using Common;
using System.Text.RegularExpressions;
using SitePluginCommon;
using YouTubeLiveSitePlugin.Test2;
using System.Net.Http;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Linq;

namespace YouTubeLiveSitePlugin.Next
{
    interface IInternalMessage { }
    class InternalComment : IInternalMessage
    {
        public string UserId { get; internal set; }
        public long TimestampUsec { get; internal set; }
        public string Id { get; internal set; }
        public string ThumbnailUrl { get; internal set; }
        public int ThumbnailWidth { get; internal set; }
        public int ThumbnailHeight { get; internal set; }
        public List<IMessagePart> MessageItems { get; internal set; }
        public List<IMessagePart> NameItems { get; internal set; }
    }
    class InternalSuperChat : IInternalMessage
    {
        public string UserId { get; internal set; }
        public long TimestampUsec { get; internal set; }
        public string Id { get; internal set; }
        public string ThumbnailUrl { get; internal set; }
        public int ThumbnailWidth { get; internal set; }
        public int ThumbnailHeight { get; internal set; }
        public List<IMessagePart> MessageItems { get; internal set; }
        public List<IMessagePart> NameItems { get; internal set; }
        public string PurchaseAmount { get; internal set; }
    }
    /// <summary>
    /// メンバー登録があった時に流れるメッセージ
    /// </summary>
    class InternalMembership : IInternalMessage
    {
        public string UserId { get; internal set; }
        public long TimestampUsec { get; internal set; }
        public string Id { get; internal set; }
        public List<IMessagePart> MessageItems { get; internal set; }
        public List<IMessagePart> NameItems { get; internal set; }
        public string ThumbnailUrl { get; internal set; }
        public int ThumbnailWidth { get; internal set; }
        public int ThumbnailHeight { get; internal set; }
    }
    class UnknownAction : IInternalMessage
    {
        public UnknownAction(string raw)
        {
            Raw = raw;
        }

        public string Raw { get; }
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
        public event EventHandler<IInternalMessage> MessageReceived;
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
        /// <param name="ytInitialData"></param>
        /// <param name="ytCfg"></param>
        /// <param name="cc"></param>
        /// <param name="loginInfo"></param>
        /// <returns></returns>
        /// <exception cref="ReloadException"></exception>
        /// <exception cref="SpecChangedException"></exception>
        public async Task ReceiveAsync(string vid, YtInitialData ytInitialData, YtCfg ytCfg, CookieContainer cc, ILoginState loginInfo)
        {
            if (_cts != null)
            {
                throw new InvalidOperationException("receiving");
            }
            _cts = new CancellationTokenSource();
            try
            {
                await ReceiveInternalAsync(ytInitialData, ytCfg, cc, loginInfo);
            }
            finally
            {
                _cts = null;
            }
        }
        public async Task ReceiveInternalAsync(YtInitialData ytInitialData, YtCfg ytCfg, CookieContainer cc, ILoginState loginInfo)
        {
            var dataToPost = new DataToPost(ytCfg);
            string initialContinuation;
            if (_siteOptions.IsAllChat)
            {
                initialContinuation = ytInitialData.ChatContinuation().AllChatContinuation;
            }
            else
            {
                initialContinuation = ytInitialData.ChatContinuation().JouiChatContinuation;
            }
            dataToPost.SetContinuation(initialContinuation);

            while (!_cts.IsCancellationRequested)
            {
                //例外はここではcatchしない。
                var s = await Tools.GetGetLiveChat(dataToPost, ytCfg.InnerTubeApiKey, cc, loginInfo, _logger);
                var actions = s.GetActions();
                var continuation = s.GetContinuation();
                if (continuation is ReloadContinuation reload)
                {
                    throw new ReloadException();
                }
                else if (continuation is UnknownContinuation unknown)
                {
                    throw new SpecChangedException(unknown.Raw);
                }
                dataToPost.SetContinuation(continuation.Continuation);
                var count = actions.Count;
                var timeoutMs = Math.Max(continuation.TimeoutMs, 1000);
                var waitTime = count > 0 ? timeoutMs / count : 1000;
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
        }
        private void ProcessAction(IInternalMessage action)
        {
            switch (action)
            {
                case InternalComment comment:
                    MessageReceived?.Invoke(this, comment);
                    break;
                case InternalSuperChat superChat:
                    MessageReceived?.Invoke(this, superChat);
                    break;
                case InternalMembership membership:
                    MessageReceived?.Invoke(this, membership);
                    break;
                default:
                    break;
            }
        }
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
                            Elapsed = Test2.Tools.ToElapsedString(elapsed),
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
        private async Task ConnectInternalAsync(string input, IBrowserProfile browserProfile)
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
                case InvalidInput invalidInput:
                    SendSystemInfo("入力されたURLは未対応の形式です", InfoType.Error);
                    _logger.LogException(new ParseException(input));
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

            var liveChatHtml = await GetLiveChat(vid, _cc);
            var ytCfgStr = Tools.ExtractYtCfg(liveChatHtml);
            var ytCfg = new YtCfg(ytCfgStr);
            var ytInitialData = Tools.ExtractYtInitialData(liveChatHtml);
            if (!ytInitialData.CanChat)
            {
                SendSystemInfo("このライブストリームではチャットは無効です。", InfoType.Notice);
                return;
            }
            var loginInfo = Tools.CreateLoginInfo(ytInitialData.IsLoggedIn);
            //ログイン済みユーザの正常にコメントが取得できるようになったら以下のコードは不要
            //---ここから---
            if (loginInfo is LoggedIn)
            {
                var k = Tools.GetSapiSid(_cc);
                if (k == null)
                {
                    //SIDが無い。ログイン済み判定なのにSIDが無い場合が散見されるが原因不明。強制的に未ログインにする。
                    var cookies = Tools.ExtractCookies(_cc);
                    var cver = ytInitialData.Cver;
                    var keys = string.Join(",", cookies.Select(c => c.Name));
                    _logger.LogException(new Exception(), "", $"cver={cver},keys={keys}");
                    _cc = new CookieContainer();
                    goto reload;
                }
            }
            //---ここまで---
            SetLoggedInState(ytInitialData.IsLoggedIn);
            _postCommentCoodinator = new DataCreator(ytInitialData, ytCfg.InnerTubeApiKey, ytCfg.DelegatedSessionId, _cc);
            var initialActions = ytInitialData.GetActions();
            foreach (var action in initialActions)
            {
                OnMessageReceived(action, true);
            }

            var chatTask = _chatProvider.ReceiveAsync(vid, ytInitialData, ytCfg, _cc, loginInfo);
            var metaTask = metaProvider.ReceiveAsync(ytCfg, vid, _cc);

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
                    catch (ChatUnavailableException)
                    {
                        _isDisconnectedExpected = true;
                        SendSystemInfo("配信が終了したか、チャットが無効です。", InfoType.Notice);
                    }
                    catch (ReloadException)
                    {
                    }
                    catch (SpecChangedException ex)
                    {
                        SendSystemInfo("YouTubeの仕様変更に未対応のためコメント取得を継続できません", InfoType.Error);
                        _logger.LogException(ex);
                        _isDisconnectedExpected = true;
                    }
                    catch (Exception ex)
                    {
                        SendSystemInfo(ex.Message, InfoType.Error);
                        //意図しない切断
                        //ただし、サーバーからReloadメッセージが来た場合と違って、単純にリロードすれば済む問題ではない。
                        _logger.LogException(ex);
                        await Task.Delay(1000);
                    }
                    tasks.Remove(chatTask);

                    if (_isDisconnectedExpected == false)
                    {
                        //何らかの原因で意図しない切断が発生した。
                        SendSystemInfo("エラーが発生したためサーバーとの接続が切断されましたが、自動的に再接続します", InfoType.Notice);
                        goto reload;
                    }
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

        private void MetaProvider_MetadataReceived(object sender, IMetadata e)
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
        private void OnMessageReceived(IInternalMessage e, bool isInitialComment)
        {
            switch (e)
            {
                case InternalSuperChat superChat:
                    {
                        if (IsDuplicate(superChat.Id))
                        {
                            return;
                        }
                        var context = CreateMessageContext2(superChat, isInitialComment);
                        RaiseMessageReceived(context);
                    }
                    break;
                case InternalComment comment:
                    {
                        if (IsDuplicate(comment.Id))
                        {
                            return;
                        }
                        var context = CreateMessageContext2(comment, isInitialComment);
                        RaiseMessageReceived(context);
                    }
                    break;
                case InternalMembership membership:
                    {
                        if (IsDuplicate(membership.Id))
                        {
                            return;
                        }
                        var context = CreateMessageContext2(membership, isInitialComment);
                        RaiseMessageReceived(context);
                    }
                    break;
            }
        }
        private void ChatProvider_MessageReceived(object sender, IInternalMessage e)
        {
            OnMessageReceived(e, false);
        }
        private YouTubeLiveMessageContext CreateMessageContext2(InternalMembership comment, bool isInitialComment)
        {
            IYouTubeLiveMessage message;
            IEnumerable<IMessagePart> commentItems;
            IEnumerable<IMessagePart> nameItems;

            var a = new YouTubeLiveMembership(comment);
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
        private YouTubeLiveMessageContext CreateMessageContext2(InternalSuperChat superChat, bool isInitialComment)
        {
            IYouTubeLiveMessage message;
            IEnumerable<IMessagePart> commentItems;
            IEnumerable<IMessagePart> nameItems;

            var a = new YouTubeLiveSuperchat(superChat);
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
        private YouTubeLiveMessageContext CreateMessageContext2(InternalComment comment, bool isInitialComment)
        {
            IYouTubeLiveMessage message;
            IEnumerable<IMessagePart> commentItems;
            IEnumerable<IMessagePart> nameItems;

            var a = new YouTubeLiveComment(comment);
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
        private YouTubeLiveMessageContext CreateMessageContext(CommentData commentData, bool isInitialComment)
        {
            IYouTubeLiveMessage message;
            IEnumerable<IMessagePart> commentItems;
            IEnumerable<IMessagePart> nameItems;

            if (commentData.IsPaidMessage)
            {
                var superchat = new YouTubeLiveSuperchat(commentData);
                message = superchat;
                nameItems = superchat.NameItems;
                commentItems = superchat.CommentItems;
            }
            else
            {
                var comment = new YouTubeLiveComment(commentData);
                message = comment;
                nameItems = comment.NameItems;
                commentItems = comment.CommentItems;
            }
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
                await ConnectInternalAsync(input, browserProfile);
            }
            finally
            {
                AfterDisconnected();
            }
        }
        bool _isDisconnectedExpected;
        public override void Disconnect()
        {
            _chatProvider?.Disconnect();
            _isDisconnectedExpected = true;
        }
        protected override void BeforeConnect()
        {
            _userCommentCountDict.Clear();
            _receivedCommentIds.Clear();
            _isDisconnectedExpected = false;
            base.BeforeConnect();
        }
        protected override void AfterDisconnected()
        {
            _elapsedTimer.Enabled = false;
            base.AfterDisconnected();
            SendSystemInfo("切断しました", InfoType.Notice);
        }
        public override async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
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

        public override IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.YouTubeLive, userId);
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
        public override Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
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

        public CommentProviderNext(ICommentOptions options, IYouTubeLibeServer server, YouTubeLiveSiteOptions siteOptions, ILogger logger, IUserStoreManager userStoreManager)
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
        private readonly IYouTubeLibeServer _server;
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
            var hash = CreateHash();
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
        public string GetSapiSid()
        {
            var cookies = Tools.ExtractCookies(_cc);
            var c = cookies.Find(cookie => cookie.Name == "SAPISID");
            return c?.Value;
        }
        public virtual long GetCurrentUnixTime()
        {
            return Common.UnixTimeConverter.ToUnixTime(DateTime.Now);
        }
        private string ComputeSHA1(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            byte[] hashValue;
            using (var crypto = new SHA1CryptoServiceProvider())
            {
                hashValue = crypto.ComputeHash(bytes);
            }
            var sb = new StringBuilder();
            foreach (var b in hashValue)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }
        public string CreateHash()
        {
            var unixTime = GetCurrentUnixTime();
            var sapiSid = GetSapiSid();
            var origin = "https://www.youtube.com";
            if (sapiSid == null)
            {
                throw new SpecChangedException("");
            }
            var s = $"{unixTime} {sapiSid} {origin}";
            var hash = ComputeSHA1(s).ToLower();
            return $"{unixTime}_{hash}";
        }
        public DataCreator(YtInitialData ytInitialData, string innerTubeApiLey,string delegatedSessionId, CookieContainer cc)
        {
            InnerTubeApiKey = innerTubeApiLey;
            _delegatedSessionId = delegatedSessionId;
            _cc = cc;
            _ytInitialData = ytInitialData.Raw;
            _ytInitialDataT = ytInitialData;
        }
        private readonly YtInitialData _ytInitialDataT;

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
