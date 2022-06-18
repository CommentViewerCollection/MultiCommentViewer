using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using SitePluginCommon;

namespace LineLiveSitePlugin
{
    internal class LineLiveCommentProvider : ICommentProvider
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
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<IMessageContext> MessageReceived;

        private string _channelName;
        private IMessageProvider _provider;
        //private CookieContainer _cc;
        protected Dictionary<string, string> _loveIconUrlDict;
        bool _isFirstConnection;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        /// <exception cref="LiveNotFoundException"></exception>
        protected virtual async Task<string> GetLiveIdFromChannelId(IDataServer server, string channelId)
        {
            var (channelInfo, raw) = await Api.GetChannelInfo(server, channelId);
            if (channelInfo.LiveBroadcasts.Rows.Count > 0)
            {
                var row0 = channelInfo.LiveBroadcasts.Rows[0];
                return row0.Id.ToString();
            }
            else
            {
                //配信してない
                throw new LiveNotFoundException();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="InvalidUrlException"></exception>
        protected virtual (string channelId, string liveId) GetLiveIdFromInput(string input)
        {
            //https://live.line.me/channels/2234253/broadcast/8383700
            var match = Regex.Match(input, "line\\.me/channels/(\\d+)/broadcast/(\\d+)");
            if (match.Success)
            {
                var channelId = match.Groups[1].Value;
                var liveId = match.Groups[2].Value;
                return (channelId, liveId);
            }
            var match0 = Regex.Match(input, "line\\.me/channels/(\\d+)");
            if (match0.Success)
            {
                var channelId = match0.Groups[1].Value;
                return (channelId, null);
            }
            //URLが不正
            throw new InvalidUrlException();
        }
        private void BeforeConnect()
        {
            CanConnect = false;
            CanDisconnect = true;
            _isUserDisconnected = false;
            _loveIconUrlDict = new Dictionary<string, string>();
            _isFirstConnection = true;
            _channelName = "";
            _cts = new CancellationTokenSource();
            _first.Reset();
        }
        private void AfterDisconnected()
        {
            CanConnect = true;
            CanDisconnect = false;
        }
        protected virtual CookieContainer GetCookieContainer(IBrowserProfile browserProfile)
        {
            var cc = new CookieContainer();
            try
            {
                var cookies = browserProfile.GetCookieCollection("live.line.me");
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
        public int LiveCheckIntervalMs { get; set; } = 30 * 1000;
        protected virtual async Task InitLoveIconUrlDict()
        {
            var lovesV4 = await Api.GetLovesV4(_server);
            foreach (var item in lovesV4.Items)
            {
                _loveIconUrlDict.Add(item.ItemId, item.ThumbnailUrl);
            }
        }
        protected virtual Task<(ILiveInfo, string raw)> GetLiveInfo(string channelId, string liveId)
        {
            return Api.GetLiveInfoV4(_server, channelId, liveId);
        }
        FirstCommentDetector _first = new FirstCommentDetector();
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            BeforeConnect();
            var autoReconnectMode = false;
            var cc = GetCookieContainer(browserProfile);

            await InitLoveIconUrlDict();

            string channelId;
            string liveId;
            try
            {
                (channelId, liveId) = GetLiveIdFromInput(input);
            }
            catch (InvalidUrlException)
            {
                SendSystemInfo("入力されたURLが正しくないようです", InfoType.Error);
                AfterDisconnected();
                return;
            }
            if (string.IsNullOrEmpty(liveId))
            {
                autoReconnectMode = true;
            }
            while (true)
            {
                if (autoReconnectMode)
                {
                    MetadataUpdated?.Invoke(this, new Metadata
                    {
                        Title = "(放送が始まるまで待機しています)",
                    });
                    try
                    {
                        liveId = await GetLiveIdFromChannelId(_server, channelId);
                    }
                    catch (LiveNotFoundException)
                    {
                        try
                        {
                            SendSystemInfo((LiveCheckIntervalMs / 1000) + "秒待ってから放送しているか確認します", InfoType.Debug);
                            await Task.Delay(LiveCheckIntervalMs, _cts.Token);
                        }
                        catch (TaskCanceledException) { break; }
                        continue;
                    }
                }
                var (liveInfo, raw) = await GetLiveInfo(channelId, liveId);
                if (liveInfo.LiveStatus == "FINISHED")
                {
                    SendSystemInfo("配信が終了しました", InfoType.Notice);
                    if (autoReconnectMode)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    SendSystemInfo("LiveStatus=" + liveInfo.LiveStatus, InfoType.Debug);
                }
                MetadataUpdated?.Invoke(this, new Metadata
                {
                    Title = liveInfo.Title,
                });

                var url = liveInfo.ChatUrl;
                _provider = CreateMessageProvider();
                _provider.Opened += Provider_Opened;
                _provider.Received += Provider_Received;

                var messageProviderTask = _provider.ReceiveAsync(url);

                var tasks = new List<Task>();
                tasks.Add(messageProviderTask);
                var promptyStatsProvider = CreatePromptyStatsProvider();
                promptyStatsProvider.Received += PromptyStatsProvider_Received;
                var promptyStatsTask = promptyStatsProvider.ReceiveAsync(channelId, liveId);
                tasks.Add(promptyStatsTask);
                var blacklistProvider = CreateBlackListProvider();
                blacklistProvider.Received += BlacklistProvider_Received;
                var blackListTask = blacklistProvider.ReceiveAsync(Tools.ExtractCookies(cc));
                tasks.Add(blackListTask);


                while (true)
                {
                    var t = await Task.WhenAny(tasks);
                    if (t == messageProviderTask)
                    {
                        //messageProviderTaskが何かしらの理由で終了したから色々終了させる。
                        promptyStatsProvider.Disconnect();
                        blacklistProvider.Disconnect();
                        await RemoveTaskFromList(messageProviderTask, tasks);
                        await RemoveTaskFromList(promptyStatsTask, tasks);
                        await RemoveTaskFromList(blackListTask, tasks);

                        break;
                    }
                    else if (t == promptyStatsTask)
                    {
                        await RemoveTaskFromList(promptyStatsTask, tasks);
                    }
                    else if (t == blackListTask)
                    {
                        await RemoveTaskFromList(blackListTask, tasks);
                    }
                }
                SendSystemInfo("websocketが切断", InfoType.Debug);
                if (_isUserDisconnected)
                {
                    break;
                }
                else
                {
                    SendSystemInfo("websocketの切断がユーザによるものではないため放送ステータスを確認", InfoType.Debug);
                }
                try
                {
                    await AfterMessageProviderDisconnected();
                }
                catch (Exception) { break; }
            }
            if (autoReconnectMode)
            {
                //タイトルが（放送が始まるまで待機しています）となっている可能性を考慮して消す
                MetadataUpdated?.Invoke(this, new Metadata
                {
                    Title = "",
                });
            }
            AfterDisconnected();
        }
        /// <summary>
        /// taskの終了を待ち、tasksから取り除く
        /// </summary>
        /// <param name="task"></param>
        /// <param name="tasks"></param>
        /// <returns></returns>
        private async Task RemoveTaskFromList(Task task, List<Task> tasks)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            tasks.Remove(task);
        }

        private IBlackListProvider CreateBlackListProvider()
        {
            return new BlackListProvider(_server);
        }

        protected virtual PromptyStatsProvider CreatePromptyStatsProvider()
        {
            return new PromptyStatsProvider(_server);
        }

        private void BlacklistProvider_Received(object sender, long[] e)
        {
            var ids = e;
            if (ids == null) return;
            SetNgUser(_oldBlackListUserIds, ids);
            _oldBlackListUserIds = ids;
        }
        /// <summary>
        /// 前回取得したブラックリストユーザID
        /// </summary>
        long[] _oldBlackListUserIds;
        protected virtual void SetNgUser(long[] old, long[] @new)
        {
            var (nochange, added, removed) = Tools.Split(old, @new);
            foreach (var id in removed)
            {
                var user = GetUser(id.ToString());
                //TODO:コメビュだけNGにしたい場合に対応できない。手動でNGに入れていたとしても解除されてしまう。
                user.IsNgUser = false;
            }
            foreach (var id in added)
            {
                var user = GetUser(id.ToString());
                user.IsNgUser = true;
            }
        }

        private void PromptyStatsProvider_Received(object sender, IPromptyStats e)
        {
            MetadataUpdated?.Invoke(this, new Metadata
            {
                IsLive = e.LiveStatus == "LIVE",
                TotalViewers = e.ViewerCount.ToString(),
            });
        }

        protected virtual Task AfterMessageProviderDisconnected()
        {
            return Task.Delay(1000);
        }
        protected virtual IMessageProvider CreateMessageProvider()
        {
            return new MessageProvider();
        }
        private void SendSystemInfo(string message, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                Text = message,
                SiteType = SiteType.LineLive,
                Type = type,
            }, _options);
            MessageReceived?.Invoke(this, context);
        }
        private string UnixTime2PostTime(long unixTime)
        {
            return Tools.FromUnixTime(unixTime).ToString("HH:mm:ss");
        }
        private LineLiveMessageContext CreateMessageContext(ParseMessage.IMessage message, ParseMessage.IUser sender, string raw, bool isInitialComment)
        {
            LineLiveMessageContext messageContext;
            if (message is ParseMessage.IMessageData comment)
            {
                var user = GetUser(sender.Id.ToString());
                var isFirstComment = _first.IsFirstComment(user.UserId);
                var m = new LineLiveComment(raw)
                {
                    Text = comment.Message,
                    IsNgMessage = comment.IsNgMessage,
                    PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(comment.SentAt),
                    UserIconUrl = sender.IconUrl,
                    UserId = sender.Id,
                    DisplayName = sender.DisplayName,
                };
                var metadata = new MessageMetadata(m, _options, _siteOptions, user, this, isFirstComment)
                {
                    IsInitialComment = isInitialComment,
                    SiteContextGuid = SiteContextGuid,
                };
                var methods = new LineLiveMessageMethods();
                messageContext = new LineLiveMessageContext(m, metadata, methods);
            }
            else if (message is ParseMessage.ILove love)
            {
                var user = GetUser(sender.Id.ToString());
                var isFirstComment = _first.IsFirstComment(user.UserId);
                var str = sender.DisplayName + "さんがハートを送りました！";
                var m = new LineLiveItem(raw)
                {
                    CommentItems = Common.MessagePartFactory.CreateMessageItems(str),
                    PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(love.SentAt),
                    UserIconUrl = sender.IconUrl,
                    UserId = sender.Id,
                    DisplayName = sender.DisplayName,
                };
                var metadata = new MessageMetadata(m, _options, _siteOptions, user, this, isFirstComment)
                {
                    IsInitialComment = isInitialComment,
                    SiteContextGuid = SiteContextGuid,
                };
                var methods = new LineLiveMessageMethods();
                messageContext = new LineLiveMessageContext(m, metadata, methods);
            }
            else if (message is ParseMessage.IFollowStartData follow)
            {
                var user = GetUser(sender.Id.ToString());
                var isFirstComment = _first.IsFirstComment(user.UserId);
                var msg = sender.DisplayName + "さんがフォローしました！";
                var m = new LineLiveItem(raw)
                {
                    CommentItems = Common.MessagePartFactory.CreateMessageItems(msg),
                    PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(follow.FollowedAt),
                    UserIconUrl = sender.IconUrl,
                    UserId = sender.Id,
                    DisplayName = sender.DisplayName,
                };
                var metadata = new MessageMetadata(m, _options, _siteOptions, user, this, isFirstComment)
                {
                    IsInitialComment = isInitialComment,
                    SiteContextGuid = SiteContextGuid,
                };
                var methods = new LineLiveMessageMethods();
                messageContext = new LineLiveMessageContext(m, metadata, methods);
            }
            else if (message is ParseMessage.IGiftMessage gift)
            {
                var user = GetUser(sender.Id.ToString());
                var isFirstComment = _first.IsFirstComment(user.UserId);
                if (_loveIconUrlDict.ContainsKey(gift.ItemId))
                {
                    gift.Url = _loveIconUrlDict[gift.ItemId];
                }
                else
                {
                    gift.Url = "";
                }
                List<IMessagePart> messageItems;
                if (gift.ItemId == "limited-love-gift" || string.IsNullOrEmpty(gift.Url))
                {
                    //{"type":"giftMessage","data":{"message":"","type":"LOVE","itemId":"limited-love-gift","quantity":1,"displayName":"limited.love.gift.item","sender":{"id":2903515,"hashedId":"715i4MKqyv","displayName":"上杉The Times","iconUrl":"https://scdn.line-apps.com/obs/0hmNs42D-0MmFOTR9H8JtNNnYQNBY3YzEpNmkpRHdEbQI3LnYxIX97UGIdaVdjKXVjd3ktVGNEP1VjenU1ew/f64x64","hashedIconId":"0hmNs42D-0MmFOTR9H8JtNNnYQNBY3YzEpNmkpRHdEbQI3LnYxIX97UGIdaVdjKXVjd3ktVGNEP1VjenU1ew","isGuest":false,"isBlocked":false},"isNGGift":false,"sentAt":1531445716,"key":"2426265.29035150000000000000","blockedByCms":false}}
                    var msg = sender.DisplayName + "さんがハートで応援ポイントを送りました！";
                    messageItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(msg) };
                }
                else
                {
                    var msg = sender.DisplayName + "さんが" + gift.Quantity + "コインプレゼントしました！";
                    messageItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(msg), new MessageImage { Url = gift.Url } };
                }

                var m = new LineLiveItem(raw)
                {
                    CommentItems = messageItems,
                    PostedAt = SitePluginCommon.Utils.UnixtimeToDateTime(gift.SentAt),
                    UserIconUrl = sender.IconUrl,
                    UserId = sender.Id,
                    DisplayName = sender.DisplayName,
                };
                var metadata = new MessageMetadata(m, _options, _siteOptions, user, this, isFirstComment)
                {
                    IsInitialComment = isInitialComment,
                    SiteContextGuid = SiteContextGuid,
                };
                var methods = new LineLiveMessageMethods();
                messageContext = new LineLiveMessageContext(m, metadata, methods);
            }
            else
            {
                messageContext = null;
            }
            return messageContext;
        }
        private void Provider_Received(object sender, string e)
        {
            try
            {
                var data = Tools.Parse(e);
                if (data.message == null)
                {
                    return;
                }
                if (data.message is ParseMessage.IBulk bulk)
                {
                    if (!_isFirstConnection)
                        return;
                    _isFirstConnection = false;
                    foreach (var (bulkItemMessage, bulkItemUser, raw) in bulk.Messages)
                    {
                        var messageContext = CreateMessageContext(bulkItemMessage, bulkItemUser, raw, true);
                        if (messageContext == null) continue;
                        MessageReceived?.Invoke(this, messageContext);
                    }
                }
                else
                {
                    var messageContext = CreateMessageContext(data.message, data.sender, "", false);
                    if (messageContext == null) return;
                    MessageReceived?.Invoke(this, messageContext);
                }
            }
            catch (ParseException ex)
            {
                _logger.LogException(ex);
                SendSystemInfo("ParseException: " + ex.Raw, InfoType.Debug);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                SendSystemInfo(ex.Message, InfoType.Debug);
            }

            Debug.WriteLine(e);
        }

        private void Provider_Opened(object sender, EventArgs e)
        {
            Debug.WriteLine("opened");
        }
        /// <summary>
        /// ユーザによる切断か
        /// </summary>
        bool _isUserDisconnected;
        CancellationTokenSource _cts;
        public void Disconnect()
        {
            _isUserDisconnected = true;
            _provider?.Disconnect();
            _cts?.Cancel();
        }

        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        public async Task PostCommentAsync(string text)
        {
            var s = $"PRIVMSG {_channelName} :{text}";
            await Task.FromResult<object>(null);
        }

        public IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.LineLive, userId);
        }

        public async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var cc = GetCookieContainer(browserProfile);
            var cookies = Tools.ExtractCookies(cc);
            var me = await Api.GetMyAsync(_server, cookies);
            var info = new CurrentUserInfo
            {
                IsLoggedIn = me != null ? !string.IsNullOrEmpty(me.UserId) : false,
                Username = me?.DisplayName,
                UserId = me?.UserId
            };
            return info;
        }

        public void SetMessage(string raw)
        {
            throw new NotImplementedException();
        }

        public Guid SiteContextGuid { get; set; }
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly ICommentOptions _options;
        private readonly LineLiveSiteOptions _siteOptions;
        private readonly IUserStoreManager _userStoreManager;

        public LineLiveCommentProvider(IDataServer server, ILogger logger, ICommentOptions options, LineLiveSiteOptions siteOptions, IUserStoreManager userStoreManager)
        {
            _server = server;
            _logger = logger;
            _options = options;
            _siteOptions = siteOptions;
            _userStoreManager = userStoreManager;
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
