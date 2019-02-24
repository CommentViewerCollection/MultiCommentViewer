using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
namespace LineLiveSitePlugin
{
    public class LineLiveServer : IDataServer
    {
        public async Task<string> GetAsync(string url, CookieContainer cc)
        {
            using (var handler = new HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new HttpClient(handler))
            {
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> GetAsync(string url, string userAgent, CookieContainer cc)
        {
            using (var handler = new HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> GetAsync(string url, Dictionary<string, string> headers, CookieContainer cc)
        {
            using (var handler = new HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new HttpClient(handler))
            {
                if (headers != null)
                {
                    foreach (var kv in headers)
                    {
                        client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                    }
                }
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> GetAsync(string url)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetStringAsync(url);
                return result;
            }
        }
        public async Task<string> PostAsync(string url, Dictionary<string, string> data, CookieContainer cc)
        {
            var content = new FormUrlEncodedContent(data);
            using (var handler = new HttpClientHandler { UseCookies = true, CookieContainer = cc })
            using (var client = new HttpClient(handler))
            {
                var result = await client.PostAsync(url, content);
                var resBody = await result.Content.ReadAsStringAsync();
                return resBody;
            }
        }
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
