using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SitePluginCommon
{
    public abstract class CommentProviderBase : ICommentProvider
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
        private readonly ILogger _logger;
        private readonly ICommentOptions _options;

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
        public Guid SiteContextGuid { get; set; }
        protected virtual CookieContainer GetCookieContainer(IBrowserProfile browserProfile, string domain)
        {
            var cc = new CookieContainer();
            try
            {
                var cookies = browserProfile.GetCookieCollection(domain);
                foreach (var cookie in cookies)
                {
                    cc.Add(cookie);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            return cc;
        }
        protected void SendSystemInfo(string message, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(message) },
                NameItems = null,
                SiteType = SiteType.Periscope,
                Type = type,
            }, _options);
            MessageReceived?.Invoke(this, context);
        }
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<IMessageContext> MessageReceived;
        protected void RaiseMessageReceived(IMessageContext context)
        {
            MessageReceived?.Invoke(this, context);
        }
        public abstract Task ConnectAsync(string input, IBrowserProfile browserProfile);

        public abstract void Disconnect();

        public abstract IUser GetUser(string userId);

        public abstract Task PostCommentAsync(string text);

        public abstract Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile);
        protected virtual void BeforeConnect()
        {
            CanConnect = false;
            CanDisconnect = true;
        }
        protected virtual void AfterDisconnected()
        {
            CanConnect = true;
            CanDisconnect = false;
        }
        public CommentProviderBase(ILogger logger, ICommentOptions options)
        {
            _logger = logger;
            _options = options;

            CanConnect = true;
            CanDisconnect = false;
        }
    }
}
