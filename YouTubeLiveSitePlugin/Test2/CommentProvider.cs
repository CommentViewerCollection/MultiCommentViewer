using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using SitePlugin;
using ryu_s.BrowserCookie;
using Common;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Codeplex.Data;
using System.Web;
using System.Windows.Media;
using System.Windows;
using System.Linq;

namespace YouTubeLiveSitePlugin.Test2
{
    internal class YouTubeLiveSiteOptions : DynamicOptionsBase
    {
        public Color PaidCommentBackColor { get => GetValue(); set => SetValue(value); }
        public Color PaidCommentForeColor { get => GetValue(); set => SetValue(value); }
        protected override void Init()
        {
            Dict.Add(nameof(PaidCommentBackColor), new Item { DefaultValue = ColorFromArgb("#FFFF0000"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
            Dict.Add(nameof(PaidCommentForeColor), new Item { DefaultValue = ColorFromArgb("#FFFFFFFF"), Predicate = c => true, Serializer = c => ColorToArgb(c), Deserializer = s => ColorFromArgb(s) });
        }
        internal YouTubeLiveSiteOptions Clone()
        {
            return (YouTubeLiveSiteOptions)this.MemberwiseClone();
        }
        internal void Set(YouTubeLiveSiteOptions changedOptions)
        {
            foreach (var src in changedOptions.Dict)
            {
                var v = src.Value;
                SetValue(v.Value, src.Key);
            }
        }
        #region Converters
        private FontFamily FontFamilyFromString(string str)
        {
            return new FontFamily(str);
        }
        private string FontFamilyToString(FontFamily family)
        {
            return family.FamilyNames.Values.First();
        }
        private FontStyle FontStyleFromString(string str)
        {
            return (FontStyle)new FontStyleConverter().ConvertFromString(str);
        }
        private string FontStyleToString(FontStyle style)
        {
            return new FontStyleConverter().ConvertToString(style);
        }
        private FontWeight FontWeightFromString(string str)
        {
            return (FontWeight)new FontWeightConverter().ConvertFromString(str);
        }
        private string FontWeightToString(FontWeight weight)
        {
            return new FontWeightConverter().ConvertToString(weight);
        }
        private Color ColorFromArgb(string argb)
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
        private string ColorToArgb(Color color)
        {
            var argb = color.ToString();
            return argb;
        }
        #endregion
    }
    class CommentProvider : ICommentProvider
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
        public event EventHandler LoggedInStateChanged;
        private bool _IsLoggedIn;
        public bool IsLoggedIn
        {
            get { return _IsLoggedIn; }
            set
            {
                if (_IsLoggedIn == value) return;
                _IsLoggedIn = value;
                LoggedInStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;

        CookieContainer _cc;
        private readonly ConnectionName _connectionName;
        private readonly IOptions _options;
        private readonly YouTubeLiveSiteOptions _siteOptions;
        private readonly ILogger _logger;
        ChatProvider chatProvider;
        MetadataProvider _metaProvider;

        private void SendInfo(string message)
        {
            CommentReceived?.Invoke(this, new InfoCommentViewModel(_connectionName, _options, message));
        }
        private void BeforeConnect()
        {
            CanConnect = false;
            CanDisconnect = true;
        }
        private void AfterConnect()
        {
            chatProvider = null;
            CanConnect = true;
            CanDisconnect = false;
        }
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            BeforeConnect();
            string vid = null;
            var retryCount = 0;
            var resolver = new VidResolver();
            try
            {
                var result = await resolver.GetVid(_server, input);
                if(result is MultiVidsResult multi)
                {
                    SendInfo("このチャンネルでは複数のライブが配信中です。");
                    foreach(var v in multi.Vids)
                    {
                        SendInfo(v);//titleも欲しい
                    }
                }
                else if(result is VidResult vidResult)
                {
                    vid = vidResult.Vid;
                }
                else if(result is NoVidResult no)
                {
                    SendInfo("");
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            catch (Exception ex)
            {
                CommentReceived?.Invoke(this, new InfoCommentViewModel(_connectionName, _options, "入力されたURLは存在しないか無効な値です"));
                _logger.LogException(ex, "Invalid input", "input=" + input);
                AfterConnect();
                return;
            }
            if (string.IsNullOrEmpty(vid))
            {
                AfterConnect();
                return;
            }

            try
            {
                var cookies = browserProfile.GetCookieCollection("youtube.com");
                _cc = new CookieContainer();
                _cc.Add(cookies);
            }
            catch { }
reload:

            string liveChatHtml = null;

            try
            {
                //live_chatを取得する。この中にこれから必要なytInitialDataとytcfgがある
                using (var wc = new MyWebClient(_cc))
                {
                    wc.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.2924.87 Safari/537.36";
                    var liveChatUrl = $"https://www.youtube.com/live_chat?v={vid}&is_popout=1";
                    var bytes = await wc.DownloadDataTaskAsync(liveChatUrl);
                    liveChatHtml = Encoding.UTF8.GetString(bytes);
                }
            }
            catch(WebException ex) when(ex.Status == WebExceptionStatus.ProtocolError)
            {
                if(ex.Response is HttpWebResponse http)
                {
                    switch (http.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            SendInfo("入力されたURLは存在しない可能性があります");
                            break;
                        case HttpStatusCode.InternalServerError:
                            SendInfo("サーバでエラーが発生しました");
                            break;
                        case HttpStatusCode.ServiceUnavailable:
                            SendInfo("サーバが落ちています");
                            break;
                    }
                    //http.StatusCode
                }
                AfterConnect();
                return;
            }
            catch(Exception ex)
            {
                _logger.LogException(ex);
            }
            if (string.IsNullOrEmpty(liveChatHtml))
            {
                await Task.Delay(3000);
                goto reload;
            }
            try
            {


                var tasks = new List<Task>();

                string ytInitialData = null;
                try
                {
                    ytInitialData = Tools.ExtractYtInitialData(liveChatHtml);
                }
                catch (ParseException ex)
                {
                    _logger.LogException(ex, "live_chatからのytInitialDataの抜き出しに失敗", liveChatHtml);
                }
                if (string.IsNullOrEmpty(ytInitialData))
                {
                    //これが無いとコメントが取れないから終了
                    CommentReceived?.Invoke(this, new InfoCommentViewModel(_connectionName, _options, "ytInitialDataの取得に失敗しました"));
                    return;
                }
                var (initialContinuation, initialCommentData) = Tools.ParseYtInitialData(ytInitialData);
                var initialComments = new List<ICommentViewModel>();
                foreach(var data in initialCommentData)
                {
                    var cvm = new YouTubeLiveCommentViewModel(_connectionName, _options, data, this);
                    initialComments.Add(cvm);
                }
                if(initialComments.Count > 0)
                    InitialCommentsReceived?.Invoke(this, initialComments);
                initialComments = null;

                //コメント投稿に必要なものの準備
                var liveChatContext = Tools.GetLiveChatContext(liveChatHtml);
                IsLoggedIn = liveChatContext.IsLoggedIn;
                if(Tools.TryExtractSendButtonServiceEndpoint(ytInitialData, out string serviceEndPoint))
                {
                    var json = DynamicJson.Parse(serviceEndPoint);
                    _postCommentContext = new PostCommentContext
                    {
                        ClientIdPrefix = json.sendLiveChatMessageEndpoint.clientIdPrefix,
                        SessionToken = liveChatContext.XsrfToken,
                        Sej = serviceEndPoint,
                    };
                }

                Task chatTask = null;
                Task metaTask = null;

                chatProvider = new ChatProvider(_logger);
                chatProvider.InitialActionsReceived += ChatProvider_InitialActionsReceived;
                chatProvider.ActionsReceived += ChatProvider_ActionsReceived;
                chatProvider.SessionTokenUpdated += (s, e) =>
                {
                    var token = e;
                    if(_postCommentContext != null)
                    {
                        _postCommentContext.SessionToken = token;
                    }
                };
                chatTask = chatProvider.ReceiveAsync(vid, initialContinuation, _cc);
                tasks.Add(chatTask);

                string ytCfg = null;
                try
                {
                    ytCfg = Tools.ExtractYtcfg(liveChatHtml);
                }
                catch (ParseException ex)
                {
                    _logger.LogException(ex, "live_chatからのytcfgの抜き出しに失敗", liveChatHtml);
                }
                if (!string.IsNullOrEmpty(ytCfg))
                {
                    _metaProvider = new MetadataProvider(_logger);
                    _metaProvider.MetadataReceived += MetaProvider_MetadataReceived;
                    metaTask = _metaProvider.ReceiveAsync(ytCfg: ytCfg, vid: vid, cc: _cc);
                    tasks.Add(metaTask);
                }
                var t = await Task.WhenAny(tasks);
                if(t == metaTask)
                {
                    //metaTask内でParseExceptionもしくはDisconnect()
                    //metaTaskは終わっても良い。
                    await chatTask;
                }
                else
                {
                    //chatTaskが終わったらmetaTaskも終了させる
                    _metaProvider.Disconnect();
                    await metaTask;
                }
            }
            catch (ReloadException)
            {
                retryCount++;
                goto reload;
            }
            catch(ChatUnavailableException)
            {
                SendInfo("放送が終了しているかチャットが無効な放送です");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            finally
            {
                AfterConnect();
            }
        }

        private void MetaProvider_MetadataReceived(object sender, IMetadata e)
        {
            MetadataUpdated?.Invoke(this, e);
        }

        private void ChatProvider_ActionsReceived(object sender, List<CommentData> e)
        {
            foreach (var action in e)
            {
                var cvm = new YouTubeLiveCommentViewModel(_connectionName, _options, action, this);
                CommentReceived?.Invoke(this, cvm);
            }
        }

        private void ChatProvider_InitialActionsReceived(object sender, List<CommentData> e)
        {
            var list = new List<ICommentViewModel>();
            foreach(var commentData in e)
            {
                var cvm = new YouTubeLiveCommentViewModel(_connectionName, _options, commentData, this);
                list.Add(cvm);
            }
            InitialCommentsReceived?.Invoke(this, list);
        }

        public void Disconnect()
        {
            chatProvider?.Disconnect();
        }

        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }
        bool CanPostComment => _postCommentContext != null;
        PostCommentContext _postCommentContext;
        int _commentPostCount;
        public async Task PostCommentAsync(string text)
        {
            if (CanPostComment)
            {
                try
                {
                    var clientMessageId = _postCommentContext.ClientIdPrefix + _commentPostCount;
                    var s = "{\"text_segments\":[{\"text\":\"" + text + "\"}]}";
                    var sej = _postCommentContext.Sej.Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
                    var sessionToken = _postCommentContext.SessionToken;
                    var data = $"client_message_id={UrlEncode(clientMessageId)}&rich_message={UrlEncode(s)}&sej={UrlEncode(sej)}&session_token={UrlEncode(sessionToken)}";
                    var url = "https://www.youtube.com/service_ajax?name=sendLiveChatMessageEndpoint";
                    var resBytes = await _server.PostAsync(url, data, _cc);
                    _commentPostCount++;

                }
                catch(WebException ex)
                {
                    var res = ex.Response as HttpWebResponse;
                    using (var sr = new System.IO.StreamReader(res.GetResponseStream()))
                    {
                        var s = sr.ReadToEnd();
                        Debug.WriteLine(s);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            }
        }
        private string UrlEncode(string s)
        {
            var lower = HttpUtility.UrlEncode(s);
            Regex reg = new Regex(@"%[a-f0-9]{2}");
            string upper = reg.Replace(lower, m => m.Value.ToUpperInvariant());
            return upper;
        }
        IYouTubeLibeServer _server;
        public CommentProvider(ConnectionName connectionName, IOptions options, YouTubeLiveSiteOptions siteOptions, ILogger logger)
        {
            _connectionName = connectionName;
            _options = options;
            _siteOptions = siteOptions;
            _logger = logger;
            _server = new YouTubeLiveServer();

            CanConnect = true;
            CanDisconnect = false;
        }
    }
    class PostCommentContext
    {
        public string SessionToken { get; set; }
        public string Sej { get; set; }
        public string ClientIdPrefix { get; set; }
    }
    internal interface IYouTubeLibeServer
    {
        Task<string> GetAsync(string url);
        Task<string> GetEnAsync(string url);
        Task<string> PostAsync(string url, string data, CookieContainer cc);
    }
    internal class YouTubeLiveServer : IYouTubeLibeServer
    {
        public async Task<string> GetAsync(string url)
        {
            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
            wc.Headers["origin"] = "https://www.youtube.com";
            var bytes = await wc.DownloadDataTaskAsync(url);
            return Encoding.UTF8.GetString(bytes);
        }

        public async Task<string> GetEnAsync(string url)
        {
            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
            wc.Headers["origin"] = "https://www.youtube.com";
            wc.Headers[HttpRequestHeader.AcceptLanguage] = "en-US,en;q=0.5";
            var bytes = await wc.DownloadDataTaskAsync(url);
            return Encoding.UTF8.GetString(bytes);
        }

        public async Task<string> PostAsync(string url, string data, CookieContainer cc)
        {
            var wc = new MyWebClient(cc);
            wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:59.0) Gecko/20100101 Firefox/59.0";
            wc.Headers["origin"] = "https://www.youtube.com";
            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            wc.Headers[HttpRequestHeader.Accept] = "*/*";
            var payload = Encoding.UTF8.GetBytes(data);
            var bytes = await wc.UploadDataTaskAsync(url, payload);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
