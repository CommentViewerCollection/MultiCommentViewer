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

namespace ShowRoomSitePlugin
{
    internal class ShowRoomCommentProvider : CommentProviderBase
    {
        protected override void BeforeConnect()
        {
            base.BeforeConnect();
            _cts = new CancellationTokenSource();
            _messageProvider = new MessageProvider(new Websocket(), _logger);
            _messageProvider.Received += MessageProvider_Received;
        }
        private MessageMetadata CreateMessageMetadata(IShowRoomComment message, IUser user, bool isFirstComment)
        {
            return new MessageMetadata(message, _options, _siteOptions, user, this, isFirstComment)
            {
                SiteContextGuid = SiteContextGuid,
            };
        }
        private void MessageProvider_Received(object sender, IInternalMessage e)
        {
            switch (e)
            {
                case T1 t1:
                    {
                        if(_siteOptions.IsIgnore50Counts && int.TryParse(t1.Cm, out int count))
                        {
                            if(count >= 1 && count <= 50)
                            {
                                //1-50の数字のみのコメントは無視する
                                return;
                            }
                        }
                        var message = new ShowRoomComment(t1);
                        var userId = message.UserId;
                        var isFirstComment = _first.IsFirstComment(userId);
                        var user = GetUser(userId);
                        user.Name = message.NameItems;
                        var metadata = CreateMessageMetadata(message, user, isFirstComment);
                        var methods = new MessageMethods();
                        RaiseMessageReceived(new MessageContext(message, metadata, methods));
                    }
                    break;
            }
        }

        protected override void AfterDisconnected()
        {
            base.AfterDisconnected();
            _messageProvider.Received -= MessageProvider_Received;
            _messageProvider = null;
        }
        private async Task ConnectInternalAsync(string input, IBrowserProfile browserProfile)
        {
            var roomName = GetRoomNameFromInput(input);
            if (string.IsNullOrEmpty(roomName))
            {
                throw new Exception("invalid input");
            }
            var livePageUrl = "https://www.showroom-live.com/" + roomName;
            var livePageHtml = await _server.GetAsync(livePageUrl);
            var match = Regex.Match(livePageHtml, "room_id=(\\d+)");
            if (!match.Success)
            {
                throw new Exception("room_idが無い");
            }
            var room_id = match.Groups[1].Value;
            var liveInfo = await Api.GetLiveInfo(_server, room_id);
            if(liveInfo.LiveStatus == 1)
            {
                //放送終了？
                return;
            }
            await _messageProvider.ReceiveAsync(liveInfo.BcsvrHost, liveInfo.BcsvrKey);
            return;
        }
        private string GetRoomNameFromInput(string input)
        {
            var match = Regex.Match(input, "showroom-live.com/([^/?#]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        public override async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            BeforeConnect();
            try
            {
                await ConnectInternalAsync(input, browserProfile);
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

        IMessageProvider _messageProvider;
        CancellationTokenSource _cts;
        public override void Disconnect()
        {
            _cts?.Cancel();
            _messageProvider?.Disconnect();
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
            return _userStoreManager.GetUser(SiteType.ShowRoom, userId);
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
        private readonly IShowRoomSiteOptions _siteOptions;
        private readonly IUserStoreManager _userStoreManager;
        public ShowRoomCommentProvider(IDataServer server, ILogger logger, ICommentOptions options, IShowRoomSiteOptions siteOptions, IUserStoreManager userStoreManager)
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
