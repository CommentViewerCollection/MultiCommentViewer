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
namespace YouTubeLiveSitePlugin.Test2
{
    internal class YouTubeLiveSiteOptions : DynamicOptionsBase
    {
        protected override void Init()
        {
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
        public string GetVid(string url)
        {
            if(Regex.IsMatch(url, "^[^/?=:]+$"))
            {
                return url;
            }
            var match = Regex.Match(url, "youtube\\.com/watch?v=([^?=/]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            throw new Exception("");
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
            string vid;
            var retryCount = 0;
            try
            {
                vid = GetVid(input);
            }
            catch (Exception)
            {
                AfterConnect();
                throw;
            }
            try
            {
                var cookies = browserProfile.GetCookieCollection("youtube.com");
                _cc = new CookieContainer();
                _cc.Add(cookies);
            }
            catch { }
reload:
            try
            {
                //live_chatを取得する。この中にこれから必要なytInitialDataとytcfgがある
                var wc = new MyWebClient(_cc);
                wc.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.2924.87 Safari/537.36";
                var liveChatUrl = $"https://www.youtube.com/live_chat?v={vid}&is_popout=1";
                var bytes = await wc.DownloadDataTaskAsync(liveChatUrl);
                var liveChatHtml = Encoding.UTF8.GetString(bytes);

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
                chatProvider = new ChatProvider(_logger);
                chatProvider.InitialActionsReceived += ChatProvider_InitialActionsReceived;
                chatProvider.ActionsReceived += ChatProvider_ActionsReceived;
                var t = chatProvider.ReceiveAsync(vid, initialContinuation, _cc);


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
                    _metaProvider = new MetadataProvider();
                    _metaProvider.MetadataReceived += MetaProvider_MetadataReceived;
                    var metaTask = _metaProvider.ReceiveAsync(ytCfg: ytCfg, vid: vid, cc: _cc);
                    tasks.Add(metaTask);
                }
                await Task.WhenAll(tasks);
            }
            catch (ReloadException)
            {
                retryCount++;
                goto reload;
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

        public Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }
        public CommentProvider(ConnectionName connectionName, IOptions options, YouTubeLiveSiteOptions siteOptions, ILogger logger)
        {
            _connectionName = connectionName;
            _options = options;
            _siteOptions = siteOptions;
            _logger = logger;

            CanConnect = true;
            CanDisconnect = false;
        }
    }
}
