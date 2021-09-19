using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SitePlugin;
using ryu_s.BrowserCookie;
using Common;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using SitePluginCommon;

namespace MixchSitePlugin
{
    [Serializable]
    public class InvalidInputException : Exception
    {
        public InvalidInputException() { }
    }
    class CommentProvider : ICommentProvider
    {
        string _liveId;
        #region ICommentProvider
        #region Events
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<IMessageContext> MessageReceived;
        #endregion //Events

        #region CanConnect
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
        #endregion //CanConnect

        #region CanDisconnect
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
        #endregion //CanDisconnect

        const int LiveStatusUnknown = -1;
        const int LiveStatusOnAir = 0;
        const int LiveStatusOffAir = 1;
        private int _liveStatus = LiveStatusUnknown;
        public int LiveStatus
        {
            get { return _liveStatus; }
            set
            {
                if (_liveStatus == value)
                    return;
                if (_liveStatus != LiveStatusUnknown)
                {
                    if (value == LiveStatusOnAir)
                    {
                        SendSystemInfo("配信が開始しました", InfoType.Notice);
                    }
                    else if (value == LiveStatusOffAir)
                    {
                        SendSystemInfo("配信が終了しました", InfoType.Notice);
                    }
                }
                else
                {
                    if (value == LiveStatusOffAir)
                    {
                        SendSystemInfo("配信が開始されていません", InfoType.Notice);
                    }
                }
                _liveStatus = value;
            }
        }

        protected virtual Task<string> GetLiveId(string input)
        {
            return Tools.GetLiveId(_dataSource, input);
        }
        private void BeforeConnecting()
        {
            CanConnect = false;
            CanDisconnect = true;
            _isExpectedDisconnect = false;
        }
        private void AfterDisconnected()
        {
            _ws = null;
            CanConnect = true;
            CanDisconnect = false;
        }
        protected virtual List<Cookie> GetCookies(IBrowserProfile browserProfile)
        {
            List<Cookie> cookies = null;
            try
            {
                cookies = browserProfile.GetCookieCollection(MixchSiteContext.MixchDomain);
            }
            catch { }
            return cookies ?? new List<Cookie>();
        }
        protected virtual CookieContainer CreateCookieContainer(IBrowserProfile browserProfile)
        {
            var cc = new CookieContainer();
            try
            {
                var cookies = browserProfile.GetCookieCollection(MixchSiteContext.MixchCookieDomain);
                foreach (var cookie in cookies)
                {
                    cc.Add(cookie);
                }
            }
            catch { }
            return cc;
        }
        private async Task ConnectInternalAsync(string input, IBrowserProfile browserProfile)
        {
            if (_ws != null)
            {
                throw new InvalidOperationException("");
            }
            var cookies = GetCookies(browserProfile);
            _cc = CreateCookieContainer(cookies);
            string liveId;
            try
            {
                liveId = await GetLiveId(input);
                _liveId = liveId;
            }
            catch (InvalidInputException ex)
            {
                _logger.LogException(ex, "無効な入力値", $"input={input}");
                SendSystemInfo("無効な入力値です", InfoType.Error);
                AfterDisconnected();
                return;
            }

        // TODO: ライブが配信中かチェックする
        // TODO: 過去のコメントを取得する
        Reconnect:
            _ws = CreateMixchWebsocket();
            _ws.Received += WebSocket_Received;

            var userAgent = GetUserAgent(browserProfile.Type);
            var wsTask = _ws.ReceiveAsync(liveId, userAgent, cookies);

            var tasks = new List<Task>
            {
                wsTask,
            };

            while (tasks.Count > 0)
            {
                var t = await Task.WhenAny(tasks);
                try
                {
                    await wsTask;
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
                tasks.Remove(wsTask);
                SendSystemInfo("wsタスク終了", InfoType.Debug);
            }
            _ws.Received -= WebSocket_Received;

            // TODO: 意図的ではない切断の場合は再接続する
        }
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            BeforeConnecting();
            try
            {
                await ConnectInternalAsync(input, browserProfile);
            }
            finally
            {
                AfterDisconnected();
            }
        }

        protected virtual IMixchWebsocket CreateMixchWebsocket()
        {
            return new MixchWebsocket(_logger);
        }

        private CookieContainer CreateCookieContainer(List<Cookie> cookies)
        {
            var cc = new CookieContainer();
            try
            {
                foreach (var cookie in cookies)
                {
                    cc.Add(cookie);
                }
            }
            catch { }
            return cc;
        }

        private MixchMessageContext CreateMessageContext(Packet p, bool isInitialComment)
        {
            var userId = p.user_id.ToString();
            var user = GetUser(userId) as IUser2;
            if (!_userDict.ContainsKey(userId))
            {
                _userDict.AddOrUpdate(user.UserId, user, (id, u) => u);
            }
            bool isFirstComment;
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

            var nameItems = new List<IMessagePart>();
            nameItems.Add(MessagePartFactory.CreateMessageText(p.name));
            user.Name = nameItems;

            var messageItems = new List<IMessagePart>();
            var messageBody = p.Message();
            if (!string.IsNullOrEmpty(messageBody))
            {
                messageItems.Add(MessagePartFactory.CreateMessageText(messageBody));
            }

            MixchMessageContext messageContext = null;
            IMixchMessage message;
            message = new MixchMessage("")
            {
                MixchMessageType = (MixchMessageType)p.kind,
                MessageItems = messageItems,
                Id = "", // ミクチャはメッセージにIDが存在しない
                NameItems = nameItems,
                PostTime = DateTimeOffset.FromUnixTimeSeconds(p.created).LocalDateTime,
                UserId = p.user_id.ToString(),
            };
            var metadata = new MessageMetadata(message, _options, _siteOptions, user, this, isFirstComment)
            {
                IsInitialComment = isInitialComment,
                SiteContextGuid = SiteContextGuid,
            };
            var methods = new MixchMessageMethods();
            messageContext = new MixchMessageContext(message, metadata, methods);
            return messageContext;
        }

        IMixchWebsocket _ws;
        /// <summary>
        /// ユーザが意図した切断か
        /// </summary>
        bool _isExpectedDisconnect;

        public void Disconnect()
        {
            _isExpectedDisconnect = true;
            if (_ws != null)
            {
                _ws.Disconnect();
            }
        }
        public IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.Mixch, userId);
        }
        #endregion //ICommentProvider


        #region Fields
        private ICommentOptions _options;
        private MixchSiteOptions _siteOptions;
        private ILogger _logger;
        private IUserStoreManager _userStoreManager;
        private readonly IDataSource _dataSource;
        private CookieContainer _cc;
        #endregion //Fields

        #region ctors
        public CommentProvider(ICommentOptions options, MixchSiteOptions siteOptions, ILogger logger, IUserStoreManager userStoreManager)
        {
            _options = options;
            _siteOptions = siteOptions;
            _logger = logger;
            _userStoreManager = userStoreManager;
            _dataSource = new DataSource();

            CanConnect = true;
            CanDisconnect = false;
        }
        #endregion //ctors

        #region Events
        #endregion //Events
        private void SendSystemInfo(string message, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                Text = message,
                SiteType = SiteType.Mixch,
                Type = type,
            }, _options);
            MessageReceived?.Invoke(this, context);
        }
        public Guid SiteContextGuid { get; set; }
        Dictionary<string, int> _userCommentCountDict = new Dictionary<string, int>();
        [Obsolete]
        Dictionary<string, UserViewModel> _userViewModelDict = new Dictionary<string, UserViewModel>();
        ConcurrentDictionary<string, IUser2> _userDict = new ConcurrentDictionary<string, IUser2>();
        private static string GetUserAgent(BrowserType browser)
        {
            string userAgent;
            switch (browser)
            {
                case BrowserType.Chrome:
                    userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36";
                    break;
                case BrowserType.Firefox:
                    userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
                    break;
                case BrowserType.IE:
                    userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; .NET4.0C; .NET4.0E; .NET CLR 2.0.50727; .NET CLR 3.0.30729; .NET CLR 3.5.30729; rv:11.0) like Gecko";
                    break;
                case BrowserType.Opera:
                    userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36 OPR/43.0.2442.1144";
                    break;
                default:
                    throw new Exception("未対応のブラウザ");
            }
            return userAgent;
        }

        private void WebSocket_Received(object sender, Packet p)
        {
            try
            {
                if (p.IsStatus())
                {
                    MetadataUpdated?.Invoke(this, new Metadata
                    {
                        Title = p.title,
                        Elapsed = Tools.ElapsedToString(p.elapsed),
                        Others = p.DisplayPoint(),
                    });
                    LiveStatus = p.status;
                }
                else if (p.HasMessage())
                {
                    var messageContext = CreateMessageContext(p, false);
                    if (messageContext != null)
                    {
                        MessageReceived?.Invoke(this, messageContext);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        public async Task PostCommentAsync(string str)
        {
            throw new NotImplementedException();
        }

        public async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var cc = CreateCookieContainer(browserProfile);
            var me = await API.GetMeAsync(_dataSource, cc);
            return new CurrentUserInfo
            {
                IsLoggedIn = !string.IsNullOrEmpty(me.UserId),
                UserId = me.UserId,
                Username = me.DisplayName,
            };
        }

        public void SetMessage(string raw)
        {
            throw new NotImplementedException();
        }
    }
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
