using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows.Media;
using Codeplex.Data;
using LineLiveSitePlugin.Low.LiveInfo;

namespace LineLiveSitePlugin.ParseMessage
{
    internal interface IMessage
    {
    }
    internal interface IUser
    {
        long Id { get; }
        string DisplayName { get; }
        string IconUrl { get; }
    }
    class User : IUser
    {
        public long Id { get; }
        public string DisplayName { get; }
        public string IconUrl { get; }
        public User(long id, string displayName, string iconUrl)
        {
            Id = id;
            DisplayName = displayName;
            IconUrl = iconUrl;
        }
    }
    internal interface IMessageData : IMessage
    {
        string Message { get; }
        long SentAt { get; }
        bool IsNgMessage { get; }
        string RoomId { get; }
    }
    internal interface IFollowStartData : IMessage
    {
        long FollowedAt { get; }
        string RoomId { get; }
    }

    internal interface ILove : IMessage
    {
        string RoomId { get; }
        long Quantity { get; }
        long SentAt { get; }
    }

    internal interface IBulk : IMessage
    {
        List<(IMessage, IUser, string)> Messages { get; }
    }

    internal interface IGiftMessage : IMessage
    {
        string ItemId { get; }
        string Url { get; set; }
        long Quantity { get; }
        long SentAt { get; set; }
    }
}
namespace LineLiveSitePlugin
{
    internal static class Tools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="old"></param>
        /// <param name="new"></param>
        /// <returns></returns>
        public static (IList<T> nochange, IList<T> added, IList<T> removed) Split<T>(IList<T> old, IList<T> @new)
        {
            var nochange = new List<T>();
            var added = new List<T>();
            var removed = new List<T>();
            if (old == null && @new == null) return (nochange, added, removed);
            if (old == null && @new != null) return (nochange, @new, removed);
            if (old != null && @new == null) return (nochange, added, old);

            //@newが配列の場合@new.Remove()がNotSupportExceptionを投げてしまう。
            var newList = new List<T>(@new);
            foreach (var oldItem in old)
            {
                if (newList.Contains(oldItem))
                {
                    nochange.Add(oldItem);
                    newList.Remove(oldItem);
                }
                else
                {
                    removed.Add(oldItem);
                }
            }
            added.AddRange(@new);
            return (nochange, added, removed);
        }
        public static List<Cookie> ExtractCookies(CookieContainer container)
        {
            var cookies = new List<Cookie>();

            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable",
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.GetField |
                                                                    BindingFlags.Instance,
                                                                    null,
                                                                    container,
                                                                    new object[] { });

            foreach (var key in table.Keys)
            {
                var domain = key as string;

                if (domain == null)
                    continue;

                if (domain.StartsWith("."))
                    domain = domain.Substring(1);

                var address = string.Format("http://{0}/", domain);

                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out Uri uri) == false)
                    continue;

                foreach (Cookie cookie in container.GetCookies(uri))
                {
                    cookies.Add(cookie);
                }
            }

            return cookies;
        }
        public static DateTime FromUnixTime(long unix)
        {
            var baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var date = baseDate.AddSeconds(unix);
            return date.ToLocalTime();
        }
        public static Color ColorFromArgb(string argb)
        {
            if (argb == null)
                throw new ArgumentNullException("argb");
            var pattern = "#(?<a>[0-9a-fA-F]{2})(?<r>[0-9a-fA-F]{2})(?<g>[0-9a-fA-F]{2})(?<b>[0-9a-fA-F]{2})";
            var match = System.Text.RegularExpressions.Regex.Match(argb, pattern, System.Text.RegularExpressions.RegexOptions.Compiled);

            if (!match.Success)
            {
                throw new ArgumentException("形式が不正");
            }
            else
            {
                var a = byte.Parse(match.Groups["a"].Value, System.Globalization.NumberStyles.HexNumber);
                var r = byte.Parse(match.Groups["r"].Value, System.Globalization.NumberStyles.HexNumber);
                var g = byte.Parse(match.Groups["g"].Value, System.Globalization.NumberStyles.HexNumber);
                var b = byte.Parse(match.Groups["b"].Value, System.Globalization.NumberStyles.HexNumber);
                return Color.FromArgb(a, r, g, b);
            }
        }

        internal static CookieContainer CreateCookieContainer(List<Cookie> cookies)
        {
            var cc = new CookieContainer();
            foreach (var cookie in cookies)
            {
                try
                {
                    cc.Add(cookie);
                }
                catch (Exception) { }
            }
            return cc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <exception cref="ParseException"></exception>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            T low;
            try
            {
                low = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                throw new ParseException(json, ex);
            }
            return low;
        }
        public static dynamic Deserialize(string json)
        {
            dynamic low;
            try
            {
                low = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            }
            catch (Exception ex)
            {
                throw new ParseException(json, ex);
            }
            return low;
        }
        public static (ParseMessage.IMessage message, ParseMessage.IUser sender) Parse(string s)
        {
            var json = DynamicJson.Parse(s);
            var type = json.type;
            ParseMessage.IMessage ret;
            ParseMessage.IUser user;
            switch (type)
            {
                case "message":
                    {
                        var k = Newtonsoft.Json.JsonConvert.DeserializeObject<Low.Message.Message>(s);
                        ret = k.Data;
                        var sender = k.Data.Sender;
                        user = new ParseMessage.User(sender.Id, sender.DisplayName, sender.IconUrl);
                    }
                    break;
                case "followStart":
                    {
                        var k = Newtonsoft.Json.JsonConvert.DeserializeObject<Low.FollowStart.RootObject>(s);
                        ret = k.Data;
                        var sender = k.Data.User;
                        user = new ParseMessage.User(sender.Id, sender.DisplayName, sender.IconUrl);
                    }
                    break;
                case "love":
                    {
                        var k = Newtonsoft.Json.JsonConvert.DeserializeObject<Low.Love.RootObject>(s);
                        ret = k.Data;
                        var sender = k.Data.Sender;
                        user = new ParseMessage.User(sender.Id, sender.DisplayName, sender.IconUrl);
                    }
                    break;
                case "bulk":
                    {
                        var k = Newtonsoft.Json.JsonConvert.DeserializeObject<Low.Bulk.RootObject>(s);
                        var data = k.Data;
                        var list = new List<(ParseMessage.IMessage, ParseMessage.IUser, string raw)>();
                        foreach (var message in data.Payloads)
                        {
                            try
                            {
                                var msg = message.ToString();
                                var (a, b) = Parse(msg);
                                if (a == null && b == null)
                                {
                                    continue;
                                }
                                list.Add((a, b, msg));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                        ret = new BulkImple { Messages = list };
                        user = null;
                    }
                    break;
                case "giftMessage":
                    {
                        var k = Newtonsoft.Json.JsonConvert.DeserializeObject<Low.GiftMessage.RootObject>(s);
                        ret = k.Data;
                        var sender = k.Data.Sender;
                        user = new ParseMessage.User(sender.Id, sender.DisplayName, sender.IconUrl);
                    }
                    break;
                case "systemMessage":
                    {
                        var k = Newtonsoft.Json.JsonConvert.DeserializeObject<Low.SystemMessage.RootObject>(s);
                        //ret = k.Data;
                        //k.Data.ExtraData.Userは自分のアカウント情報
                        Debug.WriteLine(s);
                        ret = null;
                        user = null;
                    }
                    break;
                case "messageTemplate":
                    //{"type":"messageTemplate","data":{"templates":[{"message":"Good night!"},{"message":"XOXO"},{"message":"OK"},{"message":"Dawww"},{"message":"First time watcher!"}]}}
                    ret = null;
                    user = null;
                    break;
                case "guideMessage":
                    //{"type":"guideMessage","data":{"message":"Ask 🌱🐣✨ なう。✨🐥🍀 whats the plan for dinner!"}}
                    ret = null;
                    user = null;
                    break;
                case "ownerMessage":
                    //{"type":"ownerMessage","data":{"message":"こんちわ！","sentAt":1552022995}}
                    //配信者コメントだと思うけど、useridとかiconurlが無い。どうにかして取らないといけない。
                    ret = null;
                    user = null;
                    break;
                default:
                    //未処理のメッセージ
                    //{"type":"gift","data":{"message":"頑張ってねーおやすみなさい*˙︶˙*)ﾉ\"","type":"LOVE","itemId":"gire014","quantity":1000,"displayName":"live.gift.regular.1000.diamondring","sender":{"id":3748341,"hashedId":"8QhCcxAEZq","displayName":"㊙️☆🐸中国のババア🐸","iconUrl":"https://scdn.line-apps.com/obs/0hIGPVfl6AFmhtLzubUYxpP1VyEB8UARUgFQsNTVQnTV8QHgQ-UBxbWR18S14SHFc4WU9RCkB6TVoQHgY5Bg/f64x64","hashedIconId":"0hIGPVfl6AFmhtLzubUYxpP1VyEB8UARUgFQsNTVQnTV8QHgQ-UBxbWR18S14SHFc4WU9RCkB6TVoQHgY5Bg","isGuest":false,"isBlocked":false},"isNGGift":false,"sentAt":1540570986,"key":"9176638.37483410000000000000","blockedByCms":false}}
                    //{"type":"socialShare","data":{"user":{"id":7277010,"hashedId":"srjYmTaWmm","displayName":"🍻🎤けんちゃん🐕🎧","iconUrl":"https://scdn.line-apps.com/obs/0hhHlCtZ3aN2dROhrTaP9IMGlnMRAoFDQvKR4sQmg4YAJ5WnQ2Pgh8BXM9aAcuXXU1aFwrVXIyblZ8DHZkPQ/f64x64","hashedIconId":"0hhHlCtZ3aN2dROhrTaP9IMGlnMRAoFDQvKR4sQmg4YAJ5WnQ2Pgh8BXM9aAcuXXU1aFwrVXIyblZ8DHZkPQ","isGuest":false,"isBlocked":false},"roomId":"HgZBttxyiWwrPwnJBkSrEzSLUXRyEGFQ","social":["TWITTER"],"sharedAt":1540571009}}
                    //{"type":"gift","data":{"message":"","type":"LOVE","itemId":"gire013","quantity":500,"displayName":"live.gift.regular.500.teddybear","sender":{"id":1680337,"hashedId":"v1TUegGS47","displayName":"てる♪v(*'-^*)","iconUrl":"https://scdn.line-apps.com/obs/0h-FsVCGNBcmlOL1_BPjgNPnZydB43AXEhNgtpTHd4fgtqTGA9Ikw4XDh_Ll1nTGE3ckE-B24seFw0GjU5cg/f64x64","hashedIconId":"0h-FsVCGNBcmlOL1_BPjgNPnZydB43AXEhNgtpTHd4fgtqTGA9Ikw4XDh_Ll1nTGE3ckE-B24seFw0GjU5cg","isGuest":false,"isBlocked":false},"isNGGift":false,"sentAt":1540571059,"key":"9250484.16803370000000000000","blockedByCms":false}}
                    //{"type":"screenCapture","data":{"user":{"id":255268,"hashedId":"JjydSbhv6B","displayName":"miho♡","iconUrl":"https://scdn.line-apps.com/obs/0hdyXthD1eO1ZlDRb7kRtEAV1QPSEcIzgeHSkgc1xfYmZIbi9QCzlwYxNeZzFJOnsGWj98OUhaYmVPP38JXg/f64x64","hashedIconId":"0hdyXthD1eO1ZlDRb7kRtEAV1QPSEcIzgeHSkgc1xfYmZIbi9QCzlwYxNeZzFJOnsGWj98OUhaYmVPP38JXg","isGuest":false,"isBlocked":false},"roomId":"HgZBttxyiWwrPwnJBkSrEzSLUXRyEGFQ","capturedAt":1540571090}}
                    //{"type":"gift","data":{"message":"","type":"LOVE","itemId":"gire017","quantity":4,"displayName":"live.gift.regular.4.lolipop.pink","sender":{"id":4648097,"hashedId":"L73yEnK2rc","displayName":"のぶ(のふくろ)","iconUrl":"https://scdn.line-apps.com/obs/0hOD2s-hohEGsPCj3fFbNvPDdXFhx2JBMjdy4LTjYJTgkgblFoNmVcDC4JS1MrP1I9Y2lcWn8LT1IlbQM7Ng/f64x64","hashedIconId":"0hOD2s-hohEGsPCj3fFbNvPDdXFhx2JBMjdy4LTjYJTgkgblFoNmVcDC4JS1MrP1I9Y2lcWn8LT1IlbQM7Ng","isGuest":false,"isBlocked":false},"isNGGift":false,"sentAt":1540571480,"key":"9670807.46480970000000000000","blockedByCms":false}}
                    //{"type":"gift","data":{"message":"","type":"LOVE","itemId":"gire005","quantity":10,"displayName":"live.gift.regular.10.luckycat.pink","sender":{"id":7676942,"hashedId":"pkR3HxsStU","displayName":"❤みく(呼び捨て)＆U_key64🔑❤","iconUrl":"https://scdn.line-apps.com/obs/0hzJ_o7EaTJWViFgh5zT5aMlpLIxIbOCYtGjI-QFsXflxJdWU1WnRtC0URKVBOcmA6DnBuUBVDewFIIGMwWw/f64x64","hashedIconId":"0hzJ_o7EaTJWViFgh5zT5aMlpLIxIbOCYtGjI-QFsXflxJdWU1WnRtC0URKVBOcmA6DnBuUBVDewFIIGMwWw","isGuest":false,"isBlocked":false},"isNGGift":false,"sentAt":1542729632,"key":"5306470.76769420000000000000","blockedByCms":false}}
                    //{"type":"entranceAnnouncement","data":{"userId":6637658,"displayName":"🐚👾☕あをと☁🥂"}}
                    //{"type":"entranceAnnouncement","data":{"userId":21896268,"userHashedId":"49PFP7Terh","displayName":"ヨッシー","welcomeInfo":null}}
                    //{"type":"pokeReceive","data":{"userId":20538054,"channelIconUrl":"https://obs.line-scdn.net/0hLmUehq0XE0JoHAd5MlVsFVBBFTURMhAKEDgIZ1EYRXRNLlQSBy5fIh4ZS3JBKlYWBi9cIBpMS3AXe1UWVQ/f231x231","userIconUrl":"https://obs.line-scdn.net/0hGdDO2Wa-GEt5NgwQQzBnHEFrHjwAGBsDARIDbkAxQy9UAA1KEVQCLAk1Rn5dUl0eEVgELFwyTnNWAwpOEg/f231x231","greetingMessage":"遊びに来てくれてありがとう！","sentAt":1655142705}}
                    throw new ParseException(s);
            }
            return (ret, user);
        }

        internal static ILiveInfo Parse(RootObject liveInfoLow)
        {
            var liveInfo = new LiveInfo
            {
                ChatUrl = liveInfoLow.Chat?.Url,
                LiveStatus = liveInfoLow.Item.LiveStatus,
                Title = liveInfoLow.Item.Title,
            };
            return liveInfo;
        }
    }
    internal class BulkImple : ParseMessage.IBulk
    {
        public List<(ParseMessage.IMessage, ParseMessage.IUser, string)> Messages { get; set; }
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
