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
    class ChatProvider2
    {
        public event EventHandler<IInternalMessage> MessageReceived;
        public event EventHandler<bool> LoggedInStateChanged;
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
        public async Task ReceiveAsync(string vid, CookieContainer cc)
        {
            if (_cts != null)
            {
                Debug.WriteLine("receiving");
                return;
            }
            _cts = new CancellationTokenSource();
            var liveChatHtml = await GetLiveChat(vid, cc);
            var ytCfgStr = Tools.ExtractYtCfg(liveChatHtml);
            var ytCfg = new YtCfg(ytCfgStr);
            var ytInitialData = Tools.ExtractYtInitialData(liveChatHtml);
            LoggedInStateChanged?.Invoke(this, ytInitialData.IsLoggedIn);
            var initialActions = ytInitialData.GetActions();
            foreach (var action in initialActions)
            {
                ProcessAction(action);
            }
            var dataToPost = new DataToPost(ytCfg);
            string initialContinuation;
            if (_siteOptions.IsAllChat)
            {
                initialContinuation = ytInitialData.ChatContinuation.AllChatContinuation;
            }
            else
            {
                initialContinuation = ytInitialData.ChatContinuation.JouiChatContinuation;
            }
            dataToPost.SetContinuation(initialContinuation);

            while (!_cts.IsCancellationRequested)
            {
                var hash = new HashGenerator(cc).CreateHash();
                GetLiveChat s;
                try
                {
                    s = await Tools.GetGetLiveChat(dataToPost, ytCfg.InnerTubeApiKey, hash, cc);
                }
                catch (HttpRequestException ex)
                {
                    await Task.Delay(1000);
                    throw new ReloadException(ex);
                }
                catch (Exception ex)
                {
                    await Task.Delay(1000);
                    throw new ReloadException(ex);
                }
                var actions = s.GetActions();
                IContinuation continuation;
                try
                {
                    continuation = s.GetContinuation();
                }
                catch (ChatUnavailableException)
                {
                    //SendSystemInfo()
                    throw new NotImplementedException();
                }
                catch (ContinuationNotExistsException)
                {
                    //SendSystemInfo()
                    throw new NotImplementedException();
                }
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
        public ChatProvider2(IYouTubeLiveSiteOptions siteOptions)
        {
            _siteOptions = siteOptions;
        }
        private CancellationTokenSource _cts;
        private readonly IYouTubeLiveSiteOptions _siteOptions;
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
                default:
                    throw new NotImplementedException();
            }
            var cc = CreateCookieContainer(browserProfile);
            await InitElapsedTimer(vid);
            _chatProvider = new ChatProvider2(_siteOptions);
            _chatProvider.MessageReceived += ChatProvider_MessageReceived;
            _chatProvider.LoggedInStateChanged += _chatProvider_LoggedInStateChanged;

            var metaProvider = new MetaDataYoutubeiProvider(_server, _logger);
            metaProvider.InfoReceived += MetaProvider_InfoReceived;
            metaProvider.MetadataReceived += MetaProvider_MetadataReceived;

        reload:
            var chatTask = _chatProvider.ReceiveAsync(vid, cc);
            var html = await _server.GetAsync($"https://www.youtube.com/live_chat?is_popout=1&v={vid}");
            var ytCfg = Tools.ExtractYtCfg(html);
            var metaTask = metaProvider.ReceiveAsync(ytCfg, vid, cc);

            var t = await Task.WhenAny(chatTask, metaTask);
            if (t == chatTask)
            {
                try
                {
                    metaProvider.Disconnect();
                    await metaTask;
                }
                catch (Exception ex)
                {

                }
                try
                {
                    await chatTask;
                }
                catch (ReloadException ex)
                {
                    goto reload;
                }
                catch (Exception ex)
                {

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

                }
            }


            _chatProvider.MessageReceived -= ChatProvider_MessageReceived;
            metaProvider.InfoReceived -= MetaProvider_InfoReceived;
            metaProvider.MetadataReceived -= MetaProvider_MetadataReceived;
        }

        private void MetaProvider_MetadataReceived(object sender, IMetadata e)
        {
            RaiseMetadataUpdated(e);
        }

        private void MetaProvider_InfoReceived(object sender, InfoData e)
        {
            throw new NotImplementedException();
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
        private void ChatProvider_MessageReceived(object sender, IInternalMessage e)
        {
            switch (e)
            {
                case InternalSuperChat superChat:
                    {
                        if (IsDuplicate(superChat.Id))
                        {
                            return;
                        }
                        var context = CreateMessageContext2(superChat, false);
                        RaiseMessageReceived(context);
                    }
                    break;
                case InternalComment comment:
                    {
                        if (IsDuplicate(comment.Id))
                        {
                            return;
                        }
                        var context = CreateMessageContext2(comment, false);
                        RaiseMessageReceived(context);
                    }
                    break;
                case InternalMembership membership:
                    {
                        if (IsDuplicate(membership.Id))
                        {
                            return;
                        }
                        var context = CreateMessageContext2(membership, false);
                        RaiseMessageReceived(context);
                    }
                    break;
            }
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

        public override void Disconnect()
        {
            _chatProvider?.Disconnect();
        }
        protected override void BeforeConnect()
        {
            _userCommentCountDict.Clear();
            _receivedCommentIds.Clear();
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

        public override Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
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

        Task<bool> IYouTubeCommentProvider.PostCommentAsync(string comment)
        {
            throw new NotImplementedException();
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
}
