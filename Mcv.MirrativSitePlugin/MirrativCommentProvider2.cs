using Mcv.PluginV2;
using Mcv.PluginV2.AutoReconnection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
namespace MirrativSitePlugin
{
    class MirrativCommentProvider2 : CommentProviderBase
    {
        FirstCommentDetector _first = new FirstCommentDetector();
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly IMirrativSiteOptions _siteOptions;

        public override async Task ConnectAsync(string input, List<Cookie> cookies)
        {
            BeforeConnect();
            try
            {
                await ConnectInternalAsync(input, cookies);
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
        MessageProvider2 _p1;
        MetadataProvider2 _p2;
        bool _isInitialized;
        public async Task InitAsync()
        {
            if (_isInitialized) return;
            var p1 = new MessageProvider2(new WebSocket("wss://online.mirrativ.com/"), _logger);
            p1.MessageReceived += P1_MessageReceived;
            p1.MetadataUpdated += P1_MetadataUpdated;
            _p1 = p1;
            var p2 = new MetadataProvider2(_server, _siteOptions);
            p2.MetadataUpdated += P2_MetadataUpdated;
            p2.Master = p1;
            _p2 = p2;
        }
        private async Task ConnectInternalAsync(string input, List<Cookie> cookies)
        {
            await InitAsync();
            //var p1 = new MessageProvider2(new WebSocket("wss://online.mirrativ.com/"), _logger);
            //p1.MessageReceived += P1_MessageReceived;
            //p1.MetadataUpdated += P1_MetadataUpdated;
            //var p2 = new MetadataProvider2(_server, _siteOptions);
            //p2.MetadataUpdated += P2_MetadataUpdated;
            //p2.Master = p1;
            try
            {
                var dummy = new DummyImpl(_server, input, _logger, _siteOptions, _p1, _p2);
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
                        var context = CreateMessageContext(c, true, "");
                        RaiseMessageReceived(context);
                    }
                }

                //接続開始
                await _autoReconnector.AutoReconnectAsync();
            }
            finally
            {
                //p1.MessageReceived -= P1_MessageReceived;
                //p1.MetadataUpdated -= P1_MetadataUpdated;
                //p2.MetadataUpdated -= P2_MetadataUpdated;
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
            var comment = new MirrativComment(message, raw);//InitialCommentにギフトが含まれている場合があったらバグ。
            string? newNickname = null;
            if (_siteOptions.NeedAutoSubNickname)
            {
                var nick = Utils.ExtractNickname(message.Comment);
                if (!string.IsNullOrEmpty(nick))
                {
                    newNickname = nick;
                }
            }
            return new MirrativMessageContext(comment, userId, newNickname);
        }
        private MirrativMessageContext CreateMessageContext(IMirrativMessage message)
        {
            if (message is IMirrativComment comment)
            {
                var userId = comment.UserId;
                string? newNickname = null;
                if (_siteOptions.NeedAutoSubNickname)
                {
                    var messageText = comment.Text;
                    var nick = Utils.ExtractNickname(messageText);
                    if (!string.IsNullOrEmpty(nick))
                    {
                        newNickname = nick;
                    }
                }
                return new MirrativMessageContext(comment, userId, newNickname);
            }
            else if (message is IMirrativJoinRoom join && _siteOptions.IsShowJoinMessage)
            {
                var userId = join.UserId;
                return new MirrativMessageContext(join, userId, null); ;
            }
            else if (message is IMirrativItem item)
            {
                var userId = item.UserId;
                string? newNickname = null;
                if (_siteOptions.NeedAutoSubNickname)
                {
                    var nick = Utils.ExtractNickname(item.Text);
                    if (!string.IsNullOrEmpty(nick))
                    {
                        newNickname = nick;
                    }
                }
                return new MirrativMessageContext(item, userId, newNickname);
            }
            else if (message is IMirrativConnected connected)
            {
                return new MirrativMessageContext(connected, null, null);
            }
            else if (message is IMirrativDisconnected disconnected)
            {
                return new MirrativMessageContext(disconnected, null, null);
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
        protected virtual CookieContainer GetCookieContainer(List<Cookie> cookies)
        {
            var cc = new CookieContainer();

            try
            {
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
        public override async Task<ICurrentUserInfo> GetCurrentUserInfo(List<Cookie> cookies)
        {
            var cc = GetCookieContainer(cookies);
            var currentUser = await Api.GetCurrentUserAsync(_server, cc);

            return new CurrentUserInfo
            {
                IsLoggedIn = currentUser.IsLoggedIn,
                UserId = currentUser.UserId,
                Username = currentUser.Name,
            };
        }
        public override Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }

        public override async void SetMessage(string raw)
        {
            await InitAsync();
            _p1.SetMessage(raw);
        }

        public MirrativCommentProvider2(IDataServer server, ILogger logger, IMirrativSiteOptions siteOptions)
            : base(logger)
        {
            _server = server;
            _logger = logger;
            _siteOptions = siteOptions;
        }
    }
}
