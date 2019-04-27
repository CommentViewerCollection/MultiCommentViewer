using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using SitePluginCommon;

namespace PeriscopeSitePlugin
{
    internal class PeriscopeCommentProvider : CommentProviderBase
    {
        protected override void BeforeConnect()
        {
            base.BeforeConnect();
            _isUserDisconnected = false;
            _cts = new CancellationTokenSource();
        }
        protected override void AfterDisconnected()
        {
            base.AfterDisconnected();
            _messageProvider = null;
        }
        private bool IsBroadcastRunning(BroadcastInfo broadcastInfo)
        {
            return broadcastInfo.State == "RUNNING";
        }
        public override async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            BeforeConnect();
            var autoReconnectMode = false;
            var cc = GetCookieContainer(browserProfile, "pscp.tv");
            var broadcastId = Tools.ExtractLiveId(input);
            var (avp, broadcastInfo) = await Api.GetAccessVideoPublicAsync(_server, broadcastId);
            await Api.GetAccessVideoAsync(_server, broadcastId, cc);
            if (!IsBroadcastRunning(broadcastInfo))
            {
                SendSystemInfo("放送が終了しているため切断します", InfoType.Notice);
                AfterDisconnected();
                return;
            }
            var acp = await Api.GetAccessChatPublicAsync(_server, avp.ChatToken);
            _messageProvider = new MessageProvider(new Websocket(), _logger);
            //_messageProvider.
            _messageProvider.Received += MessageProvider_Received;
            var hostname = Tools.ExtractHostnameFromEndpoint(acp.Endpoint);
            if (hostname.Contains("replay"))
            {

            }
            await _messageProvider.ReceiveAsync(hostname, acp.AccessToken, broadcastId);

            AfterDisconnected();
        }
        FirstCommentDetector _first = new FirstCommentDetector();
        private void MessageProvider_Received(object sender, IInternalMessage e)
        {
            if(e is Kind1Type1 kind1Type1)
            {
                var message = new PeriscopeComment(kind1Type1);
                var userId = message.UserId;
                var isFirstComment = _first.IsFirstComment(userId);
                var user = GetUser(userId);
                var metadata = new MessageMetadata(message, _options, _siteOptions, user, this, isFirstComment);
                var methods = new MessageMethods();
                RaiseMessageReceived(new MessageContext(message, metadata, methods));
            }
            else if(e is Kind2Kind1 kind2kind1)
            {
                var message = new PeriscopeJoin(kind2kind1);
                var userId = message.UserId;
                var isFirstComment = false;
                var user = GetUser(userId);
                var metadata = new MessageMetadata(message, _options, _siteOptions, user, this, isFirstComment);
                var methods = new MessageMethods();
                RaiseMessageReceived(new MessageContext(message, metadata, methods));
            }
            else if(e is Kind2Kind2 kind2Kind2)
            {
                var message = new PeriscopeLeave(kind2Kind2);
                var userId = message.UserId;
                var isFirstComment = false;
                var user = GetUser(userId);
                var metadata = new MessageMetadata(message, _options, _siteOptions, user, this, isFirstComment);
                var methods = new MessageMethods();
                RaiseMessageReceived(new MessageContext(message, metadata, methods));
            }
        }

        IMessageProvider _messageProvider;
        /// <summary>
        /// ユーザによる切断か
        /// </summary>
        bool _isUserDisconnected;
        CancellationTokenSource _cts;
        public override void Disconnect()
        {
            _isUserDisconnected = true;
            _cts?.Cancel();
        }

        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        public override async Task PostCommentAsync(string text)
        {
            await Task.FromResult<object>(null);
        }

        public override IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.Periscope, userId);
        }

        public override async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var userInfo = new CurrentUserInfo
            {

            };
            return await Task.FromResult(userInfo);
        }

        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly ICommentOptions _options;
        private readonly PeriscopeSiteOptions _siteOptions;
        private readonly IUserStoreManager _userStoreManager;
        public PeriscopeCommentProvider(IDataServer server, ILogger logger, ICommentOptions options, PeriscopeSiteOptions siteOptions, IUserStoreManager userStoreManager)
            : base(logger, options)
        {
            _server = server;
            _logger = logger;
            _options = options;
            _siteOptions = siteOptions;
            _userStoreManager = userStoreManager;
        }
    }
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
