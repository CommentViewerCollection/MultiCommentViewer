using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using SitePluginCommon;
using System.Collections.Concurrent;

namespace NicoSitePlugin
{
    class NicoCommentProvider : INicoCommentProvider
    {
        private readonly ICommentOptions _options;
        private readonly INicoSiteOptions _siteOptions;
        private readonly IDataSource _dataSource;
        private readonly ILogger _logger;
        private readonly IUserStoreManager _userStoreManager;

        private bool _canConnect;
        public bool CanConnect
        {
            get { return _canConnect; }
            private set
            {
                _canConnect = value;
                CanConnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool _canDisconnect;


        public bool CanDisconnect
        {
            get { return _canDisconnect; }
            private set
            {
                _canDisconnect = value;
                CanDisconnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<IMessageContext> MessageReceived;

        private void BeforeConnect()
        {
            CanConnect = false;
            CanDisconnect = true;
            _internal.BeforeConnect();

        }
        private void AfterDisconnected()
        {
            CanConnect = true;
            CanDisconnect = false;
            _internal.AfterDisconnected();
        }

        protected virtual CookieContainer GetCookieContainer(IBrowserProfile browserProfile)
        {
            var cc = new CookieContainer();
            try
            {
                var cookies = browserProfile.GetCookieCollection("nicovideo.jp");
                foreach (var cookie in cookies)
                {
                    cc.Add(cookie);
                }
            }
            catch { }
            return cc;
        }
        static List<INicoCommentProviderInternal> GetCommentProviderInternals(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager, IDataSource dataSource, ILogger logger, ICommentProvider cp, Guid SiteContextGuid)
        {
            var list = new List<INicoCommentProviderInternal>
            {
                new CommunityCommentProvider(options, siteOptions, userStoreManager, dataSource, logger, cp)
                {
                    SiteContextGuid=SiteContextGuid,
                },
                new JikkyoCommentProvider(options,siteOptions,userStoreManager,dataSource,logger,cp)
                {
                    SiteContextGuid=SiteContextGuid,
                },
            };
            return list;
        }
        public static bool IsValidInput(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager, IDataSource dataSource, ILogger logger, ICommentProvider cp, string input, Guid siteContextGuid)
        {
            foreach (var cpin in GetCommentProviderInternals(options, siteOptions, userStoreManager, dataSource, logger, cp, siteContextGuid))
            {
                if (cpin.IsValidInput(input))
                {
                    return true;
                }
            }
            return false;
        }
        INicoCommentProviderInternal _internal;
        MultipleCommentsBlocker _blocker = new MultipleCommentsBlocker();
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            _blocker.Reset();
            var cc = GetCookieContainer(browserProfile);

            var list = GetCommentProviderInternals(_options, _siteOptions, _userStoreManager, _dataSource, _logger, this, SiteContextGuid);
            var cu = await GetCurrentUserInfo(browserProfile);
            if (cu.IsLoggedIn)
            {
                foreach (var f in list)
                {
                    var isValid = f.IsValidInput(input);
                    if (isValid)
                    {
                        _internal = f;
                        break;
                    }
                }
            }
            else
            {
                //未ログインでもWebSocket経由なら取れる。
                var f = new NewLiveInternalProvider(_options, _siteOptions, _userStoreManager, _logger, _dataSource)
                {
                    SiteContextGuid = SiteContextGuid,
                };
                var isValid = f.IsValidInput(input);
                if (isValid)
                {
                    _internal = f;
                }
            }
            if (_internal == null)
            {
                //非対応のInput
                //AfterDisconnected();
                return;
            }
            BeforeConnect();
            _internal.MetadataUpdated += (s, e) => MetadataUpdated?.Invoke(s, e);
            _internal.MessageReceived += (s, e) =>
            {
                if (e.Message is INicoComment nicoComment)
                {
                    var userId = nicoComment.UserId;
                    var comment = nicoComment.Text;
                    var postedDate = nicoComment.PostedAt;
                    if (!_blocker.IsUniqueComment(userId, comment, postedDate))
                    {
                        Debug.WriteLine("ニコ生で二重コメントを発見したため無視します");
                        return;
                    }
                }
                MessageReceived?.Invoke(s, e);
            };
            try
            {
                await _internal.ConnectAsync(input, cc);
            }
            catch (Exception ex)
            {
                throw new NicoException("", $"input={input},browser={browserProfile.Type}({browserProfile.ProfileName})", ex);
            }
            finally
            {
                _internal.MetadataUpdated -= (s, e) => MetadataUpdated?.Invoke(s, e);
                _internal.MessageReceived -= (s, e) => MessageReceived?.Invoke(s, e);
                AfterDisconnected();
            }
        }

        public IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.NicoLive, userId);
        }


        public void Disconnect()
        {
            _internal.Disconnect();
        }

        public Task PostCommentAsync(string comment, string mail)
        {
            if (_internal == null)
            {
                Debug.WriteLine("_internal is null");
                return Task.CompletedTask;
            }
            return _internal.PostCommentAsync(comment, mail);
        }
        public Task PostCommentAsync(string comment)
        {
            return PostCommentAsync(comment, "");
        }

        public async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var cc = GetCookieContainer(browserProfile);
            string userId = null;
            var cookies = Tools.ExtractCookies(cc);
            foreach (var cookie in cookies)
            {
                if (cookie.Name == "user_session")
                {
                    var match = Regex.Match(cookie.Value, "^user_session_(\\d+)_");
                    if (match.Success)
                    {
                        userId = match.Groups[1].Value;
                    }
                }
            }
            var info = new CurrentUserInfo();
            if (!string.IsNullOrEmpty(userId))
            {
                string displayName = null;
                try
                {
                    displayName = await API.GetDisplayNameFromUserId(_dataSource, userId);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, $"user_id={userId}");
                }
                if (string.IsNullOrEmpty(displayName))
                {
                    try
                    {
                        var cauUser = await API.GetNicoCasUserInfo(_dataSource, userId);
                        displayName = cauUser.Name;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex, $"user_id={userId}");
                    }
                }
                if (string.IsNullOrEmpty(displayName))
                {
                    displayName = "（不明）";
                }
                info.IsLoggedIn = true;
                info.Username = displayName;
                info.UserId = userId;
            }
            return info;
        }

        public void SetMessage(string raw)
        {
            if (_internal == null)
            {
                return;
            }
            _internal.SetMessage(raw);
        }

        public Guid SiteContextGuid { get; set; }
        public NicoCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IDataSource dataSource, ILogger logger, IUserStoreManager userStoreManager)
        {
            _options = options;
            _siteOptions = siteOptions;
            _dataSource = dataSource;
            _logger = logger;
            _userStoreManager = userStoreManager;

            CanConnect = true;
            CanDisconnect = false;

        }
    }
    class MultipleCommentsBlocker
    {
        ConcurrentDictionary<string, (string comment, DateTime postedDate)> _dict = new ConcurrentDictionary<string, (string comment, DateTime postedDate)>();
        public void Reset()
        {
            _dict.Clear();
        }
        private static readonly object LockObject = new object();
        public bool IsUniqueComment(string userId, string comment, DateTime postedDate)
        {
            lock (LockObject)
            {
                if (_dict.TryGetValue(userId, out var a))
                {
                    if (a.comment == comment && a.postedDate.AddSeconds(5) > postedDate)
                    {
                        return false;
                    }
                    else
                    {
                        _dict.AddOrUpdate(userId, id => (comment, postedDate), (k, c) => (comment, postedDate));
                        return true;
                    }
                }
                else
                {
                    _dict.AddOrUpdate(userId, id => (comment, postedDate), (k, c) => (comment, postedDate));
                    return true;
                }
            }
        }
    }
}
