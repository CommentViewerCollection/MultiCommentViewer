using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MirrativSitePlugin
{
    class MetadataProvider
    {
        private readonly IDataServer _server;
        private readonly IMirrativSiteOptions _siteOptions;
        private readonly string _liveId;
        public event EventHandler<ILiveInfo> MetadataUpdated;

        public MetadataProvider(IDataServer server, IMirrativSiteOptions siteOptions, string liveId)
        {
            _server = server;
            _siteOptions = siteOptions;
            _liveId = liveId;
        }
        public async Task ReceiveAsync()
        {
            _isDisconnectRequested = false;
            _cts = new CancellationTokenSource();

            while (true)
            {
                if (_isDisconnectRequested)
                {
                    break;
                }
                var liveInfo = await Api.PollLiveAsync(_server, _liveId);
                MetadataUpdated?.Invoke(this, liveInfo);
                try
                {
                    await Task.Delay(_siteOptions.PollingIntervalSec * 60 * 1000, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
        bool _isDisconnectRequested;
        public void Disconnect()
        {
            _isDisconnectRequested = true;
            _cts?.Cancel();
        }
        CancellationTokenSource _cts;
    }
    class MirrativCommentProvider : ICommentProvider
    {
        private bool _canConnect;
        public bool CanConnect
        {
            get { return _canConnect; }
            set
            {
                if (_canConnect == value)
                    return;
                _canConnect = value;
                CanConnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool _canDisconnect;
        public bool CanDisconnect
        {
            get { return _canDisconnect; }
            set
            {
                if (_canDisconnect == value)
                    return;
                _canDisconnect = value;
                CanDisconnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<IMessageContext> MessageReceived;

        private MessageProvider _provider;
        protected virtual CookieContainer GetCookieContainer(IBrowserProfile browserProfile)
        {
            var cc = new CookieContainer();

            try
            {
                var cookies = browserProfile.GetCookieCollection("mirrativ.com");
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
        protected virtual MessageProvider CreateMessageProvider(string broadcastKey)
        {
            return new MessageProvider(new WebSocket("wss://online.mirrativ.com/"), _logger, broadcastKey);
        }
        private IMetadata LiveInfo2Meta(ILiveInfo liveInfo)
        {
            return new Metadata
            {
                Title = liveInfo.Title,
                TotalViewers = liveInfo.TotalViewerNum.ToString(),
                CurrentViewers = liveInfo.OnlineUserNum.ToString(),
            };
        }
        FirstCommentDetector _first = new FirstCommentDetector();
        /// <summary>
        /// ユーザが何をinputに入力したか
        /// </summary>
        enum InputType
        {
            Unknown,
            /// <summary>
            /// ユーザID、もしくはそれを含むURL
            /// </summary>
            UserId,
            /// <summary>
            /// 放送ID、もしくはそれを含むURL
            /// </summary>
            LiveId,
        }
        /// <summary>
        /// 切断した理由
        /// UserIdで接続した場合、ユーザの意図的な切断以外では切断しないようにするための識別子
        /// </summary>
        enum DisconnectReason
        {
            Unknown,
            /// <summary>
            /// ユーザが意図したもの
            /// </summary>
            User,
        }
        DisconnectReason _disconnectReason;
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            CanConnect = false;
            CanDisconnect = true;

            _disconnectReason =  DisconnectReason.Unknown;
            InputType inputType;
            try
            {
                while (true)
                {
                    _first.Reset();
                    string liveId;
                    if (Tools.IsValidUserId(input))
                    {
                        inputType = InputType.UserId;
                        var userId = Tools.ExtractUserId(input);
                        liveId = await GetLiveIdAsync(userId);//TODO:
                        //GetLiveIdAsync()を実行中にユーザがDisconnect()するとliveIdがnullになる
                        if (string.IsNullOrEmpty(liveId))
                        {
                            break;
                        }
                    }
                    else if (Tools.IsValidLiveId(input))
                    {
                        inputType = InputType.LiveId;
                        liveId = Tools.ExtractLiveId(input);
                    }
                    else
                    {
                        inputType = InputType.Unknown;
                        //
                        break;
                    }

                    var liveInfo = await Api.GetLiveInfo(_server, liveId);
                    MetadataUpdated?.Invoke(this, LiveInfo2Meta(liveInfo));
                    Connected?.Invoke(this, new ConnectedEventArgs
                    {
                        IsInputStoringNeeded = false,
                        UrlToRestore = null,
                    });
                    var initialComments = await Api.GetLiveComments(_server, liveId);
                    foreach (var c in initialComments)
                    {
                        var userId = c.UserId;
                        var isFirstComment = _first.IsFirstComment(userId);
                        var user = GetUser(userId);

                        var context = CreateMessageContext(c, true, "");
                        MessageReceived?.Invoke(this, context);
                    }
                    _provider = CreateMessageProvider(liveInfo.Broadcastkey);
                    _provider.MessageReceived += Provider_MessageReceived;
                    _provider.MetadataUpdated += Provider_MetadataUpdated;

                    var commentTask = _provider.ReceiveAsync();
                    var metaProvider = new MetadataProvider(_server, _siteOptions, liveId);
                    metaProvider.MetadataUpdated += MetaProvider_MetadataUpdated;
                    var metaTask = metaProvider.ReceiveAsync();
                    var tasks = new List<Task>();
                    tasks.Add(commentTask);
                    tasks.Add(metaTask);

                    while (tasks.Count > 0)
                    {
                        var t = await Task.WhenAny(tasks);
                        if (t == commentTask)
                        {
                            try
                            {
                                await commentTask;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogException(ex, "",$"input={input}");
                            }
                            tasks.Remove(commentTask);
                            metaProvider.Disconnect();
                            try
                            {
                                await metaTask;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogException(ex, "", $"input={input}");
                            }
                            tasks.Remove(metaTask);
                        }
                        else if (t == metaTask)
                        {
                            try
                            {
                                await metaTask;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogException(ex, "", $"input={input}");
                            }
                            tasks.Remove(metaTask);
                            //MetadataProviderの内部でcatchしないような例外が投げられた。メタデータの取得は諦めたほうが良い。多分。
                        }
                    }
                    //inputTypeがUserIdの場合は
                    if (inputType != InputType.UserId)
                    {
                        break;
                    }
                    if(_disconnectReason == DisconnectReason.User)
                    {
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogException(ex);
            }
            finally
            {
                CanConnect = true;
                CanDisconnect = false;
            }
        }

        private void Provider_MetadataUpdated(object sender, IMetadata e)
        {
            MetadataUpdated?.Invoke(this, e);
        }

        private MirrativMessageContext CreateMessageContext(IMirrativMessage message)
        {
            if (message is IMirrativComment comment)
            {
                var userId = comment.UserId;
                var isFirst = _first.IsFirstComment(userId);
                var user = GetUser(userId);
                //var comment = new MirrativComment(message, raw);
                var metadata = new MirrativMessageMetadata(comment, _options, _siteOptions, user, this, isFirst)
                {
                    IsInitialComment = false,
                    SiteContextGuid = SiteContextGuid,
                };
                var methods = new MirrativMessageMethods();
                if (_siteOptions.NeedAutoSubNickname)
                {
                    var messageText = message.CommentItems.ToText();
                    var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
                    if (!string.IsNullOrEmpty(nick))
                    {
                        user.Nickname = nick;
                    }
                }
                return new MirrativMessageContext(comment, metadata, methods);
            }
            else if (message is IMirrativJoinRoom join)
            {
                var userId = join.UserId;
                var isFirst = false;
                var user = GetUser(userId);
                //var comment = new MirrativComment(message, raw);
                var metadata = new MirrativMessageMetadata(join, _options, _siteOptions, user, this, isFirst)
                {
                    IsInitialComment = false,
                    SiteContextGuid = SiteContextGuid,
                };
                var methods = new MirrativMessageMethods();
                return new MirrativMessageContext(join, metadata, methods);
            }
            else if (message is IMirrativItem item)
            {
                var userId = item.UserId;
                var isFirst = false;
                var user = GetUser(userId);
                var metadata = new MirrativMessageMetadata(item, _options, _siteOptions, user, this, isFirst)
                {
                    IsInitialComment = false,
                    SiteContextGuid = SiteContextGuid,
                };
                var methods = new MirrativMessageMethods();
                return new MirrativMessageContext(item, metadata, methods);
            }
            else if (message is IMirrativConnected connected)
            {
                var metadata = new MirrativMessageMetadata(connected, _options, _siteOptions, null, this, false)
                {
                    IsInitialComment = false,
                    SiteContextGuid = SiteContextGuid,
                };
                var methods = new MirrativMessageMethods();
                return new MirrativMessageContext(connected, metadata, methods);
            }
            else if (message is IMirrativDisconnected disconnected)
            {
                var metadata = new MirrativMessageMetadata(disconnected, _options, _siteOptions, null, this, false)
                {
                    IsInitialComment = false,
                    SiteContextGuid = SiteContextGuid,
                };
                var methods = new MirrativMessageMethods();
                return new MirrativMessageContext(disconnected, metadata, methods);
            }
            else
            {
                return null;
            }
        }
        private void Provider_MessageReceived(object sender, IMirrativMessage e)
        {
            var message = e;
            var messageContext = CreateMessageContext(message);
            if (messageContext != null)
            {
                MessageReceived?.Invoke(this, messageContext);
            }
            if(message is IMirrativDisconnected)
            {
                _provider.Disconnect();
            }
        }
        /// <summary>
        /// 指定されたユーザの配信中の放送IDを取得する
        /// 配信していない場合は配信を始めるまで待機する
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<string> GetLiveIdAsync(string userId)
        {
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                var userProfile = await Api.GetUserProfileAsync(_server, userId);
                if (!string.IsNullOrEmpty(userProfile.OnLiveLiveId))
                {
                    return userProfile.OnLiveLiveId;
                }
                Debug.WriteLine("配信中ではないため、次の配信が始まるまで待機します");
                try
                {
                    await Task.Delay(CheckIfLiveStartedIntervalSec * 1000, _cts.Token);
                }
                catch (TaskCanceledException) { break; }
            }
            return null;
        }
        /// <summary>
        /// 配信が開始されたかチェックする間隔（秒）
        /// </summary>
        int CheckIfLiveStartedIntervalSec { get; set; } = 10;
        CancellationTokenSource _cts;

        private void MetaProvider_MetadataUpdated(object sender, ILiveInfo e)
        {
            var liveInfo = e;
            MetadataUpdated?.Invoke(this, LiveInfo2Meta(liveInfo));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="raw"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        private MirrativMessageContext CreateMessageContext(Message message, bool isInitialComment, string raw)
        {
            var userId = message.UserId;
            var isFirst = _first.IsFirstComment(userId);
            var user = GetUser(userId);
            var comment = new MirrativComment(message, raw);
            var metadata = new MirrativMessageMetadata(comment, _options, _siteOptions, user, this, isFirst)
            {
                IsInitialComment = isInitialComment,
                SiteContextGuid = SiteContextGuid,
            };
            var methods = new MirrativMessageMethods();
            if (_siteOptions.NeedAutoSubNickname)
            {
                var messageText = message.Comment;
                var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            return new MirrativMessageContext(comment, metadata, methods);
        }
        /*
         * メッセージ例
         * t == 1
         * {"speech":"銀杏 どこですか？","d":1,"ac":"銀杏🌸","burl":"","iurl":"","cm":"どこですか？","created_at":1540124936,"u":"4534198","is_moderator":0,"lci":"1049351080","t":1}
         * 
         * t == 2
         * 
         * t == 3
         * {"online_viewer_num":44,"speech":"銀杏🌸が入室しました","d":1,"ac":"銀杏🌸","burl":"","iurl":"","created_at":1540124836,"u":"4534198","is_moderator":0,"lci":0,"t":3}
         * 
         * t == 7
         * {"created_at":1540124709,"collab_enabled":"1","sticker_enabled":"0","collab_has_vacancy":1,"orientation_v2":"6","t":7}
         * 
         * t == 8
         * {"t":8}
         * 
         * t == 9
         * {"u":"4534198","ac":"銀杏🌸","burl":"","iurl":"","owner_name":"🐶 dog🐶","target_live_id":"jfCADsTAHky3y8L85roP9Q","t":9}
         * 
         */
        //private async void Provider_Opened(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        await _provider_old.SendAsync("PING");
        //        await _provider_old.SendAsync("SUB" + '\t' + _broadcastKey);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogException(ex);
        //        SendSystemInfo(ex.Message, InfoType.Error);
        //    }
        //}
        private void SendSystemInfo(string message, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(message) },
                NameItems = null,
                SiteType = SiteType.Mirrativ,
                Type = type,
            }, _options);
            MessageReceived?.Invoke(this, context);
        }
        public void Disconnect()
        {
            _disconnectReason = DisconnectReason.User;
            _cts?.Cancel();
            _provider?.Disconnect();            
        }
        public IUser GetUser(string userId)
        {
            return _userStore.GetUser(userId);
        }
        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        public async Task PostCommentAsync(string text)
        {
            //var s = $"PRIVMSG {_channelName} :{text}";
            await Task.FromResult<object>(null);
        }

        public async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var cc = GetCookieContainer(browserProfile);
            var currentUser = await Api.GetCurrentUserAsync(_server, cc);

            return new CurrentUserInfo
            {
                IsLoggedIn = currentUser.IsLoggedIn,
                UserId = currentUser.UserId,
                Username = currentUser.Name,
            };
        }
        public Guid SiteContextGuid { get; set; }
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly ICommentOptions _options;
        private readonly IMirrativSiteOptions _siteOptions;
        private readonly IUserStore _userStore;

        public MirrativCommentProvider(IDataServer server, ILogger logger, ICommentOptions options, IMirrativSiteOptions siteOptions, IUserStore userStore)
        {
            _server = server;
            _logger = logger;
            _options = options;
            _siteOptions = siteOptions;
            _userStore = userStore;
            CanConnect = true;
            CanDisconnect = false;
        }
    }
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
