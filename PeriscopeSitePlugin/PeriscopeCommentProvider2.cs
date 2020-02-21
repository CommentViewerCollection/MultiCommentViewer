using Codeplex.Data;
using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using SitePluginCommon;
using SitePluginCommon.AutoReconnection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin
{
    enum UrlType
    {
        Unknown,
        LivePage,
        Channel,
    }
    interface IUrl
    {
        UrlType Type { get; }
    }
    class ChannelUrl : IUrl
    {
        public UrlType Type { get; } = UrlType.Channel;
        public string Url { get; }
        public string ChannelId { get; }

        public ChannelUrl(string channelUrl)
        {
            Url = channelUrl;
            var (channelid, _) = Tools.ExtractChannelNameAndLiveId(channelUrl);
            ChannelId = channelid;
        }
    }
    class LivePageUrl : IUrl
    {
        public UrlType Type { get; } = UrlType.LivePage;
        public string Url { get; }
        public string LiveId { get; }
        public string ChannelId { get; }

        public LivePageUrl(string livePageUrl)
        {
            Url = livePageUrl;
            var (channelid, liveid) = Tools.ExtractChannelNameAndLiveId(livePageUrl);
            ChannelId = channelid;
            LiveId = liveid;
        }
    }
    class UnknownUrl : IUrl
    {
        public UrlType Type { get; } = UrlType.Unknown;
        public string Url { get; }

        public UnknownUrl(string url)
        {
            Url = url;
        }
    }
    class MessageProvider : IProvider
    {
        private IWebsocket _websocket;
        private readonly ILogger _logger;

        public IProvider Master { get; }
        public bool IsFinished { get; }
        public Task Work { get; internal set; }
        public ProviderFinishReason FinishReason { get; }
        public string AccessToken { get; internal set; }
        public string RoomId { get; internal set; }
        public string WebsocketUrl { get; internal set; }

        public event EventHandler<IInternalMessage> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;

        public void Start()
        {
            _websocket = new WebSocket(WebsocketUrl);
            _websocket.Opened += Websocket_Opened;//weak referenceにしたい
            _websocket.Received += Websocket_Received;//weak referenceにしたい

            Work = _websocket.ReceiveAsync();
        }

        public void Stop()
        {
            _websocket?.Disconnect();
        }
        public MessageProvider(ILogger logger)
        {
            _logger = logger;
        }
        private void Websocket_Received(object sender, string e)
        {
            var raw = e;
            Debug.WriteLine(raw);
            try
            {
                var websocketMessage = MessageParser.ParseWebsocketMessage(raw);
                var internalMessage = MessageParser.Parse(websocketMessage);
                if (internalMessage != null)
                {
                    MessageReceived?.Invoke(this, internalMessage);
                }
            }
            catch (ParseException ex)
            {
                _logger.LogException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }

        private async void Websocket_Opened(object sender, EventArgs e)
        {
            //{"payload":"{\"access_token\":\"\"}","kind":3}
            //{"payload":"{\"body\":\"{\\\"room\\\":\\\"1MnxnvRQOAoxO\\\"}\",\"kind\":1}","kind":2}
            await _websocket.SendAsync(CreateInitialMessage1(AccessToken));
            await _websocket.SendAsync(CreateInitialMessage2(RoomId));
        }
        protected virtual string CreateInitialMessage1(string accessToken)
        {
            return "{\"payload\":\"{\\\"access_token\\\":\\\"" + accessToken + "\\\"}\",\"kind\":3}";
        }
        protected virtual string CreateInitialMessage2(string roomId)
        {
            return "{\"payload\":\"{\\\"body\\\":\\\"{\\\\\\\"room\\\\\\\":\\\\\\\"" + roomId + "\\\\\\\"}\\\",\\\"kind\\\":1}\",\"kind\":2}";
        }
    }
    class TestAutoReconnector
    {
        private readonly ConnectionManager _connectionManager;
        private readonly IDummy2 _dummy;
        private readonly MessageUntara _messageUntara;
        private readonly ILogger _logger;
        private readonly IUrl _url;
        private readonly IDataServer _server;

        private bool IsBroadcastRunning(BroadcastInfo broadcastInfo)
        {
            return broadcastInfo.State == "RUNNING";
        }
        CancellationTokenSource _generateGroupCts;
        public async Task AutoReconnectAsync()
        {
            _isDisconnectedByUser = false;
            _generateGroupCts = new CancellationTokenSource();
            while (true)
            {
                if (_url is UnknownUrl unknown)
                {
                    _messageUntara.Set($"入力されたURLは無効です。({unknown.Url})", InfoType.Error);
                    break;
                }
                if (_url is ChannelUrl channelUrl)
                {
                    //チャンネル
                    //配信中か確認
                    //配信中であればLiveIdを取得
                    //配信してなかったら始まるまで待機
                    try
                    {
                        var group = await _dummy.GenerateGroupAsync(_generateGroupCts.Token);
                        var reason = await _connectionManager.ConnectAsync(group);
                        System.Diagnostics.Debug.WriteLine($"接続が切れました。原因:{reason}");
                    }
                    catch (TaskCanceledException) { }
                }
                else if (_url is LivePageUrl livePageUrl)
                {
                    var (_, broadcastInfo) = await Api.GetAccessVideoPublicAsync(_server, livePageUrl.LiveId);
                    if (!IsBroadcastRunning(broadcastInfo))
                    {
                        _messageUntara.Set($"この放送は終了しています。({livePageUrl.Url})", InfoType.Error);
                        break;
                    }
                    try
                    {
                        var group = await _dummy.GenerateGroupAsync(_generateGroupCts.Token);
                        var reason = await _connectionManager.ConnectAsync(group);
                    }
                    catch (TaskCanceledException) { }
                    //～の理由により接続が切断されました。
                    //配信が終了している場合でもcontinueでおｋ。
                }
                if (_isDisconnectedByUser)
                {
                    break;
                }
            }
            _generateGroupCts = null;
        }
        bool _isDisconnectedByUser;
        public void Disconnect()
        {
            _isDisconnectedByUser = true;
            _connectionManager.Disconnect();
            _generateGroupCts?.Cancel();
        }
        public TestAutoReconnector(ConnectionManager connectionManager, IDummy2 dummy, MessageUntara messageUntara, ILogger logger, IUrl url, IDataServer server)
        {
            _connectionManager = connectionManager;
            _dummy = dummy;
            _messageUntara = messageUntara;
            _logger = logger;
            _url = url;
            _server = server;
        }
    }    /// <summary>
         /// 名称未設定
         /// </summary>
    public interface IDummy2
    {
        Task<bool> CanConnectAsync();
        Task<IEnumerable<IProvider>> GenerateGroupAsync(CancellationToken ct);
    }
    class DummyImpl : IDummy2
    {
        private readonly IDataServer _server;
        private readonly IUrl _url;
        private readonly IBrowserProfile _browserProfile;
        private readonly ILogger _logger;
        private readonly IPeriscopeSiteOptions _siteOptions;
        private readonly MessageProvider _p1;
        private readonly MessageUntara _messageSetter;

        //private readonly MetadataProvider2 _p2;

        public Task<bool> CanConnectAsync()
        {
            throw new NotImplementedException();
        }
        private bool IsBroadcastRunning(BroadcastInfo broadcastInfo)
        {
            return broadcastInfo.State == "RUNNING";
        }
        private async Task<string> WaitForBroadcastRunning(ChannelUrl channelUrl, CancellationToken ct)
        {
            string liveId;
            while (true)
            {
                var html = await _server.GetAsync(channelUrl.Url);
                var json = Tools.ExtractChannelPageJson(html);
                var d = DynamicJson.Parse(json);
                var broadcasts = ((object[])d.BroadcastCache.broadcasts);
                if (broadcasts.Length > 0)
                {
                    var broadcastJson = (string)((dynamic)((KeyValuePair<string, object>)broadcasts[0]).Value).broadcast.ToString();
                    var broadcast = Tools.Deserialize<Low.Broadcast.RootObject>(broadcastJson);
                    if (broadcast.State == "RUNNING")
                    {
                        liveId = broadcast.Id;
                        _messageSetter.Set($"{channelUrl.ChannelId}は配信中です。放送ID:{liveId}", InfoType.Debug);
                        break;
                    }
                }
                //待機する旨通知する。
                _messageSetter.Set("配信していないため1分待機します。", InfoType.Notice);
                await Task.Delay(60 * 1000, ct);//設定で変更できるようにする
            }
            return liveId;
        }
        /// <summary>
        /// 
        /// 配信してなかったら配信開始まで待機する。
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<IProvider>> GenerateGroupAsync(CancellationToken ct)
        {
            string liveId;
            if (_url is ChannelUrl channelUrl)
            {
                var a = await WaitForBroadcastRunning(channelUrl, ct);
                liveId = a;
            }
            else if (_url is LivePageUrl livePageUrl)
            {
                liveId = livePageUrl.LiveId;
            }
            else
            {
                return new List<IProvider>();
            }
            var (avp, _) = await Api.GetAccessVideoPublicAsync(_server, liveId);
            var acp = await Api.GetAccessChatPublicAsync(_server, avp.ChatToken);
            var hostname = Tools.ExtractHostnameFromEndpoint(acp.Endpoint);
            _p1.AccessToken = acp.AccessToken;
            _p1.RoomId = liveId;
            _p1.WebsocketUrl = $"wss://{hostname}/chatapi/v1/chatnow";

            return new List<IProvider>
            {
                _p1,
            };
        }

        public DummyImpl(IDataServer server, IUrl url, IBrowserProfile browserProfile, ILogger logger, IPeriscopeSiteOptions siteOptions, MessageProvider p1, MessageUntara messageSetter)
        {
            _server = server;
            _url = url;
            _browserProfile = browserProfile;
            _logger = logger;
            _siteOptions = siteOptions;
            _p1 = p1;
            _messageSetter = messageSetter;
            //_p2 = p2;
        }
    }
    class PeriscopeCommentProvider2 : CommentProviderBase
    {
        FirstCommentDetector _first = new FirstCommentDetector();
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly ICommentOptions _options;
        private readonly IPeriscopeSiteOptions _siteOptions;
        private readonly IUserStoreManager _userStoreManager;

        public override async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            BeforeConnect();
            try
            {
                await ConnectInternalAsync(input, browserProfile);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "", $"input={input}");
            }
            finally
            {
                AfterDisconnected();
            }
        }
        TestAutoReconnector _autoReconnector;
        private async Task ConnectInternalAsync(string input, IBrowserProfile browserProfile)
        {
            var url = Tools.GetUrl(input);
            if (url is UnknownUrl)
            {
                //不正なURL
                return;
            }
            var cc = GetCookieContainer(browserProfile);

            var messageSetter = new MessageUntara();
            messageSetter.SystemInfoReiceved += MessageSetter_SystemInfoReiceved;
            var p1 = new MessageProvider(_logger);
            p1.MessageReceived += P1_MessageReceived;
            p1.MetadataUpdated += P1_MetadataUpdated;
            //var p2 = new MetadataProvider2(_server, _siteOptions);
            //p2.MetadataUpdated += P2_MetadataUpdated;
            //p2.Master = p1;
            try
            {

                var dummy = new DummyImpl(_server, url, browserProfile, _logger, _siteOptions, p1, messageSetter);//, p2);
                var connectionManager = new ConnectionManager(_logger);
                _autoReconnector = new TestAutoReconnector(connectionManager, dummy, messageSetter, _logger, url, _server);
                await _autoReconnector.AutoReconnectAsync();
            }
            finally
            {
                messageSetter.SystemInfoReiceved -= MessageSetter_SystemInfoReiceved;
                p1.MessageReceived -= P1_MessageReceived;
                p1.MetadataUpdated -= P1_MetadataUpdated;
                //p2.MetadataUpdated -= P2_MetadataUpdated;
            }
        }

        private void MessageSetter_SystemInfoReiceved(object sender, SitePluginCommon.AutoReconnector.SystemInfoEventArgs e)
        {
            SendSystemInfo(e.Message, e.Type);
        }

        private void P2_MetadataUpdated(object sender, ILiveInfo e)
        {

        }

        private void P1_MetadataUpdated(object sender, IMetadata e)
        {

        }

        private void P1_MessageReceived(object sender, IInternalMessage e)
        {
            if (e is Kind1Type1 kind1Type1)
            {
                var message = new PeriscopeComment(kind1Type1);
                var userId = message.UserId;
                var isFirstComment = _first.IsFirstComment(userId);
                var user = GetUser(userId);
                user.Name = MessagePartFactory.CreateMessageItems(message.Text);
                var metadata = CreateMessageMetadata(message, user, isFirstComment);
                var methods = new MessageMethods();
                RaiseMessageReceived(new MessageContext(message, metadata, methods));
            }
            else if (e is Kind2Kind1 kind2kind1)
            {
                if (!_siteOptions.IsShowJoinMessage)
                {
                    //取得する必要がないため無視する
                    return;
                }
                var message = new PeriscopeJoin(kind2kind1);
                var userId = message.UserId;
                var isFirstComment = false;
                var user = GetUser(userId);
                user.Name = MessagePartFactory.CreateMessageItems(message.DisplayName);
                var metadata = CreateMessageMetadata(message, user, isFirstComment);
                var methods = new MessageMethods();
                RaiseMessageReceived(new MessageContext(message, metadata, methods));
            }
            else if (e is Kind2Kind2 kind2Kind2)
            {
                if (!_siteOptions.IsShowLeaveMessage)
                {
                    //取得する必要がないため無視する
                    return;
                }
                var message = new PeriscopeLeave(kind2Kind2);
                var userId = message.UserId;
                var isFirstComment = false;
                var user = GetUser(userId);
                user.Name = MessagePartFactory.CreateMessageItems(message.DisplayName);
                var metadata = CreateMessageMetadata(message, user, isFirstComment);
                var methods = new MessageMethods();
                RaiseMessageReceived(new MessageContext(message, metadata, methods));
            }
        }
        private MessageMetadata CreateMessageMetadata(IPeriscopeMessage message, IUser user, bool isFirstComment)
        {
            return new MessageMetadata(message, _options, _siteOptions, user, this, isFirstComment)
            {
                SiteContextGuid = SiteContextGuid,
            };
        }
        public override void Disconnect()
        {
            _autoReconnector?.Disconnect();
        }


        protected virtual CookieContainer GetCookieContainer(IBrowserProfile browserProfile)
        {
            var cc = new CookieContainer();

            try
            {
                var cookies = browserProfile.GetCookieCollection("pscp.tv");
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
        public override async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var userInfo = new CurrentUserInfo
            {

            };
            return await Task.FromResult(userInfo);
        }

        public override IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.Periscope, userId);
        }

        public override Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }

        public override void SetMessage(string raw)
        {
            throw new NotImplementedException();
        }

        public PeriscopeCommentProvider2(IDataServer server, ILogger logger, ICommentOptions options, IPeriscopeSiteOptions siteOptions, IUserStoreManager userStoreManager)
            : base(logger, options)
        {
            _server = server;
            _logger = logger;
            _options = options;
            _siteOptions = siteOptions;
            _userStoreManager = userStoreManager;
        }
    }
}
