using System;
using Common;
using System.Windows.Threading;
using SitePlugin;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net.Http;
using ryu_s.BrowserCookie;
using SitePluginCommon;

namespace TwicasSitePlugin
{
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
    class TwicasCommentProvider : ICommentProvider
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
        protected virtual CookieContainer CreateCookieContainer(IBrowserProfile browserProfile)
        {
            var cc = new CookieContainer();
            try
            {
                var cookies = browserProfile.GetCookieCollection("twitcasting.tv");
                foreach (var cookie in cookies)
                {
                    cc.Add(cookie);
                }
            }
            catch { }
            return cc;
        }
        private void SendSystemInfo(string message, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(message) },
                NameItems = null,
                SiteType = SiteType.Twicas,
                Type = type,
            }, _options);
            MessageReceived?.Invoke(this, context);
        }
        private CookieContainer _cc;
        MessageProvider _messageProvider;
        string _csSessionId;
        string _broadcasterId;
        long _liveId = -1;
        FirstCommentDetector _first = new FirstCommentDetector();
        public async Task ConnectAsync(string input, global::ryu_s.BrowserCookie.IBrowserProfile browserProfile)
        {
            var broadcasterId = Tools.ExtractBroadcasterId(input);
            _broadcasterId = broadcasterId;
            if (string.IsNullOrEmpty(broadcasterId))
            {
                //Info
                return;
            }
            _cc = CreateCookieContainer(browserProfile);

            CanConnect = false;
            CanDisconnect = true;
            _first.Reset();
            int cnum = -1;

            string audienceId;
            try
            {
                var (context, contextRaw) = await API.GetLiveContext(_server, broadcasterId, _cc);
                cnum = context.MovieCnum;
                _liveId = context.MovieId;
                Connected?.Invoke(this, new ConnectedEventArgs
                {
                    IsInputStoringNeeded = true,
                    UrlToRestore = broadcasterId,
                });
                if (!string.IsNullOrEmpty(context.AudienceId))
                {
                    audienceId = context.AudienceId;
                    SendSystemInfo($"ログイン済みユーザID:{audienceId}", InfoType.Notice);
                    IsLoggedIn = true;
                    _csSessionId = context.CsSessionId;
                }
                else
                {
                    SendSystemInfo("未ログイン", InfoType.Notice);
                    IsLoggedIn = false;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogException(ex);
                string message;
                if (ex.InnerException != null)
                {
                    message = ex.InnerException.Message;
                }
                else
                {
                    message = ex.Message;
                }
                SendSystemInfo(message, InfoType.Debug);
            }
            catch (InvalidBroadcasterIdException ex)
            {
                _logger.LogException(ex, "", $"input=\"{input}\"");
                SendSystemInfo("入力されたURLまたはIDは存在しないか無効な値です", InfoType.Notice);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
                SendSystemInfo(ex.Message, InfoType.Debug);
            }
            if(cnum < 0 || _liveId < 0)
            {
                AfterDisconnected();
                return;
            }
            try
            {
                _messageProvider = new MessageProvider(_server, _siteOptions, _cc, _userStore, _options, this, _logger)
                {
                    SiteContextGuid = SiteContextGuid,
                };
                _messageProvider.MetaReceived += MessageProvider_MetaReceived;
                _messageProvider.MessageReceived += MessageProvider_MessageReceived;
                _messageProvider.InfoOccured += MessageProvider_InfoOccured;

                await _messageProvider.ConnectAsync(broadcasterId, cnum, _liveId);
            }
            catch (Exception ex)
            {
                SendSystemInfo(ex.Message, InfoType.Error);
                _logger.LogException(ex);
            }
            finally
            {
                AfterDisconnected();
            }
        }

        private void MessageProvider_MessageReceived(object sender, IMessageContext e)
        {
            if(e.Message is ITwicasComment comment)
            {
                _lastCommentId = long.Parse(comment.Id);
            }
            MessageReceived?.Invoke(this, e);
        }

        private void AfterDisconnected()
        {
            _messageProvider = null;
            CanConnect = true;
            CanDisconnect = false;
        }
        private void MessageProvider_InfoOccured(object sender, InfoData e)
        {
            SendSystemInfo(e.Message, e.Type);
        }
        private long _lastCommentId;
        private void MessageProvider_MetaReceived(object sender, IMetadata e)
        {
            MetadataUpdated?.Invoke(this, e);
        }

        public void Disconnect()
        {
            if(_messageProvider != null)
            {
                _messageProvider.Disconnect();
            }
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
            var (comments, raw) = await API.PostCommentAsync(_server, _broadcasterId, _liveId, _lastCommentId, text, _csSessionId, _cc);
            //var ms = new List<ICommentData>();
            //foreach(var c in comments)
            //{
            //    var data = Tools.Parse(c);
            //    Debug.WriteLine($"{data.Id} {Tools.ToText(data.Message)} PostCommentAsync");
            //    ms.Add(data);
            //}
            //OnReceiveComments(ms);
        }

        public Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var cc = CreateCookieContainer(browserProfile);
            string name = null;
            foreach(var cookie in Tools.ExtractCookies(cc))
            {
                switch (cookie.Name)
                {
                    case "tc_id":
                        name = cookie.Value;
                        break;
                }
            }
            var info = new CurrentUserInfo
            {
                IsLoggedIn = !string.IsNullOrEmpty(name),
                Username = name,
            };
            return Task.FromResult<ICurrentUserInfo>(info);
        }

        public event EventHandler LoggedInStateChanged;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<IMessageContext> MessageReceived;

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                if (_isLoggedIn == value) return;
                _isLoggedIn = value;
                LoggedInStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public Guid SiteContextGuid { get; set; }
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly ICommentOptions _options;
        private readonly ITwicasSiteOptions _siteOptions;
        private readonly IUserStore _userStore;
        public TwicasCommentProvider(IDataServer server, ILogger logger, ICommentOptions options, TwicasSiteOptions siteOptions, IUserStore userStore)
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
}
