using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using SitePlugin;
using ryu_s.BrowserCookie;
using Common;
using System.Diagnostics;
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
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            CanConnect = false;
            CanDisconnect = true;

            try
            {
                var vid = input;

                var cookies = browserProfile.GetCookieCollection("youtube.com");
                _cc = new CookieContainer();
                _cc.Add(cookies);
                chatProvider = new ChatProvider(_logger);
                chatProvider.InitialActionsReceived += ChatProvider_InitialActionsReceived;
                chatProvider.ActionsReceived += ChatProvider_ActionsReceived;
                var t = chatProvider.ReceiveAsync(vid, _cc);

                await Task.WhenAll(t);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            finally
            {
                chatProvider = null;
                CanConnect = true;
                CanDisconnect = false;
            }
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
