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
using System.Collections.Concurrent;
using System.Linq;
using SitePluginCommon;
using System.Net.Http;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace YouTubeLiveSitePlugin
{
    public interface IInfoMessage : IMessage
    {
        InfoType Type { get; set; }
    }
    public class InfoMessage : IInfoMessage
    {
        public InfoType Type { get; set; }
        public string Raw { get; }
        public SiteType SiteType { get; set; }
        public IEnumerable<IMessagePart> NameItems { get; set; }
        public IEnumerable<IMessagePart> CommentItems { get; set; }

        public event EventHandler<ValueChangedEventArgs> ValueChanged;
    }
    public class InfoMessageMetadata : IMessageMetadata
    {
        private readonly IInfoMessage _infoMessage;
        private readonly ICommentOptions _options;

        public Color BackColor => _options.InfoBackColor;
        public Color ForeColor => _options.InfoForeColor;
        public FontFamily FontFamily => _options.FontFamily;
        public int FontSize => _options.FontSize;
        public FontWeight FontWeight => _options.FontWeight;
        public FontStyle FontStyle => _options.FontStyle;
        public bool IsNgUser => false;
        public bool IsSiteNgUser => false;
        public bool IsFirstComment => false;
        public bool IsInitialComment => false;
        public bool Is184 => false;
        public IUser User => null;
        public ICommentProvider CommentProvider => null;
        public bool IsVisible
        {
            get
            {
                return true;
            }
        }
        public bool IsNameWrapping => false;

        public event PropertyChangedEventHandler PropertyChanged;

        public InfoMessageMetadata(IInfoMessage infoMessage, ICommentOptions options)
        {
            _infoMessage = infoMessage;
            _options = options;
        }
    }
    public class InfoMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
    public class InfoMessageContext : IMessageContext
    {
        public SitePlugin.IMessage Message { get; }

        public IMessageMetadata Metadata { get; }

        public IMessageMethods Methods { get; }
        public InfoMessageContext(IInfoMessage message, InfoMessageMetadata metadata, InfoMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
    }
}
namespace YouTubeLiveSitePlugin.Test2
{
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
        public bool IsLoggedIn
        {
            get { return _connection != null ? _connection.IsLoggedIn : false; }
        }
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<IMessageContext> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        public event EventHandler<ConnectedEventArgs> Connected;

        CookieContainer _cc;
        private readonly ICommentOptions _options;
        private readonly YouTubeLiveSiteOptions _siteOptions;
        private readonly ILogger _logger;
        private readonly IUserStore _userStore;
        //ChatProvider _chatProvider;
        //IMetadataProvider _metaProvider;
        //IActiveCounter<string> _activeCounter;
        private void SendInfo(string comment, InfoType type)
        {
            CommentReceived?.Invoke(this, new SystemInfoCommentViewModel(_options, comment, type));
        }
        private void BeforeConnect()
        {
            CanConnect = false;
            CanDisconnect = true;
            _receivedCommentIds = new SynchronizedCollection<string>();
            //_userCommentCountDict = new Dictionary<string, int>();
        }
        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        private void ActiveCounter_Updated(object sender, int e)
        {
            MetadataUpdated?.Invoke(this, new Metadata { Active = e.ToString() });
        }

        private void AfterConnect()
        {
            //_chatProvider = null;
            CanConnect = true;
            CanDisconnect = false;
            _connection = null;
            SendInfo("切断しました", InfoType.Error);
        }



        protected virtual CookieContainer CreateCookieContainer(IBrowserProfile browserProfile)
        {
            var cc = new CookieContainer();//まずCookieContainerのインスタンスを作っておく。仮にCookieの取得で失敗しても/live_chatで"YSC"と"VISITOR_INFO1_LIVE"が取得できる。これらは/service_ajaxでメタデータを取得する際に必須。
            try
            {
                var cookies = browserProfile.GetCookieCollection("youtube.com");
                cc.Add(cookies);
            }
            catch { }
            return cc;
        }
        EachConnection _connection;
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            if (string.IsNullOrEmpty(input))
            {
                return;
            }
            BeforeConnect();
            MetadataUpdated?.Invoke(this, new Metadata
            {
                Active = "-",
                CurrentViewers = "-",
                Elapsed = "-",
                Title = "-",
                TotalViewers = "-",
            });
            string vid = null;
            bool isInputStoringNeeded = false;
            var resolver = new VidResolver();
            try
            {
                var result = await resolver.GetVid(_server, input);
                if (result is MultiVidsResult multi)
                {
                    SendInfo("このチャンネルでは複数のライブが配信中です。", InfoType.Notice);
                    foreach (var v in multi.Vids)
                    {
                        SendInfo(v, InfoType.Notice);//titleも欲しい
                    }
                }
                else if (result is VidResult vidResult)
                {
                    vid = vidResult.Vid;
                }
                else if (result is NoVidResult no)
                {
                    SendInfo("このチャンネルでは生放送をしていないようです", InfoType.Error);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            catch (HttpRequestException ex)
            {
                SendInfo("Http error", InfoType.Error);
                _logger.LogException(ex, "Http error", "input=" + input);
                AfterConnect();
                return;
            }
            catch (YtInitialDataNotFoundException ex)
            {
                SendInfo("ytInitialDataが存在しません", InfoType.Error);
                _logger.LogException(ex);
                AfterConnect();
                return;
            }
            catch (Exception ex)
            {
                SendInfo("入力されたURLは存在しないか無効な値です", InfoType.Error);
                _logger.LogException(ex, "Invalid input", "input=" + input);
                AfterConnect();
                return;
            }
            if (string.IsNullOrEmpty(vid))
            {
                AfterConnect();
                return;
            }
            if (resolver.IsChannel(input) || resolver.IsCustomChannel(input) || resolver.IsUser(input))
            {
                isInputStoringNeeded = true;
            }

            _cc = CreateCookieContainer(browserProfile);
            Dictionary<string, int> userCommentCountDict = new Dictionary<string, int>();
            _connection = new EachConnection(_logger, _cc, _options, _server, _siteOptions, userCommentCountDict, _receivedCommentIds, this, _userStore);
            _connection.Connected += (s, e) =>
            {
                Connected?.Invoke(this, new ConnectedEventArgs { IsInputStoringNeeded = isInputStoringNeeded });
            };
            _connection.MessageReceived += (s, e) => MessageReceived?.Invoke(s, e);
            _connection.MetadataUpdated += (s, e) => MetadataUpdated?.Invoke(s, e);
            _connection.LoggedInStateChanged += (s, e) => LoggedInStateChanged(s, e);
            var reloadManager = new ReloadManager()
            {
                CountLimit = 5,
                CountCheckTimeRangeMin = 1,
            };
        reload:
            if (!reloadManager.CanReload())
            {
                SendInfo($"{reloadManager.CountCheckTimeRangeMin}分以内に{reloadManager.CountLimit}回再接続を試みました。サーバーに余計な負荷を掛けるのを防ぐため自動再接続を中断します", InfoType.Error);
                AfterConnect();
                return;
            }
            reloadManager.SetTime();

            try
            {
                var disconnectReason = await _connection.ReceiveAsync(vid, browserProfile.Type);
                switch (disconnectReason)
                {
                    case DisconnectReason.Reload:
                        SendInfo("エラーが発生したためサーバーとの接続が切断されましたが、自動的に再接続します", InfoType.Error);
                        goto reload;
                    case DisconnectReason.ByUser:
                    case DisconnectReason.Finished:
                        //TODO:SendInfo()
                        break;
                    case DisconnectReason.ChatUnavailable:
                        //TODO:SendInfo()
                        break;
                    case DisconnectReason.YtInitialDataNotFound:
                        //TODO:SendInfo()
                        break;
                }
            }
            catch(Exception ex)
            {
                _logger.LogException(ex, "", $"input={input}");
                SendInfo("回復不能なエラーが発生しました", InfoType.Error);
            }
            AfterConnect();
        }

        protected virtual DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }


        SynchronizedCollection<string> _receivedCommentIds;

        public void Disconnect()
        {
            _connection?.Disconnect();
        }
        public IUser GetUser(string userId)
        {
            return _userStore.GetUser(userId);
        }
        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        async Task ICommentProvider.PostCommentAsync(string text)
        {
            var b = await PostCommentAsync(text);
        }
        public async Task<bool> PostCommentAsync(string text)
        {
            return await _connection.PostCommentAsync(text);
        }

        public async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var currentUserInfo = new CurrentUserInfo();
            var cc = CreateCookieContainer(browserProfile);
            var url = "https://www.youtube.com/embed";
            var html = await _server.GetAsync(url, cc);
            //"user_display_name":"Ryu"
            var match = Regex.Match(html, "\"user_display_name\":\"([^\"]+)\"");
            if (match.Success)
            {
                var name = match.Groups[1].Value;
                currentUserInfo.Username = name;
                currentUserInfo.IsLoggedIn = true;
            }
            else
            {
                currentUserInfo.IsLoggedIn = false;
            }
            return currentUserInfo;
        }

        IYouTubeLibeServer _server;
        public CommentProvider(ICommentOptions options, IYouTubeLibeServer server, YouTubeLiveSiteOptions siteOptions, ILogger logger, IUserStore userStore)
        {
            _options = options;
            _siteOptions = siteOptions;
            _logger = logger;
            _userStore = userStore;
            _server = server;

            CanConnect = true;
            CanDisconnect = false;
        }
    }
//    class CommentProvider : ICommentProvider
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
//        public event EventHandler LoggedInStateChanged;
//        private bool _isLoggedIn;
//        public bool IsLoggedIn
//        {
//            get { return _isLoggedIn; }
//            set
//            {
//                if (_isLoggedIn == value) return;
//                _isLoggedIn = value;
//                LoggedInStateChanged?.Invoke(this, EventArgs.Empty);
//            }
//        }
//        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
//        public event EventHandler<ICommentViewModel> CommentReceived;
//        public event EventHandler<IMessageContext> MessageReceived;
//        public event EventHandler<IMetadata> MetadataUpdated;
//        public event EventHandler CanConnectChanged;
//        public event EventHandler CanDisconnectChanged;
//        public event EventHandler<ConnectedEventArgs> Connected;

//        CookieContainer _cc;
//        private readonly ICommentOptions _options;
//        private readonly YouTubeLiveSiteOptions _siteOptions;
//        private readonly ILogger _logger;
//        private readonly IUserStore _userStore;
//        ChatProvider _chatProvider;
//        IMetadataProvider _metaProvider;
//        IActiveCounter<string> _activeCounter;
//        private void SendInfo(string comment, InfoType type)
//        {
//            CommentReceived?.Invoke(this, new SystemInfoCommentViewModel(_options, comment, type));
//        }
//        private void BeforeConnect()
//        {
//            CanConnect = false;
//            CanDisconnect = true;
//            _receivedCommentIds = new SynchronizedCollection<string>();
//            _userCommentCountDict = new Dictionary<string, int>();
//            _activeCounter = new ActiveCounter<string>()
//            {
//                CountIntervalSec = _options.ActiveCountIntervalSec,
//                MeasureSpanMin = _options.ActiveMeasureSpanMin,
//            };
//            _activeCounter.Updated += ActiveCounter_Updated;
//            _reloadTimeList = new List<DateTime>();
//        }
//        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
//        {
//            // Unix timestamp is seconds past epoch
//            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
//            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
//            return dtDateTime;
//        }
//        private void ActiveCounter_Updated(object sender, int e)
//        {
//            MetadataUpdated?.Invoke(this, new Metadata { Active = e.ToString() });
//        }

//        private void AfterConnect()
//        {
//            _chatProvider = null;
//            CanConnect = true;
//            CanDisconnect = false;
//            _activeCounter.Updated -= ActiveCounter_Updated;
//        }
//        Dictionary<string, int> _userCommentCountDict = new Dictionary<string, int>();
//        private YouTubeLiveCommentViewModel CreateCommentViewModel(CommentData data)
//        {
//            var userId = data.UserId;
//            bool isFirstComment;
//            if (_userCommentCountDict.ContainsKey(userId))
//            {
//                _userCommentCountDict[userId]++;
//                isFirstComment = false;
//            }
//            else
//            {
//                _userCommentCountDict.Add(userId, 1);
//                isFirstComment = true;
//            }
//            var user = _userStore.GetUser(userId);
//            var cvm = new YouTubeLiveCommentViewModel(_options,_siteOptions, data, this, isFirstComment, user);
//            return cvm;
//        }
//        private IYouTubeLiveMessage CreateMessage(CommentData data)
//        {
//            IYouTubeLiveMessage message;
//            if (data.IsPaidMessage)
//            {
//                message = new YouTubeLiveSuperchat(data);
//            }
//            else
//            {
//                message = new YouTubeLiveComment(data);
//            }

//            return message;
//        }
//        private YouTubeLiveMessageMetadata CreateMetadata(IYouTubeLiveMessage message, bool isInitialComment)
//        {
//            string userId = null;
//            if(message is IYouTubeLiveComment comment)
//            {
//                userId = comment.UserId;
//            }else if(message is IYouTubeLiveSuperchat superchat)
//            {
//                userId = superchat.UserId;
//            }
//            bool isFirstComment;
//            IUser user;
//            if (userId != null)
//            {
//                if (_userCommentCountDict.ContainsKey(userId))
//                {
//                    _userCommentCountDict[userId]++;
//                    isFirstComment = false;
//                }
//                else
//                {
//                    _userCommentCountDict.Add(userId, 1);
//                    isFirstComment = true;
//                }
//                user = _userStore.GetUser(userId);
//            }
//            else
//            {
//                isFirstComment = false;
//                user = null;
//            }
//            var metadata = new YouTubeLiveMessageMetadata(message, _options, _siteOptions, user, this, isFirstComment)
//            {
//                IsInitialComment = isInitialComment,
//            };
//            return metadata;
//        }
//        protected virtual CookieContainer CreateCookieContainer(IBrowserProfile browserProfile)
//        {
//            var cc = new CookieContainer();//まずCookieContainerのインスタンスを作っておく。仮にCookieの取得で失敗しても/live_chatで"YSC"と"VISITOR_INFO1_LIVE"が取得できる。これらは/service_ajaxでメタデータを取得する際に必須。
//            try
//            {
//                var cookies = browserProfile.GetCookieCollection("youtube.com");
//                cc.Add(cookies);
//            }
//            catch { }
//            return cc;
//        }
//        List<DateTime> _reloadTimeList;
//        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
//        {
//            if (string.IsNullOrEmpty(input))
//            {
//                return;
//            }
//            BeforeConnect();
//            MetadataUpdated?.Invoke(this, new Metadata
//            {
//                Active = "",
//                CurrentViewers = "",
//                Elapsed = "",
//                Title = "",
//                TotalViewers = "",
//            });
//            string vid = null;
//            bool isInputStoringNeeded = false;
//            var resolver = new VidResolver();
//            try
//            {
//                var result = await resolver.GetVid(_server, input);
//                if(result is MultiVidsResult multi)
//                {
//                    SendInfo("このチャンネルでは複数のライブが配信中です。", InfoType.Notice);
//                    foreach(var v in multi.Vids)
//                    {
//                        SendInfo(v, InfoType.Notice);//titleも欲しい
//                    }
//                }
//                else if(result is VidResult vidResult)
//                {
//                    vid = vidResult.Vid;
//                }
//                else if(result is NoVidResult no)
//                {
//                    SendInfo("このチャンネルでは生放送をしていないようです", InfoType.Error);
//                }
//                else
//                {
//                    throw new NotSupportedException();
//                }
//            }
//            catch (HttpException ex)
//            {
//                SendInfo("Http error", InfoType.Error);
//                _logger.LogException(ex, "Http error", "input=" + input);
//                AfterConnect();
//                return;
//            }
//            catch (YtInitialDataNotFoundException ex)
//            {
//                SendInfo("ytInitialDataが存在しません", InfoType.Error);
//                _logger.LogException(ex);
//                AfterConnect();
//                return;
//            }
//            catch (Exception ex)
//            {
//                SendInfo("入力されたURLは存在しないか無効な値です", InfoType.Error);
//                _logger.LogException(ex, "Invalid input", "input=" + input);
//                AfterConnect();
//                return;
//            }
//            if (string.IsNullOrEmpty(vid))
//            {
//                AfterConnect();
//                return;
//            }
//            if (resolver.IsChannel(input) || resolver.IsCustomChannel(input) || resolver.IsUser(input))
//            {
//                isInputStoringNeeded = true;
//            }

//            _cc = CreateCookieContainer(browserProfile);
//        reload:
//            _reloadTimeList.Add(GetCurrentDateTime());
//            const int retryLimit = 5;
//            if(_reloadTimeList.Count > retryLimit && _reloadTimeList[_reloadTimeList.Count - 1 - retryLimit] - GetCurrentDateTime()  < new TimeSpan(0, 1, 0))
//            {
//                AfterConnect();
//                return;
//            }

//            string liveChatHtml = null;

//            try
//            {
//                //live_chatを取得する。この中にこれから必要なytInitialDataとytcfgがある
//                var liveChatUrl = $"https://www.youtube.com/live_chat?v={vid}&is_popout=1";
//                liveChatHtml = await _server.GetAsync(liveChatUrl, _cc);
//            }
//            catch(HttpRequestException ex)
//            {
//                _logger.LogException(ex);
//                goto reload;
//            }
//            //catch(HttpException ex)
//            //{
//            //    var statuscode = ex.StatusCode;
//            //    switch (statuscode)
//            //    {
//            //        case 400://BadRequest
//            //            SendInfo("入力されたURLは存在しない可能性があります", InfoType.Error);
//            //            break;
//            //        case 500:
//            //            SendInfo("サーバでエラーが発生しました", InfoType.Error);
//            //            break;
//            //        case 503:
//            //            SendInfo("サーバが落ちています", InfoType.Error);
//            //            break;
//            //        default:
//            //            SendInfo($"{ex.Message}", InfoType.Error);
//            //            break;
//            //    }
//            //    AfterConnect();
//            //    return;
//            //}
//            catch(Exception ex)
//            {
//                _logger.LogException(ex);
//                SendInfo("未知のエラーが発生しました", InfoType.Error);
//                AfterConnect();
//                return;
//            }
//            if (string.IsNullOrEmpty(liveChatHtml))
//            {
//                await Task.Delay(3000);
//                goto reload;
//            }

//            //ytInitialDataを取得して、そこからcontinuationと直近の過去コメントを取得する
//            try
//            {
//                string ytInitialData = null;
//                try
//                {
//                    ytInitialData = Tools.ExtractYtInitialData(liveChatHtml);
//                }
//                catch (ParseException ex)
//                {
//                    _logger.LogException(ex, "live_chatからのytInitialDataの抜き出しに失敗", liveChatHtml);
//                }
//                if (string.IsNullOrEmpty(ytInitialData))
//                {
//                    //これが無いとコメントが取れないから終了
//                    SendInfo("ytInitialDataの取得に失敗しました", InfoType.Error);
//                    return;
//                }
//                IContinuation initialContinuation;
//                List<CommentData> initialCommentData;
//                try
//                {
//                    (initialContinuation, initialCommentData) = Tools.ParseYtInitialData(ytInitialData);
//                }
//                catch (ContinuationNotExistsException)
//                {
//                    //放送終了
//                    return;
//                }
//                catch (ParseException ex)
//                {
//                    _logger.LogException(ex, "", $"input={input}");
//                    return;
//                }
//                catch (ChatUnavailableException)
//                {
//                    SendInfo("この配信ではチャットが無効になっているようです", InfoType.Error);
//                    return;
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogException(ex, "未知の例外", $"ytInitialData={ytInitialData},input={input}");
//                    SendInfo("ytInitialDataの解析に失敗しました", InfoType.Error);
//                    return;
//                }
//                Connected?.Invoke(this, new ConnectedEventArgs { IsInputStoringNeeded = isInputStoringNeeded });

//                //直近の過去コメントを送る。ただし、再接続の場合は不要。
//                if (_reloadTimeList.Count == 1)
//                {
//                    foreach (var data in initialCommentData)
//                    {
//                        if (_receivedCommentIds.Contains(data.Id))
//                        {
//                            continue;
//                        }
//                        else
//                        {
//                            _receivedCommentIds.Add(data.Id);
//                        }
//                        var messageContext = CreateMessageContext(data, true);
//                        MessageReceived?.Invoke(this, messageContext);
//                    }
//                }

//                //コメント投稿に必要なものの準備
//                PrepareForPostingComments(liveChatHtml, ytInitialData);

//                var tasks = new List<Task>();

//                var activeCounterTask = CreateActiveCounterTask();
//                if (activeCounterTask != null)
//                {
//                    tasks.Add(activeCounterTask);
//                }

//                var chatTask = CreateChatProviderReceivingTask(vid, initialContinuation);
//                tasks.Add(chatTask);

//                var metaTask = CreateMetadataReceivingTask(browserProfile, vid, liveChatHtml);
//                if (metaTask != null)
//                {
//                    tasks.Add(metaTask);
//                }

//                while (tasks.Count > 0)
//                {
//                    var t = await Task.WhenAny(tasks);
//                    if (t == metaTask)
//                    {
//                        try
//                        {
//                            await metaTask;
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogException(ex, "metaTaskが終了した原因");
//                        }
//                        //metaTask内でParseExceptionもしくはDisconnect()
//                        //metaTaskは終わっても良い。
//                        tasks.Remove(metaTask);
//                        _metaProvider = null;
//                    }
//                    else if (t == activeCounterTask)
//                    {
//                        try
//                        {
//                            await activeCounterTask;
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogException(ex, "activeCounterTaskが終了した原因");
//                        }
//                        tasks.Remove(activeCounterTask);
//                        _activeCounter = null;
//                    }
//                    else //chatTask
//                    {
//                        tasks.Remove(chatTask);
//                        try
//                        {
//                            await chatTask;
//                        }
//                        catch (ReloadException)
//                        {
//                            //_logger.LogException(ex);
//                            goto reload;
//                        }
//                        catch (ChatUnavailableException)
//                        {
//                            SendInfo("放送が終了しているかチャットが無効な放送です", InfoType.Error);
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogException(ex);
//                        }
//                        _chatProvider = null;

//                        //chatTaskが終わったらmetaTaskも終了させる
//                        _metaProvider.Disconnect();
//                        try
//                        {
//                            await metaTask;
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogException(ex, "metaTaskが終了した原因");
//                        }
//                        tasks.Remove(metaTask);
//                        _metaProvider = null;

//                        _activeCounter?.Stop();
//                        try
//                        {
//                            await activeCounterTask;
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogException(ex, "activeCounterTaskが終了した原因");
//                        }
//                        tasks.Remove(activeCounterTask);
//                        _activeCounter = null;
//                    }
//                }
//            }
//            finally
//            {
//                AfterConnect();
//            }
//        }

//        protected virtual DateTime GetCurrentDateTime()
//        {
//            return DateTime.Now;
//        }

//        private void PrepareForPostingComments(string liveChatHtml, string ytInitialData)
//        {
//            var liveChatContext = Tools.GetLiveChatContext(liveChatHtml);
//            IsLoggedIn = liveChatContext.IsLoggedIn;
//            if (Tools.TryExtractSendButtonServiceEndpoint(ytInitialData, out string serviceEndPoint))
//            {
//                try
//                {
//                    var json = DynamicJson.Parse(serviceEndPoint);
//                    PostCommentContext = new PostCommentContext
//                    {
//                        ClientIdPrefix = json.sendLiveChatMessageEndpoint.clientIdPrefix,
//                        SessionToken = liveChatContext.XsrfToken,
//                        Sej = serviceEndPoint,
//                    };
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogException(ex, "", $"serviceEndPoint={serviceEndPoint}");
//                }
//            }
//        }

//        private Task CreateActiveCounterTask()
//        {
//            Task activeCounterTask = null;
//            if (_options.IsActiveCountEnabled)
//            {
//                activeCounterTask = _activeCounter.Start();
//            }
//            return activeCounterTask;
//        }

//        private Task CreateChatProviderReceivingTask(string vid, IContinuation initialContinuation)
//        {
//            _chatProvider = new ChatProvider(_server, _logger);
//            _chatProvider.ActionsReceived += ChatProvider_ActionsReceived;
//            _chatProvider.InfoReceived += ChatProvider_InfoReceived;
//            var chatTask = _chatProvider.ReceiveAsync(vid, initialContinuation, _cc);
//            return chatTask;
//        }

//        private Task CreateMetadataReceivingTask(IBrowserProfile browserProfile, string vid, string liveChatHtml)
//        {
//            Task metaTask = null;
//            string ytCfg = null;
//            try
//            {
//                ytCfg = Tools.ExtractYtcfg(liveChatHtml);
//            }
//            catch (ParseException ex)
//            {
//                _logger.LogException(ex, "live_chatからのytcfgの抜き出しに失敗", liveChatHtml);
//            }
//            if (!string.IsNullOrEmpty(ytCfg))
//            {
//                //"service_ajax?name=updatedMetadataEndpoint"はIEには対応していないらしく、400が返って来てしまう。
//                //そこで、IEの場合のみ旧版の"youtubei"を使うようにした。
//                if (browserProfile.Type == BrowserType.IE)
//                {
//                    _metaProvider = new MetaDataYoutubeiProvider(_server, _logger);
//                }
//                else
//                {
//                    _metaProvider = new MetadataProvider(_server, _logger);
//                }
//                _metaProvider.MetadataReceived += MetaProvider_MetadataReceived;
//                //_metaProvider.Noticed += _metaProvider_Noticed;
//                _metaProvider.InfoReceived += MetaProvider_MetadataReceived;
//                metaTask = _metaProvider.ReceiveAsync(ytCfg: ytCfg, vid: vid, cc: _cc);
//            }

//            return metaTask;
//        }

//        /// <summary>
//        /// 文字列から@ニックネームを抽出する
//        /// 文字列中に@が複数ある場合は一番最後のものを採用する
//        /// 数字だけのニックネームは不可
//        /// </summary>
//        /// <param name="text"></param>
//        /// <returns></returns>
//        protected string ExtractNickname(string text)
//        {
//            if (string.IsNullOrEmpty(text))
//                return null;
//            var matches = Regex.Matches(text, "(?:@|＠)(\\S+)", RegexOptions.Singleline);
//            if (matches.Count > 0)
//            {
//                foreach (Match match in matches.Cast<Match>().Reverse())
//                {
//                    var val = match.Groups[1].Value;
//                    if(!Regex.IsMatch(val, "^[0-9０１２３４５６７８９]+$"))
//                    {
//                        return val;
//                    }
//                }
//            }
//            return null;
//        }
//        private void ChatProvider_InfoReceived(object sender, InfoData e)
//        {
//            SendInfo(e.Comment, e.Type);
//        }

//        private void MetaProvider_MetadataReceived(object sender, InfoData e)
//        {
//            SendInfo(e.Comment, e.Type);
//        }

//        private void MetaProvider_MetadataReceived(object sender, IMetadata e)
//        {
//            MetadataUpdated?.Invoke(this, e);
//        }
//        SynchronizedCollection<string> _receivedCommentIds;
//        private void ChatProvider_ActionsReceived(object sender, List<CommentData> e)
//        {
//            foreach (var action in e)
//            {
//                if (_receivedCommentIds.Contains(action.Id))
//                {
//                    continue;
//                }
//                else
//                {
//                    _activeCounter.Add(action.Id);
//                    _receivedCommentIds.Add(action.Id);
//                }

//                var messageContext = CreateMessageContext(action, false);
//                MessageReceived?.Invoke(this, messageContext);
//            }
//        }
//        private YouTubeLiveMessageContext CreateMessageContext(CommentData commentData, bool isInitialComment)
//        {
//            var message = CreateMessage(commentData);
//            var metadata = CreateMetadata(message, isInitialComment);
//            var methods = new YouTubeLiveMessageMethods();
//            if (_siteOptions.IsAutoSetNickname)
//            {
//                var user = metadata.User;
//                var messageText = Common.MessagePartsTools.ToText(message.CommentItems);
//                var nick = ExtractNickname(messageText);
//                if (!string.IsNullOrEmpty(nick))
//                {
//                    user.Nickname = nick;
//                }
//            }
//            return new YouTubeLiveMessageContext(message, metadata, methods);
//        }
//        public void Disconnect()
//        {
//            _chatProvider?.Disconnect();
//        }
//        public IUser GetUser(string userId)
//        {
//            return _userStore.GetUser(userId);
//        }
//        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
//        {
//            throw new NotImplementedException();
//        }
//        bool CanPostComment => PostCommentContext != null;
//        protected virtual PostCommentContext PostCommentContext { get; set; }
//        int _commentPostCount;
//        async Task ICommentProvider.PostCommentAsync(string text)
//        {
//            var b = await PostCommentAsync(text);
//        }
//        public async Task<bool> PostCommentAsync(string text)
//        {
//            var ret = false;
//            if (CanPostComment)
//            {
//                try
//                {
//                    var clientMessageId = PostCommentContext.ClientIdPrefix + _commentPostCount;
//                    var s = "{\"text_segments\":[{\"text\":\"" + text + "\"}]}";
//                    var sej = PostCommentContext.Sej.Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
//                    var sessionToken = PostCommentContext.SessionToken;
//                    var data = new Dictionary<string, string>
//                    {
//                        { "client_message_id",clientMessageId},
//                        { "rich_message",s},
//                        { "sej",sej},
//                        { "session_token",sessionToken},
//                    };
//                    var url = "https://www.youtube.com/service_ajax?name=sendLiveChatMessageEndpoint";
//                    var res = await _server.PostAsync(url, data, _cc);
//                    Debug.WriteLine(res);
//                    var json = DynamicJson.Parse(res);
//                    //if (json.IsDefined("errors"))
//                    //{
//                    //    var k = string.Join("&", data.Select(kv => kv.Key + "=" + kv.Value));
//                    //    throw new PostingCommentFailedException("コメント投稿に失敗しました（" + json.errors[0] + "）",$"data={k}, res={res}");
//                    //}
//                    if (json.IsDefined("code") && json.code == "SUCCESS")
//                    {
//                        if (json.IsDefined("data") && json.data.IsDefined("errorMessage"))
//                        {
//                            var k = string.Join("&", data.Select(kv => kv.Key + "=" + kv.Value));
//                            var errorText = json.data.errorMessage.liveChatTextActionsErrorMessageRenderer.errorText.simpleText;
//                            throw new PostingCommentFailedException("コメント投稿に失敗しました（" + errorText + "）", $"data={k}, res={res}");
//                        }
//                        else if(json.IsDefined("data") && json.data.IsDefined("actions"))
//                        {
//                            //多分成功
//                            _commentPostCount++;
//                            ret = true;
//                            goto CommentPostsucceeded;
//                        }
//                    }
//                    var k0 = string.Join("&", data.Select(kv => kv.Key + "=" + kv.Value));
//                    throw new UnknownResponseReceivedException($"data={k0}, res={res}");
//                }
//                catch (UnknownResponseReceivedException ex)
//                {
//                    _logger.LogException(ex);
//                    SendInfo(ex.Message, InfoType.Error);
//                }
//                catch (PostingCommentFailedException ex)
//                {
//                    _logger.LogException(ex);
//                    SendInfo(ex.Message, InfoType.Error);
//                }
//                //catch (HttpException ex)
//                //{
//                //    //{\"errors\":[\"検証中にエラーが発生しました。\"]} statuscodeは分からないけど200以外
//                //    _logger.LogException(ex, "", $"text={text},statuscode={ex.StatusCode}");
//                //    SendInfo("コメント投稿時にエラーが発生", InfoType.Error);
//                //}
//                catch (HttpRequestException ex)
//                {
//                    if(ex.InnerException is WebException webEx)
//                    {
//                        string response;
//                        using (var sr = new System.IO.StreamReader(webEx.Response.GetResponseStream()))
//                        {
//                            response = sr.ReadToEnd();
//                        }
//                        var statuscode = (int)((HttpWebResponse)webEx.Response).StatusCode;
//                        //{\"errors\":[\"検証中にエラーが発生しました。\"]} statuscodeは分からないけど200以外
//                        _logger.LogException(ex, "", $"text={text},statuscode={statuscode}");
//                        SendInfo("コメント投稿時にエラーが発生", InfoType.Error);
//                    }
//                    else
//                    {
//                        _logger.LogException(ex, "", $"text={text}");
//                        SendInfo("コメント投稿時にエラーが発生", InfoType.Error);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogException(ex);
//                    SendInfo("コメント投稿時にエラーが発生", InfoType.Error);
//                }
//            }
//CommentPostsucceeded:
//            return ret;
//        }

//        public async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
//        {
//            var currentUserInfo = new CurrentUserInfo();
//            var cc = CreateCookieContainer(browserProfile);
//            var url = "https://www.youtube.com/embed";
//            var html = await _server.GetAsync(url, cc);
//            //"user_display_name":"Ryu"
//            var match = Regex.Match(html, "\"user_display_name\":\"([^\"]+)\"");
//            if (match.Success)
//            {
//                var name = match.Groups[1].Value;
//                currentUserInfo.Username = name;
//                currentUserInfo.IsLoggedIn = true;
//            }
//            else
//            {
//                currentUserInfo.IsLoggedIn = false;
//            }
//            return currentUserInfo;
//        }

//        IYouTubeLibeServer _server;
//        public CommentProvider(ICommentOptions options, IYouTubeLibeServer server, YouTubeLiveSiteOptions siteOptions, ILogger logger, IUserStore userStore)
//        {
//            _options = options;
//            _siteOptions = siteOptions;
//            _logger = logger;
//            _userStore = userStore;
//            _server = server;

//            CanConnect = true;
//            CanDisconnect = false;
//        }
//    }
    class PostCommentContext
    {
        public string SessionToken { get; set; }
        public string Sej { get; set; }
        public string ClientIdPrefix { get; set; }
    }
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
