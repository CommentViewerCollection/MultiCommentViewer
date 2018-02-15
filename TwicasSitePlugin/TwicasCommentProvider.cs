using System;
using Common;
using System.Windows.Threading;
using SitePlugin;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Net;
using System.Diagnostics;

namespace TwicasSitePlugin
{
    class TwicasCommentProvider : ICommentProvider
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


        private CookieContainer _cc;
        MessageProvider _messageProvider;
        public async Task ConnectAsync(string input, global::ryu_s.BrowserCookie.IBrowserProfile browserProfile)
        {
            var broadcasterId = Tools.ExtractBroadcasterId(input);
            if (string.IsNullOrEmpty(broadcasterId))
            {
                //Info
                return;
            }
            //listall
            _cc = new CookieContainer();
            var cookies = browserProfile.GetCookieCollection("twitcasting.tv");
            _cc.Add(cookies);

            CanConnect = false;
            CanDisconnect = true;
            try
            {
                _messageProvider = new MessageProvider(_server, _cc, _logger);
                _messageProvider.Received += MessageProvider_Received;
                _messageProvider.MetaReceived += MessageProvider_MetaReceived;

                await _messageProvider.ConnectAsync(broadcasterId);
            }
            catch(InvalidBroadcasterIdException ex)
            {
                _logger.LogException(ex);
                //TODO:Info
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                _messageProvider = null;
                CanConnect = true;
                CanDisconnect = false;
            }
        }

        private void MessageProvider_MetaReceived(object sender, IMetadata e)
        {
            MetadataUpdated?.Invoke(this, e);
        }

        private void MessageProvider_Received(object sender, IEnumerable<ICommentData> e)
        {
            foreach (var data in e)
            {
                var cvm = new TwicasCommentViewModel(_connectionName, _options, data);
                CommentReceived?.Invoke(this, cvm);
            }
        }

        public void Disconnect()
        {
            if(_messageProvider != null)
            {
                _messageProvider.Disconnect();
            }
        }

        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        public Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }
        private readonly Dictionary<IUser, ObservableCollection<TwicasCommentViewModel>> _userCommentDict = new Dictionary<IUser, ObservableCollection<TwicasCommentViewModel>>();
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly ConnectionName _connectionName;
        private readonly IOptions _options;
        private readonly TwicasSiteOptions _siteOptions;
        private readonly IUserStore _userStore;
        private readonly Dispatcher _dispatcher;
        public TwicasCommentProvider(ConnectionName connectionName, IDataServer server, ILogger logger, IOptions options, TwicasSiteOptions siteOptions, IUserStore userStore, Dispatcher dispacher)
        {
            _connectionName = connectionName;
            _server = server;
            _logger = logger;
            _options = options;
            _siteOptions = siteOptions;
            _userStore = userStore;
            _dispatcher = dispacher;

            CanConnect = true;
            CanDisconnect = false;
        }
    }
}
