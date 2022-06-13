using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
namespace LineLiveSitePlugin
{
    public interface IPromptyStats
    {
        long LoveCount { get; }
        long FreeLoveCount { get; }
        long PremiumLoveCount { get; }
        long LimitedLoveCount { get; }
        long OwnedLimitedLoveCount { get; }
        long SentLimitedLoveCount { get; }
        long ViewerCount { get; }
        long ChatCount { get; }
        //"LIVE"
        string LiveStatus { get; }
        //LiveStartedAtは常にnull
        int ApiStatusCode { get; }
        object PinnedMessage { get; }
        int Status { get; }
    }
    internal class PromptyStats : IPromptyStats
    {
        public long LoveCount { get; set; }
        public long FreeLoveCount { get; set; }
        public long PremiumLoveCount { get; set; }
        public long LimitedLoveCount { get; set; }
        public long OwnedLimitedLoveCount { get; set; }
        public long SentLimitedLoveCount { get; set; }
        public long ViewerCount { get; set; }
        public long ChatCount { get; set; }
        public string LiveStatus { get; set; }
        public int ApiStatusCode { get; set; }
        public object PinnedMessage { get; set; }
        public int Status { get; set; }
        public PromptyStats(Low.PromptyStats.RootObject low)
        {
            LoveCount = low.LoveCount;
            FreeLoveCount = low.FreeLoveCount;
            PremiumLoveCount = low.PremiumLoveCount;
            LimitedLoveCount = low.LimitedLoveCount;
            OwnedLimitedLoveCount = low.OwnedLimitedLoveCount;
            SentLimitedLoveCount = low.SentLimitedLoveCount;
            ViewerCount = low.ViewerCount;
            ChatCount = low.ChatCount.HasValue ? low.ChatCount.Value : 0;
            LiveStatus = low.LiveStatus;
            ApiStatusCode = low.ApistatusCode;
            PinnedMessage = low.PinnedMessage;
            Status = low.Status;
        }
    }
    internal static class Api
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="channelId"></param>
        /// <param name="liveId"></param>
        /// <exception cref="ParseException"></exception>
        /// <returns></returns>
        public static async Task<IPromptyStats> GetPromptyStats(IDataServer server, string channelId, string liveId)
        {
            var url = $"https://live-burst-api.line-apps.com/burst/app/channel/{channelId}/broadcast/{liveId}/promptly_stats";
            var res = await server.GetAsync(url);
            var low = Tools.Deserialize<Low.PromptyStats.RootObject>(res);
            return new PromptyStats(low);
        }
        public static async Task<IPromptyStats> GetPromptyStatsV4(IDataServer server, string channelId, string liveId)
        {
            var url = $"https://live-burst-api.line-apps.com/burst/web/v4.0/channel/{channelId}/broadcast/{liveId}/promptly_stats";
            var res = await server.GetAsync(url);
            var low = Tools.Deserialize<Low.PromptyStats.RootObject>(res);
            return new PromptyStats(low);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        public static async Task<long[]> GetBlockList(IDataServer server, List<Cookie> cookies)
        {
            string accessToken = null;
            foreach (var cookie in cookies)
            {
                if (cookie.Name == "linelive")
                {
                    accessToken = cookie.Value;
                }
            }
            if (string.IsNullOrEmpty(accessToken))
            {
                //未ログイン。どうせ情報を取得できないから諦める。
                return null;
            }
            var url = "https://live-api.line-apps.com/app/setting/blocklist/bulk";
            var headers = new Dictionary<string, string>()
            {
                {"X-CastService-WebClient-AccessToken", accessToken },
            };
            var cc = Tools.CreateCookieContainer(cookies);
            var res = await server.GetAsync(url, headers, cc);
            //{"blockedUserIds":[1983766],"apistatusCode":200,"status":200}
            //{"blockedUserIds":[316787,1983766],"apistatusCode":200,"status":200}
            var d = Codeplex.Data.DynamicJson.Parse(res);
            if (d.status == 200)
            {
                return (long[])d.blockedUserIds;
            }
            else
            {
                return null;
            }
        }
        public static async Task<(ILiveInfo, string raw)> GetLiveInfo(IDataServer server, string channelId, string liveId)
        {
            var url = $"https://live-api.line-apps.com/app/v2/channel/{channelId}/broadcast/{liveId}";
            var s = await server.GetAsync(url);
            var liveInfoLow = Tools.Deserialize<LineLiveSitePlugin.Low.LiveInfo.RootObject>(s);
            var liveInfo = Tools.Parse(liveInfoLow);
            return (liveInfo, s);
        }
        public static async Task<(ILiveInfo, string raw)> GetLiveInfoV4(IDataServer server, string channelId, string liveId)//, CookieContainer cc)
        {
            var url = $"https://live-api.line-apps.com/web/v4.0/channel/{channelId}/broadcast/{liveId}";
            var res = await server.GetAsync(url);//, cc);
            //{"status":4030005,"errorMessage":"LIVE Gateway Failed","apistatusCode":4030005}
            dynamic d = Tools.Deserialize(res);
            var liveInfo = new LiveInfo
            {
                ChatUrl = (string)d.chat.url,
                LiveStatus = (string)d.item.liveStatus,
                Title = (string)d.item.title,
            };
            return (liveInfo, res);
        }
        public static async Task<(LineLiveSitePlugin.Low.ChannelInfo.RootObject, string raw)> GetChannelInfo(IDataServer server, string channelId)
        {
            var url = $"https://live-api.line-apps.com/app/channel/{channelId}";
            var s = await server.GetAsync(url);
            var channelInfo = Tools.Deserialize<LineLiveSitePlugin.Low.ChannelInfo.RootObject>(s);
            return (channelInfo, s);
        }
        public static async Task<LineLive.Api.Loves> GetLovesV4(IDataServer server)
        {
            //https://live-api.line-apps.com/web/v4.0/billing/gift/loves?storeType=WEB&channelId={channelId}&broadcastId={broadcastId}
            var url = "https://live-api.line-apps.com/web/v4.0/billing/gift/loves";
            var headers = new Dictionary<string, string>
            {
                {"Accept-Language","ja" },
            };
            var s = await server.GetAsync(url,headers,new CookieContainer());
            var loves = LineLive.Api.Loves.Parse(s);
            return loves;
        }
        public static async Task<LineLiveSitePlugin.Low.Loves.RootObject> GetLoves(IDataServer server)
        {
            var url = "https://live-api.line-apps.com/web/v2.5/billing/gift/loves";
            var s = await server.GetAsync(url);
            var loves = Tools.Deserialize<LineLiveSitePlugin.Low.Loves.RootObject>(s);
            return loves;
        }
        public static async Task<Me> GetMyAsync(IDataServer server, List<Cookie> cookies)
        {
            //Cookie: _ga=GA1.2.1887758210.1492703493; _trmccid=8791bd77daaaeab8; _ldbrbid=tr_dc24b04cfc4630fac6c9301351e483618c2d164d9e7449be37fb5890502714b5; ldsuid=y2iOYFvTNEm4X4S1GYs6Ag==; _trmcuser={"id":""}; _ga=GA1.3.1887758210.1492703493; linelive=c7cc59e8f126353cb23192827520afe5f25e8a893efb791d06d70b69a6f70e15; _trmcdisabled2=-1; _trmcsession={"id":"3f8174e641d2b20b","path":"/","query":"","params":{},"time":1541067304256}; _gid=GA1.3.1706626909.1541067304; __try__=1541067317211; _gat=1
            string accessToken = null;
            foreach (var cookie in cookies)
            {
                if (cookie.Name == "linelive")
                {
                    accessToken = cookie.Value;
                }
            }
            if (string.IsNullOrEmpty(accessToken))
            {
                //未ログイン。どうせ情報を取得できないから諦める。
                return null;
            }
            var url = "https://live-api.line-apps.com/app/my";
            var headers = new Dictionary<string, string>()
            {
                {"X-CastService-WebClient-AccessToken", accessToken },
            };
            var cc = Tools.CreateCookieContainer(cookies);
            var res = await server.GetAsync(url, headers, cc);
            var low = JsonConvert.DeserializeObject<Low.My.RootObject>(res);
            var me = new Me
            {
                DisplayName = low.User.DisplayName,
                UserId = low.User.Id.ToString(),
            };
            return me;
        }
    }
    class Me
    {
        public string DisplayName { get; set; }
        public string UserId { get; set; }
    }
}

//namespace LineLiveSitePlugin
//{    /// <summary>
//     /// 
//     /// </summary>
//     /// <remarks>接続毎にインスタンスを作る</remarks>
//    class MessageProvider
//    {
//        public event EventHandler Opened;

//        public event EventHandler<Result> Received;
//        WebSocket _ws;
//        TaskCompletionSource<object> _tcs;
//        public Task ReceiveAsync()
//        {
//            _tcs = new TaskCompletionSource<object>();
//            var cookies = new List<KeyValuePair<string, string>>();
//            _ws = new WebSocket("wss://irc-ws.chat.twitch.tv/", "", cookies);
//            _ws.MessageReceived += _ws_MessageReceived;
//            _ws.Opened += _ws_Opened;
//            _ws.Error += _ws_Error;
//            _ws.Closed += _ws_Closed;
//            _ws.Open();
//            return _tcs.Task;
//        }

//        private void _ws_Closed(object sender, EventArgs e)
//        {
//            _tcs.SetResult(null);
//        }

//        private void _ws_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
//        {
//            _tcs.SetException(e.Exception);
//        }

//        private void _ws_Opened(object sender, EventArgs e)
//        {
//            Opened?.Invoke(this, e);
//        }

//        public async Task SendAsync(string s)
//        {
//            Debug.WriteLine("send: " + s);
//            await Task.Yield();
//            _ws.Send(s + "\r\n");
//        }

//        private void _ws_MessageReceived(object sender, MessageReceivedEventArgs e)
//        {
//            var arr = e.Message.Split(new[] { "\r\n" }, StringSplitOptions.None);
//            foreach (var message in arr)
//            {
//                if (string.IsNullOrEmpty(message))
//                    continue;
//                var result = Tools.Parse(message);
//                Received?.Invoke(this, result);
//            }
//        }

//        public void Disconnect()
//        {
//            _ws?.Close();
//            _ws = null;
//        }
//        public MessageProvider()
//        {

//        }
//    }
//    class LineLiveCommentProvider : ICommentProvider
//    {
//        private bool _canConnect;
//        public bool CanConnect
//        {
//            get { return _canConnect; }
//            set
//            {
//                if (_canConnect == value)
//                    return;
//                _canConnect = value;
//                CanConnectChanged?.Invoke(this, EventArgs.Empty);
//            }
//        }

//        private bool _canDisconnect;
//        public bool CanDisconnect
//        {
//            get { return _canDisconnect; }
//            set
//            {
//                if (_canDisconnect == value)
//                    return;
//                _canDisconnect = value;
//                CanDisconnectChanged?.Invoke(this, EventArgs.Empty);
//            }
//        }
//        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
//        public event EventHandler<ICommentViewModel> CommentReceived;
//        public event EventHandler<IMetadata> MetadataUpdated;
//        public event EventHandler CanConnectChanged;
//        public event EventHandler CanDisconnectChanged;

//        private string GetChannelName(string input)
//        {
//            return Tools.GetChannelName(input);
//        }
//        private string _channelName;
//        private MessageProvider _provider;
//        //private CookieContainer _cc;
//        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
//        {
//            CanConnect = false;
//            CanDisconnect = true;
//            try
//            {
//                _channelName = GetChannelName(input);
//                var cc = new CookieContainer();

//                try
//                {
//                    var cookies = browserProfile.GetCookieCollection("twitch.tv");
//                    cc.Add(cookies);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogException(ex);
//                }
//                _me = null;
//                try
//                {
//                    _me = await API.GetMeAsync(_server, cc);
//                }
//                catch (NotLoggedInException) { }
//                catch (Exception ex)
//                {
//                    _logger.LogException(ex);
//                }
//                if (_me != null)
//                {
//                    _emotIcons = await API.GetEmotIcons(_server, _me.Id, cc);
//                }


//                _provider = new MessageProvider();
//                _provider.Opened += Provider_Opened;
//                _provider.Received += Provider_Received;
//                await _provider.ReceiveAsync();
//            }
//            finally
//            {
//                CanConnect = true;
//                CanDisconnect = false;
//            }
//        }
//        IMe _me;
//        LowObject.Emoticons _emotIcons;
//        private async void Provider_Received(object sender, Result e)
//        {
//            var result = e;
//            switch (result.Command)
//            {
//                case "PING":
//                    await _provider.SendAsync("PONG");
//                    break;
//                case "GLOBALUSERSTATE":
//                    break;
//                case "PRIVMSG":
//                    {
//                        var commentData = new CommentData(result);
//                        var user = _userStore.GetUser(commentData.UserId);
//                        if (!_userCommentDict.TryGetValue(user, out ObservableCollection<LineLiveCommentViewModel> userComments))
//                        {
//                            userComments = new ObservableCollection<LineLiveCommentViewModel>();
//                            _userCommentDict.Add(user, userComments);
//                        }
//                        var isFirstComment = userComments.Count == 0;
//                        var cvm = new LineLiveCommentViewModel(_options, _siteOptions, commentData, _emotIcons, isFirstComment, this, user);
//                        await _dispatcher.BeginInvoke((Action)(() =>
//                        {
//                            userComments.Add(cvm);
//                        }));
//                        CommentReceived?.Invoke(this, cvm);
//                    }
//                    break;
//                default:
//                    Debug.WriteLine($"LineLive unknown command={result.Command}");
//                    var info = new InfoCommentViewModel(_options, result.Raw, InfoType.Debug);
//                    CommentReceived?.Invoke(this, info);
//                    break;
//            }
//        }

//        private async void Provider_Opened(object sender, EventArgs e)
//        {
//            try
//            {
//                if (IsLoggedIn())
//                {
//                    await _provider.SendAsync("CAP REQ :twitch.tv/tags twitch.tv/commands");
//                    await _provider.SendAsync($"PASS oauth:{_me.ChatOauthToken}");
//                    await _provider.SendAsync($"NICK {_me.Name}");
//                    await _provider.SendAsync($"USER {_me.Name} 8 * :{_me.Name}");
//                }
//                else
//                {
//                    var name = Tools.GetRandomGuestUsername();
//                    await _provider.SendAsync("CAP REQ :twitch.tv/tags twitch.tv/commands");
//                    await _provider.SendAsync($"PASS SCHMOOPIIE");
//                    await _provider.SendAsync($"NICK {name}");
//                    await _provider.SendAsync($"USER {name} 8 * :{name}");
//                }
//                await _provider.SendAsync($"JOIN " + _channelName);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogException(ex);
//                CommentReceived?.Invoke(this, new InfoCommentViewModel(_options, ""));
//            }
//        }
//        private bool IsLoggedIn()
//        {
//            return _me != null;
//        }

//        public void Disconnect()
//        {
//            _provider.Disconnect();
//        }

//        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
//        {
//            var comments = _userCommentDict[user];
//            return comments;
//        }

//        public async Task PostCommentAsync(string text)
//        {
//            var s = $"PRIVMSG {_channelName} :{text}";
//            await Task.FromResult<object>(null);
//        }
//        private readonly Dictionary<IUser, ObservableCollection<LineLiveCommentViewModel>> _userCommentDict = new Dictionary<IUser, ObservableCollection<LineLiveCommentViewModel>>();
//        private readonly IDataServer _server;
//        private readonly ILogger _logger;
//        private readonly ICommentOptions _options;
//        private readonly LineLiveSiteOptions _siteOptions;
//        private readonly IUserStore _userStore;
//        private readonly Dispatcher _dispatcher;
//        public LineLiveCommentProvider(IDataServer server, ILogger logger, ICommentOptions options, LineLiveSiteOptions siteOptions, IUserStore userStore, Dispatcher dispacher)
//        {
//            _server = server;
//            _logger = logger;
//            _options = options;
//            _siteOptions = siteOptions;
//            _userStore = userStore;
//            _dispatcher = dispacher;

//            CanConnect = true;
//            CanDisconnect = false;
//        }
//    }
//    public class LineLiveSiteContext : ISiteContext
//    {
//        public Guid Guid => new Guid("22F7824A-EA1B-411E-85CA-6C9E6BE94E39");

//        public string DisplayName => "LineLive";

//        public IOptionsTabPage TabPanel
//        {
//            get
//            {
//                var panel = new TabPagePanel();
//                panel.SetViewModel(new LineLiveSiteOptionsViewModel(_siteOptions));
//                return new LineLiveOptionsTabPage(DisplayName, panel);
//            }
//        }

//        public ICommentProvider CreateCommentProvider()
//        {
//            return new LineLiveCommentProvider(new LineLiveServer(), _logger, _options, _siteOptions, _userStore, _dispatcher);
//        }
//        private LineLiveSiteOptions _siteOptions;
//        public void LoadOptions(string path, IIo io)
//        {
//            _siteOptions = new LineLiveSiteOptions();
//            try
//            {
//                var s = io.ReadFile(path);

//                _siteOptions.Deserialize(s);
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex.Message);
//                _logger.LogException(ex, "", $"path={path}");
//            }
//        }

//        public void SaveOptions(string path, IIo io)
//        {
//            try
//            {
//                var s = _siteOptions.Serialize();
//                io.WriteFile(path, s);
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine(ex.Message);
//                _logger.LogException(ex, "", $"path={path}");
//            }
//        }
//        public bool IsValidInput(string input)
//        {
//            //チャンネル名だけ来られても他のサイトのものの可能性があるからfalse
//            //"twitch.tv/"の後に文字列があったらtrueとする。
//            var b = Regex.IsMatch(input, "twitch\\.tv/[a-zA-Z0-9_]+");
//            return b;
//        }

//        public UserControl GetCommentPostPanel(ICommentProvider commentProvider)
//        {
//            return null;
//        }

//        private readonly ICommentOptions _options;
//        private readonly ILogger _logger;
//        private readonly IUserStore _userStore;
//        private readonly Dispatcher _dispatcher;
//        public LineLiveSiteContext(ICommentOptions options, ILogger logger, IUserStore userStore, Dispatcher dispatcher)
//        {
//            _options = options;
//            _logger = logger;
//            _userStore = userStore;
//            _dispatcher = dispatcher;
//        }
//    }
//}
