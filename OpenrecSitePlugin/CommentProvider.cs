using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SitePlugin;
using ryu_s.BrowserCookie;
using Common;
using System.Threading;
using System.Net;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenrecSitePlugin
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
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
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
        private string GetLiveId(string input)
        {
            if (Tools.IsValidUrl(input))
            {
                return Tools.ExtractLiveId(input);
            }
            else if (Tools.IsValidMovieId(input))
            {
                return input;
            }
            else
            {
                throw new InvalidInputException();
            }
        }
        private void BeforeConnecting()
        {
            CanConnect = false;
            CanDisconnect = true;
        }
        private void AfterDisconnected()
        {
            _500msTimer.Enabled = false;
            _ws = null;
            CanConnect = true;
            CanDisconnect = false;
        }
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            BeforeConnecting();
            if (_ws != null)
            {
                throw new InvalidOperationException("");
            }
            try
            {
                var cookies = browserProfile.GetCookieCollection("openrec.tv");
                _cc = new CookieContainer();
                _cc.Add(cookies);
            }
            catch { }
            string liveId;
            try
            {
                liveId = GetLiveId(input);
            }
            catch (InvalidInputException ex)
            {
                _logger.LogException(ex, "無効な入力値", $"input={input}");
                SendInfo("無効な入力値です");
                AfterDisconnected();
                return;
            }

            var livePageUrl = "https://www.openrec.tv/live/" + liveId;
            var livePageHtml = await _dataSource.GetAsync(livePageUrl, _cc);
            var liveContext = Tools.ParseLivePageHtml(livePageHtml);
            var movieId = liveContext.MovieId;
            if (movieId == "0")
            {
                SendInfo("存在しないURLまたはIDです");
                AfterDisconnected();
                return;
            }
            if(liveContext.OnairStatus == "2")
            {
                SendInfo("この放送は終了しています");
                AfterDisconnected();
                return;
            }
            MetadataUpdated?.Invoke(this, new Metadata { Title = liveContext.Title });
            
            if(DateTime.TryParse(liveContext.StartAt, out DateTime startAt))
            {
                _startAt = startAt;
            }
            else
            {
                _logger.LogException(new ParseException(livePageHtml));
            }
            _500msTimer.Enabled = true;
            
            var _context = Tools.GetContext(_cc);
            var chatList = await API.GetChatList(_dataSource, movieId, _context);
            var initialComments = chatList.Select(item =>
            {
                var commentData = Tools.CreateCommentData(item, _startAt, _siteOptions);
                var userId = commentData.UserId;
                var user = _userStore.GetUser(userId);
                ICommentViewModel cvm = CreateCommentViewModel(commentData);
                return cvm;
            }).ToList();
            InitialCommentsReceived?.Invoke(this, initialComments);

            _ws = new OpenrecWebsocket(_logger);
            _ws.Received += WebSocket_Received;
            
            var userAgent = GetUserAgent(browserProfile.Type);
            var wsTask = _ws.ReceiveAsync(movieId, userAgent, _cc);

            var blackListProvider = new BlackListProvider(_dataSource, _logger);
            blackListProvider.Received += BlackListProvider_Received;
            var blackTask = blackListProvider.ReceiveAsync(movieId, _context);

            var tasks = new List<Task>
            {
                wsTask,
                blackTask
            };

            while (true)
            {
                var t = await Task.WhenAny(tasks);
                if (t == blackTask)
                {
                    try
                    {
                        await blackTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    tasks.Remove(blackTask);
                }
                else
                {
                    blackListProvider.Disconnect();
                    try
                    {
                        await blackTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    try
                    {
                        await wsTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    break;
                }
            }
            AfterDisconnected();
        }

        private void BlackListProvider_Received(object sender, List<string> e)
        {
            try
            {
                var blackList = e;
                //現状BAN状態のユーザ
                var banned = _userViewModelDict.Where(kv => kv.Value.IsNgUser).Select(kv => kv.Key).ToList();

                //ブラックリストに登録されているユーザのBANフラグをONにする
                foreach (var black in blackList)
                {
                    if (_userViewModelDict.ContainsKey(black))
                    {
                        var user = _userViewModelDict[black];
                        user.IsNgUser = true;
                    }
                    //ブラックリストに入っていることが確認できたためリストから外す
                    banned.Remove(black);
                }

                //ブラックリストに入っていなかったユーザのBANフラグをOFFにする
                foreach (var white in banned)
                {
                    _userViewModelDict[white].IsNgUser = false;
                }
            }catch(Exception ex)
            {
                _logger.LogException(ex);
            }
        }

        OpenrecWebsocket _ws;

        public void Disconnect()
        {
            if(_ws != null)
            {
                _ws.Disconnect();
            }
        }
        #endregion //ICommentProvider


        #region Fields
        private ICommentOptions _options;
        private OpenrecSiteOptions _siteOptions;
        private ILogger _logger;
        private IUserStore _userStore;
        private readonly IDataSource _dataSource;
        private CookieContainer _cc;
        #endregion //Fields

        #region ctors
        System.Timers.Timer _500msTimer = new System.Timers.Timer();
        public CommentProvider(ICommentOptions options, OpenrecSiteOptions siteOptions,ILogger logger, IUserStore userStore)
        {
            _options = options;
            _siteOptions = siteOptions;
            _logger = logger;
            _userStore = userStore;
            _dataSource = new DataSource();
            _500msTimer.Interval = 500;
            _500msTimer.Elapsed += _500msTimer_Elapsed;
            _500msTimer.AutoReset = true;

            CanConnect = true;
            CanDisconnect = false;
        }

        private void _500msTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var elapsed = DateTime.Now - _startAt;
            MetadataUpdated?.Invoke(this, new Metadata
            {
                Elapsed = Tools.ElapsedToString(elapsed),
            });
        }
        #endregion //ctors

        #region Events
        #endregion //Events
        private void SendInfo(string message)
        {
            CommentReceived?.Invoke(this, new InfoCommentViewModel(_options, message));
        }
        Dictionary<string, int> _userCommentCountDict = new Dictionary<string, int>();
        Dictionary<string, UserViewModel> _userViewModelDict = new Dictionary<string, UserViewModel>();
        DateTime _startAt;
        private OpenrecCommentViewModel CreateCommentViewModel(IOpenrecCommentData data)
        {
            var userId = data.UserId;
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
            var user = _userStore.GetUser(userId);
            if (!_userViewModelDict.TryGetValue(userId, out UserViewModel userVm))
            {
                userVm = new UserViewModel(user);
                _userViewModelDict.Add(userId, userVm);
            }
            var cvm = new OpenrecCommentViewModel(data, _options,userVm, this, isFirstComment);
            return cvm;
        }
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
        
        private void WebSocket_Received(object sender, IPacket e)
        {
            try
            {
                if (e is PacketMessageEventMessageChat chat)
                {
                    var commentData = Tools.CreateCommentData(chat.Comment, _startAt, _siteOptions);
                    var cvm = CreateCommentViewModel(commentData);
                    CommentReceived?.Invoke(this, cvm);
                }
                else if (e is PacketMessageEventMessageAudienceCount audienceCount)
                {
                    var ac = audienceCount.AudienceCount;
                    MetadataUpdated?.Invoke(this, new Metadata
                    {
                        CurrentViewers = ac.live_viewers.ToString(),
                        TotalViewers = ac.viewers.ToString(),
                    });
                }
                else if (e is PacketMessageEventMessageLiveEnd liveEnd)
                {
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        public async Task PostCommentAsync(string str)
        {

        }
    }
    public class MovieContext
    {
        /// <summary>
        /// 
        /// </summary>
        public string MovieId { get; internal set; }
        //"0":予約？
        //"1":放送中
        //"2":放送終了
        public string OnairStatus { get; internal set; }

        public string UserId { get; internal set; }
        public string OpenrecUserId { get; internal set; }
        public string MovieUserId { get; internal set; }
        public string MovieOpenrecUserId { get; internal set; }
        public string IsLiveArchive { get; internal set; }
        public string DefaultIconUrl { get; internal set; }
        public string Uri { get; internal set; }
        public MovieUserInfo MovieUserInfo { get; internal set; }
        public string Title { get; internal set; }
        public string StartAt { get; internal set; }
    }
    public class MovieUserInfo
    {
        public string recxuser_id { get; set; }
        public string terminal_id { get; set; }
        public string nickname { get; set; }
        public string icon_file { get; set; }
        public string bg_file { get; set; }
        public string introduce { get; set; }
        public object introduce_dt { get; set; }
        public object fb_id { get; set; }
        public object fb_token { get; set; }
        public object tw_id { get; set; }
        public object tw_token { get; set; }
        public object tw_secret { get; set; }
        public object os_version { get; set; }
        public string user_token { get; set; }
        public string del_flg { get; set; }
        public string user_type { get; set; }
        public string openrec_user_id { get; set; }
        public string tv_connect_flg { get; set; }
        public string identify_id { get; set; }
        public string approve_flg { get; set; }
        public object game_level { get; set; }
        public string user_agent { get; set; }
        public string sdk_user_id { get; set; }
        public string cools_count { get; set; }
        public string movie_count { get; set; }
        public string tag { get; set; }
    }
    class MessageImagePortion : IMessageImagePortion
    {
        public int SrcX { get; set; }

        public int SrcY { get; set; }

        public int SrcWidth { get; set; }

        public int SrcHeight { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public Image Image { get; set; }
        public string Alt { get; set; }
    }
}
