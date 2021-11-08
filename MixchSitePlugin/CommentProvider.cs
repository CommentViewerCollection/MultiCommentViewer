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
        private void BeforeConnecting()
        {
            CanConnect = false;
            CanDisconnect = true;
            _isExpectedDisconnect = false;
            _lastReceiveTime = DateTime.Now;
            _keepaliveTimer.Enabled = true;
            _poipoiStockTimer.Enabled = true;
        }
        private void AfterDisconnected()
        {
            _keepaliveTimer.Enabled = false;
            _poipoiStockTimer.Enabled = false;
            _ws = null;
            CanConnect = true;
            CanDisconnect = false;
        }
        protected virtual List<Cookie> GetCookies(IBrowserProfile browserProfile)
        {
            List<Cookie> cookies = null;
            try
            {
                cookies = browserProfile.GetCookieCollection("mixch.tv");
            }
            catch { }
            return cookies ?? new List<Cookie>();
        }
        protected virtual CookieContainer CreateCookieContainer(IBrowserProfile browserProfile)
        {
            var cc = new CookieContainer();
            try
            {
                var cookies = browserProfile.GetCookieCollection("mixch.tv");
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
            LiveUrlInfo liveUrlInfo;
            try
            {
                liveUrlInfo = await Tools.GetLiveId(_dataSource, input);
            }
            catch (InvalidInputException ex)
            {
                _logger.LogException(ex, "無効な入力値", $"input={input}");
                SendSystemInfo("無効な入力値です", InfoType.Error);
                AfterDisconnected();
                return;
            }

            // TODO: 過去のコメントを取得する

            while (!_isExpectedDisconnect)
            {
                _ws = CreateMixchWebsocket();
                _ws.Received += WebSocket_Received;

                var wsTask = _ws.ReceiveAsync(liveUrlInfo, "", null);

                var tasks = new List<Task> { wsTask };

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
            }
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
            var userId = p.UserId.ToString();
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
            nameItems.Add(MessagePartFactory.CreateMessageText(p.Name));
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
                MixchMessageType = (MixchMessageType)p.Kind,
                MessageItems = messageItems,
                Id = "", // ミクチャはメッセージにIDが存在しない
                NameItems = nameItems,
                PostTime = DateTimeOffset.FromUnixTimeSeconds(p.Created).LocalDateTime,
                UserId = p.UserId.ToString(),
                IsFirstComment = isFirstComment,
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
        #endregion //Fields

        #region ctors
        System.Timers.Timer _keepaliveTimer = new System.Timers.Timer();
        System.Timers.Timer _poipoiStockTimer = new System.Timers.Timer();
        public CommentProvider(ICommentOptions options, MixchSiteOptions siteOptions, ILogger logger, IUserStoreManager userStoreManager)
        {
            _options = options;
            _siteOptions = siteOptions;
            _logger = logger;
            _userStoreManager = userStoreManager;
            _dataSource = new DataSource();

            _keepaliveTimer.Interval = 1000;
            _keepaliveTimer.Elapsed += _KeepaliveTimer_Elapsed;
            _keepaliveTimer.AutoReset = true;

            _poipoiStockTimer.Interval = 1000;
            _poipoiStockTimer.Elapsed += _PoipoiStockTimer_Elapsed;
            _poipoiStockTimer.AutoReset = true;

            CanConnect = true;
            CanDisconnect = false;
        }

        // 指定秒数以上パケットの受信がなければWebSocketを再接続する
        // 7秒毎にステータス更新パケットが届くので、コメントがなくても通信が正常なら最終受信時間は更新される
        const int secondToReconnect = 15;
        private DateTime _lastReceiveTime;
        private void _KeepaliveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var diffTime = DateTime.Now - _lastReceiveTime;
            if (diffTime.Seconds >= secondToReconnect && _ws != null)
            {
                SendSystemInfo($"{secondToReconnect}秒以上データ受信がなかったので再接続します", InfoType.Notice);
                _lastReceiveTime = DateTime.Now;
                _ws.Disconnect();
            }
        }
        private void _PoipoiStockTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var unixTimestampNow = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            List<string> processedKeys = new List<string>();
            lock (_poipoiStockDict)
            {
                foreach (KeyValuePair<string, Packet> _poipoiStock in _poipoiStockDict)
                {
                    if (_poipoiStock.Value.Created + _siteOptions.PoipoiKeepSeconds <= unixTimestampNow)
                    {
                        var messageContext = CreateMessageContext(_poipoiStock.Value, false);
                        if (messageContext != null)
                        {
                            MessageReceived?.Invoke(this, messageContext);
                        }
                        processedKeys.Add(_poipoiStock.Key);
                    }
                }
                foreach (string key in processedKeys)
                {
                    _poipoiStockDict.Remove(key);
                }
            }
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
        Dictionary<string, Packet> _poipoiStockDict = new Dictionary<string, Packet>();

        private void WebSocket_Received(object sender, Packet p)
        {
            try
            {
                if (p.IsStatus())
                {
                    MetadataUpdated?.Invoke(this, new Metadata
                    {
                        Title = p.Title,
                        Elapsed = Tools.ElapsedToString(p.Elapsed),
                        Others = p.DisplayPointString(),
                    });
                    LiveStatus = p.Status;
                }
                else if ((MixchMessageType)p.Kind == MixchMessageType.PoiPoi)
                {
                    lock (_poipoiStockDict)
                    {
                        if (_poipoiStockDict.ContainsKey(p.PoiPoiKey()))
                        {
                            var _p = _poipoiStockDict[p.PoiPoiKey()];
                            _p.Count += p.Count;
                        }
                        else
                        {
                            _poipoiStockDict[p.PoiPoiKey()] = p;
                        }
                    }
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
            _lastReceiveTime = DateTime.Now;
        }
        public async Task PostCommentAsync(string str)
        {
            throw new NotImplementedException();
        }

        public async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            // FIXME: コメビュでhttpsアクセスするとWeb版で開いているライブ視聴画面でアイテムの送信ができなくなる。
            // リロードで直るので何らかのセッション不整合が起きている模様。
            // 今現在はユーザー情報を使っていないのでログイン情報の取得を停止する

            // var cc = CreateCookieContainer(browserProfile);
            // var me = await API.GetMeAsync(_dataSource, cc);
            // return new CurrentUserInfo
            // {
            //     IsLoggedIn = !string.IsNullOrEmpty(me.UserId),
            //     UserId = me.UserId,
            //     Username = me.DisplayName,
            // };
            //
            return new CurrentUserInfo { };
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
