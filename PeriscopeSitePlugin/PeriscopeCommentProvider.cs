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
using SitePluginCommon.AutoReconnector;

namespace PeriscopeSitePlugin
{
    class PeriscopeConnector : IConnector
    {
        private readonly IDataServer _server;
        private readonly string _broadcastId;
        private readonly IMessageProvider _messageProvider;
        private readonly ILogger _logger;
        DisconnectReason _disconnectReason = DisconnectReason.Unknown;
        public async Task<DisconnectReason> ConnectAsync()
        {
            _disconnectReason = DisconnectReason.Unknown;
            var (avp, broadcastInfo) = await Api.GetAccessVideoPublicAsync(_server, _broadcastId);
            if (!IsBroadcastRunning(broadcastInfo))
            {
                return DisconnectReason.Finished;
            }
            var acp = await Api.GetAccessChatPublicAsync(_server, avp.ChatToken);
            var hostname = Tools.ExtractHostnameFromEndpoint(acp.Endpoint);
            if (hostname.Contains("replay"))
            {
                return DisconnectReason.Finished;
            }

            try
            {
                await _messageProvider.ReceiveAsync(hostname, acp.AccessToken, _broadcastId);
                //ここに来るのは、ユーザによる意図的な切断、配信終了、サーバ側が原因の異常な切断の場合。
            }
            catch(Exception ex)
            {
                _logger.LogException(ex);
            }
            return _disconnectReason;
        }
        public void Disconnect()
        {
            _messageProvider.Disconnect();
            _disconnectReason = DisconnectReason.User;
        }

        public async Task<bool> IsLivingAsync()
        {
            var (_, broadcastInfo) = await Api.GetAccessVideoPublicAsync(_server, _broadcastId);
            return IsBroadcastRunning(broadcastInfo);
        }
        private bool IsBroadcastRunning(BroadcastInfo broadcastInfo)
        {
            Debug.WriteLine($"Periscope BroadcastInfo.State: {broadcastInfo.State}");
            return broadcastInfo.State == "RUNNING";
        }
        public PeriscopeConnector(IDataServer server, string broadcastId, IMessageProvider messageProvider, ILogger logger)
        {
            _server = server;
            _broadcastId = broadcastId;
            _messageProvider = messageProvider;
            _logger = logger;
        }
    }
    internal class PeriscopeCommentProvider : CommentProviderBase
    {
        protected override void BeforeConnect()
        {
            base.BeforeConnect();
        }
        protected override void AfterDisconnected()
        {
            base.AfterDisconnected();
            _autoReconnector = null;
        }
        private async Task ConnectInternal2Async(string input, IBrowserProfile browserProfile)
        {
            var (channelName, broadcastId) = Tools.ExtractChannelNameAndLiveId(input);
            if (string.IsNullOrEmpty(broadcastId))
            {
                return;
            }
            var messageProvider = new MessageProvider(new Websocket(), _logger);
            messageProvider.Received += MessageProvider_Received;
            var connector = new PeriscopeConnector(_server, broadcastId, messageProvider, _logger);
            var me = new MessageUntara();
            me.SystemInfoReiceved += Me_SystemInfoReiceved;
            _autoReconnector = new AutoReconnector(connector, me);
            await _autoReconnector.AutoConnect();
            me.SystemInfoReiceved -= Me_SystemInfoReiceved;
            messageProvider.Received -= MessageProvider_Received;
        }

        private void Me_SystemInfoReiceved(object sender, SystemInfoEventArgs e)
        {
            SendSystemInfo(e.Message, e.Type);
        }

        AutoReconnector _autoReconnector;
        public override async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            BeforeConnect();
            try
            {
                await ConnectInternal2Async(input, browserProfile);
            }
            catch(Exception ex)
            {
                _logger.LogException(ex, "", $"input={input}");
            }
            finally
            {
                AfterDisconnected();
            }
        }
        FirstCommentDetector _first = new FirstCommentDetector();
        private MessageMetadata CreateMessageMetadata(IPeriscopeMessage message, IUser user, bool isFirstComment)
        {
            return new MessageMetadata(message, _options, _siteOptions, user, this, isFirstComment)
            {
                SiteContextGuid = SiteContextGuid,
            };
        }
        private void MessageProvider_Received(object sender, IInternalMessage e)
        {
            if(e is Kind1Type1 kind1Type1)
            {
                var message = new PeriscopeComment(kind1Type1);
                var userId = message.UserId;
                var isFirstComment = _first.IsFirstComment(userId);
                var user = GetUser(userId);
                user.Name = message.NameItems;
                var metadata = CreateMessageMetadata(message, user, isFirstComment);
                var methods = new MessageMethods();
                RaiseMessageReceived(new MessageContext(message, metadata, methods));
            }
            else if(e is Kind2Kind1 kind2kind1)
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
                user.Name = message.NameItems;
                var metadata = CreateMessageMetadata(message, user, isFirstComment);
                var methods = new MessageMethods();
                RaiseMessageReceived(new MessageContext(message, metadata, methods));
            }
            else if(e is Kind2Kind2 kind2Kind2)
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
                user.Name = message.NameItems;
                var metadata = CreateMessageMetadata(message, user, isFirstComment);
                var methods = new MessageMethods();
                RaiseMessageReceived(new MessageContext(message, metadata, methods));
            }
        }
        public override void Disconnect()
        {
            _autoReconnector?.Disconnect();
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
