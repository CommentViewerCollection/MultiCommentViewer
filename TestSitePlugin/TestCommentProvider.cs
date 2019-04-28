using Common;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TestSitePlugin
{
    public class TestCommentProvider : ICommentProvider
    {
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

        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<IMessageContext> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        CancellationTokenSource _cts;
        private readonly ICommentOptions _options;
        private readonly IUserStore _userStore;

        public async Task ConnectAsync(string input, global::ryu_s.BrowserCookie.IBrowserProfile browserProfile)
        {
            if(_cts != null)
            {
                throw new InvalidOperationException("_cts is not null!");
            }
            _cts = new CancellationTokenSource();
            CanConnect = false;
            CanDisconnect = true;
            Connected?.Invoke(this, new ConnectedEventArgs
            {
                IsInputStoringNeeded = false,                 
            });
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, _cts.Token);
                }
                catch (TaskCanceledException) { break; }
            }
            CanConnect = true;
            CanDisconnect = false;
            _cts = null;
        }

        public void Disconnect()
        {
            _cts?.Cancel();
        }
        class CurrentUserInfo : ICurrentUserInfo
        {
            public string Username { get; set; }
            public bool IsLoggedIn { get; set; }
        }
        public Task<ICurrentUserInfo> GetCurrentUserInfo(global::ryu_s.BrowserCookie.IBrowserProfile browserProfile)
        {
            ICurrentUserInfo info = new CurrentUserInfo
            {
                Username = "test",
                IsLoggedIn = true,
            };
            return Task.FromResult(info);
        }

        public IUser GetUser(string userId)
        {
            return _userStore.GetUser(userId);
        }

        public Task PostCommentAsync(string text)
        {
            var arr = text.Split(',');
            if(arr.Length == 2)
            {
                var userId = arr[0];
                var content = arr[1];
                var user = _userStore.GetUser(userId);
                var comment = new TestComment(userId, content);
                var metadata = new TestMetadata(user)
                {
                    SiteContextGuid = SiteContextGuid,
                };
                var methods = new TestMethods();
                var context = new MessageContext(comment, metadata, methods);
                MessageReceived?.Invoke(this, context);
            }
            else if(arr.Length == 3)
            {
                var userId = arr[0];
                var name = arr[1];
                var content = arr[2];
                var user = _userStore.GetUser(userId);
                var comment = new TestComment(userId, name, content);
                var metadata = new TestMetadata(user)
                {
                    SiteContextGuid = SiteContextGuid,
                };
                var methods = new TestMethods();
                var context = new MessageContext(comment, metadata, methods);
                MessageReceived?.Invoke(this, context);
            }
            else
            {
                SendSystemInfo(text, InfoType.Notice);
            }
            return Task.CompletedTask;
        }
        private void SendSystemInfo(string message, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(message) },
                NameItems = null,
                SiteType = SiteType.Openrec,
                Type = type,
            }, _options);
            MessageReceived?.Invoke(this, context);
        }
        public Guid SiteContextGuid { get; set; }
        public TestCommentProvider(ICommentOptions options, IUserStore userStore)
        {
            _options = options;
            _userStore = userStore;
            CanConnect = true;
            CanDisconnect = false;
        }
    }
}
