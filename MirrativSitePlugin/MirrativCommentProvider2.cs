using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Net;
using System.Threading.Tasks;
using SitePluginCommon.AutoReconnection;
namespace MirrativSitePlugin
{
    class MirrativCommentProvider2 : CommentProviderBase
    {
        FirstCommentDetector _first = new FirstCommentDetector();
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly ICommentOptions _options;
        private readonly IMirrativSiteOptions _siteOptions;
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
        NewAutoReconnector _autoReconnector;
        /// <summary>
        /// 放送IDを取得する
        /// </summary>
        /// <param name="input"></param>
        /// <returns>ユーザページのURLを入力した場合に配信中で無ければnull</returns>
        private async Task<string> GetCurrentLiveIdAsync(string input)
        {
            string liveId = null;
            if (Tools.IsValidUserId(input))
            {
                var userId = Tools.ExtractUserId(input);
                var userProfile = await Api.GetUserProfileAsync(_server, userId);
                if (!string.IsNullOrEmpty(userProfile.OnLiveLiveId))
                {
                    liveId = userProfile.OnLiveLiveId;
                }
            }
            else if (Tools.IsValidLiveId(input))
            {
                liveId = Tools.ExtractLiveId(input);
            }
            return liveId;
        }
        private async Task ConnectInternalAsync(string input, IBrowserProfile browserProfile)
        {
            var p1 = new MessageProvider2(new WebSocket("wss://online.mirrativ.com/"), _logger);
            p1.MessageReceived += P1_MessageReceived;
            p1.MetadataUpdated += P1_MetadataUpdated;
            var p2 = new MetadataProvider2(_server, _siteOptions);
            p2.MetadataUpdated += P2_MetadataUpdated;
            p2.Master = p1;
            try
            {
                var dummy = new DummyImpl(_server, input, _logger, _siteOptions, p1, p2);
                var connectionManager = new ConnectionManager(_logger);
                _autoReconnector = new NewAutoReconnector(connectionManager, dummy, new MessageUntara(), _logger);

                //isInitialCommentを取得する
                var liveId = await GetCurrentLiveIdAsync(input);
                if (!string.IsNullOrEmpty(liveId))
                {
                    var initialComments = await Api.GetLiveComments(_server, liveId);
                    foreach (var c in initialComments)
                    {
                        var userId = c.UserId;
                        var isFirstComment = _first.IsFirstComment(userId);
                        var user = GetUser(userId);
                        var context = CreateMessageContext(c, true, "");
                        RaiseMessageReceived(context);
                    }
                }

                //接続開始
                await _autoReconnector.AutoReconnectAsync();
            }
            finally
            {
                p1.MessageReceived -= P1_MessageReceived;
                p1.MetadataUpdated -= P1_MetadataUpdated;
                p2.MetadataUpdated -= P2_MetadataUpdated;
            }
        }

        private void P2_MetadataUpdated(object sender, ILiveInfo e)
        {
            var liveInfo = e;
            RaiseMetadataUpdated(new Metadata
            {
                IsLive = liveInfo.IsLive,
                Title = liveInfo.Title,
                TotalViewers = liveInfo.TotalViewerNum.ToString(),
                CurrentViewers = liveInfo.OnlineUserNum.ToString(),
            });

        }

        private void P1_MetadataUpdated(object sender, IMetadata e)
        {
            var metadata = e;
            RaiseMetadataUpdated(metadata);
        }

        private void P1_MessageReceived(object sender, IMirrativMessage e)
        {
            var message = e;
            var messageContext = CreateMessageContext(message);
            if (messageContext != null)
            {
                RaiseMessageReceived(messageContext);
            }
        }
        private MirrativMessageContext CreateMessageContext(Message message, bool isInitialComment, string raw)
        {
            var userId = message.UserId;
            var isFirst = _first.IsFirstComment(userId);
            var user = GetUser(userId);
            var comment = new MirrativComment(message, raw);//InitialCommentにギフトが含まれている場合があったらバグ。
            var metadata = new CommentMessageMetadata(comment, _options, _siteOptions, user, this, isFirst)
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
        private MirrativMessageContext CreateMessageContext(IMirrativMessage message)
        {
            if (message is IMirrativComment comment)
            {
                var userId = comment.UserId;
                var isFirst = _first.IsFirstComment(userId);
                var user = GetUser(userId);
                //var comment = new MirrativComment(message, raw);
                var metadata = new CommentMessageMetadata(comment, _options, _siteOptions, user, this, isFirst)
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
            else if (message is IMirrativJoinRoom join && _siteOptions.IsShowJoinMessage)
            {
                var userId = join.UserId;
                var user = GetUser(userId);
                var metadata = new JoinMessageMetadata(join, _options, _siteOptions, user, this)
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
                var isFirst = _first.IsFirstComment(userId);
                var user = GetUser(userId);
                var metadata = new ItemMessageMetadata(item, _options, _siteOptions, user, this)
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
                return new MirrativMessageContext(item, metadata, methods);
            }
            else if (message is IMirrativConnected connected)
            {
                var metadata = new ConnectedMessageMetadata(connected, _options, _siteOptions)
                {
                    IsInitialComment = false,
                    SiteContextGuid = SiteContextGuid,
                };
                var methods = new MirrativMessageMethods();
                return new MirrativMessageContext(connected, metadata, methods);
            }
            else if (message is IMirrativDisconnected disconnected)
            {
                var metadata = new DisconnectedMessageMetadata(disconnected, _options, _siteOptions)
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
        public override void Disconnect()
        {
            _autoReconnector?.Disconnect();
        }
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
        public override async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
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

        public override IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.Mirrativ, userId);
        }

        public override Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }
        public MirrativCommentProvider2(IDataServer server, ILogger logger, ICommentOptions options, IMirrativSiteOptions siteOptions, IUserStoreManager userStoreManager)
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
