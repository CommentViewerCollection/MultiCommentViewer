using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WhowatchSitePlugin
{
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
    internal class WhowatchMessageContext : IMessageContext
    {
        public SitePlugin.IMessage Message { get; }

        public IMessageMetadata Metadata { get; }

        public IMessageMethods Methods { get; }
        public WhowatchMessageContext(IWhowatchMessage message, IMessageMetadata metadata, IMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
    }
    internal abstract class MessageMetadataBase : IMessageMetadata
    {
        protected readonly ICommentOptions _options;
        protected readonly IWhowatchSiteOptions _siteOptions;

        public virtual Color BackColor => _options.BackColor;

        public virtual Color ForeColor => _options.ForeColor;

        public virtual FontFamily FontFamily => _options.FontFamily;

        public virtual int FontSize => _options.FontSize;

        public virtual FontWeight FontWeight => _options.FontWeight;

        public virtual FontStyle FontStyle => _options.FontStyle;

        public virtual bool IsNgUser => false;
        public bool IsSiteNgUser => false;//TODO:IUserにIsSiteNgUserを追加する
        public virtual bool IsFirstComment { get; protected set; }
        public bool Is184 { get; }
        public IUser User { get; protected set; }
        public ICommentProvider CommentProvider { get; }
        public bool IsVisible
        {
            get
            {
                if (IsNgUser || IsSiteNgUser) return false;

                //TODO:ConnectedとかDisconnectedの場合、表示するエラーレベルがError以下の場合にfalseにしたい
                //→Connected,Disconnectedくらいは常に表示でも良いかも。エラーメッセージだけエラーレベルを設けようか。
                return true;
            }
        }
        public bool IsInitialComment { get; set; }
        public bool IsNameWrapping => _options.IsUserNameWrapping;
        public Guid SiteContextGuid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="options"></param>
        /// <param name="siteOptions"></param>
        /// <param name="user">null可</param>
        /// <param name="cp"></param>
        /// <param name="isFirstComment"></param>
        public MessageMetadataBase(ICommentOptions options, IWhowatchSiteOptions siteOptions, ICommentProvider cp)
        {
            _options = options;
            _siteOptions = siteOptions;
            CommentProvider = cp;

            options.PropertyChanged += Options_PropertyChanged;
            siteOptions.PropertyChanged += SiteOptions_PropertyChanged;
        }

        private void SiteOptions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
            }
        }

        private void Options_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_options.BackColor):
                    RaisePropertyChanged(nameof(BackColor));
                    break;
                case nameof(_options.ForeColor):
                    RaisePropertyChanged(nameof(ForeColor));
                    break;
                case nameof(_options.FontFamily):
                    RaisePropertyChanged(nameof(FontFamily));
                    break;
                case nameof(_options.FontStyle):
                    RaisePropertyChanged(nameof(FontStyle));
                    break;
                case nameof(_options.FontWeight):
                    RaisePropertyChanged(nameof(FontWeight));
                    break;
                case nameof(_options.FontSize):
                    RaisePropertyChanged(nameof(FontSize));
                    break;
                case nameof(_options.FirstCommentFontFamily):
                    RaisePropertyChanged(nameof(FontFamily));
                    break;
                case nameof(_options.FirstCommentFontStyle):
                    RaisePropertyChanged(nameof(FontStyle));
                    break;
                case nameof(_options.FirstCommentFontWeight):
                    RaisePropertyChanged(nameof(FontWeight));
                    break;
                case nameof(_options.FirstCommentFontSize):
                    RaisePropertyChanged(nameof(FontSize));
                    break;
                case nameof(_options.IsUserNameWrapping):
                    RaisePropertyChanged(nameof(IsNameWrapping));
                    break;
            }
        }
        #region INotifyPropertyChanged
        [NonSerialized]
        private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
        /// <summary>
        /// 
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
    internal class CommentMessageMetadata : MessageMetadataBase
    {
        public override Color BackColor
        {
            get
            {
                if (User != null && !string.IsNullOrEmpty(User.BackColorArgb))
                {
                    return Common.Utils.ColorFromArgb(User.BackColorArgb);
                }
                else if (IsFirstComment)
                {
                    return _options.FirstCommentBackColor;
                }
                else
                {
                    return base.BackColor;
                }
            }
        }
        public override Color ForeColor
        {
            get
            {
                if (User != null && !string.IsNullOrEmpty(User.ForeColorArgb))
                {
                    return Common.Utils.ColorFromArgb(User.ForeColorArgb);
                }
                else if (IsFirstComment)
                {
                    return _options.FirstCommentForeColor;
                }
                else
                {
                    return base.ForeColor;
                }
            }
        }
        public override FontFamily FontFamily
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontFamily;
                }
                else
                {
                    return base.FontFamily;
                }
            }
        }
        public override int FontSize
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontSize;
                }
                else
                {
                    return base.FontSize;
                }
            }
        }
        public override FontStyle FontStyle
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontStyle;
                }
                else
                {
                    return base.FontStyle;
                }
            }
        }
        public override FontWeight FontWeight
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontWeight;
                }
                else
                {
                    return base.FontWeight;
                }
            }
        }
        public override bool IsNgUser => User.IsNgUser;
        public CommentMessageMetadata(IWhowatchMessage message, ICommentOptions options, IWhowatchSiteOptions siteOptions, IUser user, ICommentProvider cp, bool isFirstComment)
            : base(options, siteOptions, cp)
        {
            User = user;
            IsFirstComment = isFirstComment;
            user.PropertyChanged += User_PropertyChanged;
        }

        private void User_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(User.IsNgUser):
                    //case nameof(User.IsSiteNgUser):
                    RaisePropertyChanged(nameof(IsVisible));
                    break;
                case nameof(User.BackColorArgb):
                    RaisePropertyChanged(nameof(BackColor));
                    break;
                case nameof(User.ForeColorArgb):
                    RaisePropertyChanged(nameof(ForeColor));
                    break;
            }
        }
    }
    internal class ItemMessageMetadata : MessageMetadataBase
    {
        public override Color BackColor => _siteOptions.ItemBackColor;
        public override Color ForeColor => _siteOptions.ItemForeColor;
        public ItemMessageMetadata(IWhowatchItem message, ICommentOptions options, IWhowatchSiteOptions siteOptions, IUser user, ICommentProvider cp)
            : base(options, siteOptions, cp)
        {
            User = user;
            user.PropertyChanged += User_PropertyChanged;
        }
        private void User_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(User.IsNgUser):
                    //case nameof(User.IsSiteNgUser):
                    RaisePropertyChanged(nameof(IsVisible));
                    break;
            }
        }
    }
    internal class WhowatchMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
    internal class WhowatchCommentProvider : ICommentProvider
    {
        #region ICommentProvider
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
        #endregion//CanConnect

        #region CanDisconnect
        private bool _canDisconnect;
        private readonly IDataServer _server;
        private readonly ICommentOptions _options;
        private readonly IWhowatchSiteOptions _siteOptions;
        private readonly IUserStoreManager _userStoreManager;
        private readonly ILogger _logger;

        public bool IsConnected => CanConnect;
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
        #endregion//CanDisconnect

        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        public event EventHandler<IMessageContext> MessageReceived;

        public async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var cc = CreateCookieContainer(browserProfile);
            var me =  await Api.GetMeAsync(_server, cc);

            return new CurrentUserInfo
            {
                Username = me.AccountName,
                UserId = me.UserPath,
                IsLoggedIn = !string.IsNullOrEmpty(me.UserPath),
            };
        }

        protected virtual CookieContainer CreateCookieContainer(IBrowserProfile browserProfile)
        {
            var cc = new CookieContainer();
            try
            {
                var cookies = browserProfile.GetCookieCollection("whowatch.tv");
                foreach (var cookie in cookies)
                {
                    cc.Add(cookie);
                }
            }
            catch { }
            return cc;
        }
        private void SendSystemInfo(string message, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(message) },
                NameItems = null,
                SiteType = SiteType.Whowatch,
                Type = type,
            }, _options);
            MessageReceived?.Invoke(this, context);
        }
        public bool IsLoggedIn => _me != null && !string.IsNullOrEmpty(_me.UserPath);
        public string LoggedInUsername => _me?.Name;
        long _live_id;
        long _lastUpdatedAt;
        CookieContainer _cc;
        const string PUBLISHING = "PUBLISHING";
        protected virtual void BeforeConnect()
        {
            CanConnect = false;
            CanDisconnect = true;
            _cts = new CancellationTokenSource();
            _first.Reset();
            SendConnectedMessage();
        }
        private void AfterDisconnected()
        {
            CanConnect = true;
            CanDisconnect = false;
            _me = null;
            SendDisconnectedMessage();
        }
        private void SendConnectedMessage()
        {

        }
        private void SendDisconnectedMessage()
        {

        }
        FirstCommentDetector _first = new FirstCommentDetector();
        public virtual async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            //lastUpdatedAt==0でLiveDataを取る
            //配信中だったらそこに入っているInitialCommentsを送る
            //配信してなかったら始まるまでループ
            //websocketでコメントを取り始める

            BeforeConnect();
            try
            {
                _cc = CreateCookieContainer(browserProfile);
                var itemDict = await GetPlayItemsAsync();
                MessageParser.Resolver = new ItemNameResolver(itemDict);

                _me = await Api.GetMeAsync(_server, _cc);

                long live_id = -1;
                var liveIdTest = Tools.ExtractLiveIdFromInput(input);
                if (liveIdTest.HasValue)
                {
                    //inputにLiveIdが含まれていた
                    live_id = liveIdTest.Value;
                }
                else
                {
                    //inputにuserPathが含まれているか調べる
                    var userPath = Tools.ExtractUserPathFromInput(input);
                    if (string.IsNullOrEmpty(userPath))
                    {
                        //LiveIdもuserPathも含まれていなかった
                        throw new InvalidInputException(input);
                    }
                    else
                    {
                        //userPathからLiveIdを取得する
                        live_id = await GetLiveIdFromUserPath(userPath, _cc, _cts.Token);
                    }
                }
                System.Diagnostics.Debug.Assert(live_id != -1);

                var lastUpdatedAt = 0;
                var liveData = await Api.GetLiveDataAsync(_server, live_id, lastUpdatedAt, _cc);
                if (liveData.Live.LiveStatus != PUBLISHING)
                {
                    SendSystemInfo("LiveStatus: " + liveData.Live.LiveStatus, InfoType.Debug);
                    SendSystemInfo("配信が終了しました", InfoType.Notice);
                    AfterDisconnected();
                    return;
                }
                foreach (var initialComment in Enumerable.Reverse(liveData.Comments))
                {
                    Debug.WriteLine(initialComment.Message);

                    var message = MessageParser.ParseMessage(initialComment, "");
                    var context = CreateMessageContext(message);
                    if(context != null)
                    {
                        MessageReceived?.Invoke(this, context);
                    }
                }

                var internalCommentProvider = new InternalCommentProvider();
                _internalCommentProvider = internalCommentProvider;
                internalCommentProvider.MessageReceived += InternalCommentProvider_MessageReceived;
                //var d = internal

                var retryCount = 0;
Retry:
                var commentProviderTask = internalCommentProvider.ReceiveAsync(live_id, liveData.Jwt);
                try
                {
                    await commentProviderTask;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    if(retryCount < 3)
                    {
                        retryCount++;
                        goto Retry;
                    }
                }
            }
            catch (OperationCanceledException)//TaskCancelledもここに来る
            {

            }
            catch(Exception ex)
            {
                _logger.LogException(ex);
            }
            //TODO:Disconnectedメッセージ
            AfterDisconnected();
        }

        private WhowatchMessageContext CreateMessageContext(IWhowatchMessage message)
        {
            IMessageMetadata metadata = null;
            if (message is IWhowatchComment comment)
            {
                var user = GetUser(comment.UserId);
                user.Name = comment.NameItems;
                var isFirstComment = _first.IsFirstComment(user.UserId);
                metadata = new CommentMessageMetadata(comment, _options, _siteOptions, user, this, isFirstComment)
                {
                    IsInitialComment = true,
                    SiteContextGuid = SiteContextGuid,
                };
            }
            else if (message is IWhowatchItem item)
            {
                var user = GetUser(item.UserId.ToString());
                user.Name = item.NameItems;
                metadata = new ItemMessageMetadata(item, _options, _siteOptions, user, this)
                {
                    IsInitialComment = true,
                    SiteContextGuid = SiteContextGuid,
                };
            }
            WhowatchMessageContext context = null;
            if (metadata != null)
            {
                var methods = new WhowatchMessageMethods();
                context = new WhowatchMessageContext(message, metadata, methods);
            }
            return context;
        }

        InternalCommentProvider _internalCommentProvider;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userPath"></param>
        /// <param name="cc"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="TaskCanceledException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        protected virtual async Task<long> GetLiveIdFromUserPath(string userPath, CookieContainer cc, CancellationToken ct)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                var profile = await Api.GetProfileAsync(_server, userPath, cc);
                if (profile.Live == null)
                {
                    await Task.Delay(_siteOptions.LiveCheckIntervalSec * 1000, ct);
                }
                else
                {
                    return profile.Live.Id;
                }
            }
        }
        private void SetNickname(string messageText, IUser user)
        {
            if (_siteOptions.NeedAutoSubNickname)
            {
                var nick = ExtractNickname(messageText);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
        }
        private void InternalCommentProvider_MessageReceived(object sender, IWhowatchMessage e)
        {
            var whowatchMessage = e;
            try
            {
                var context = CreateMessageContext(whowatchMessage);
                if (context != null)
                {
                    MessageReceived?.Invoke(this, context);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(whowatchMessage.Raw);
                _logger.LogException(ex);
            }
        }
        /// <summary>
        /// 文字列から@ニックネームを抽出する
        /// 文字列中に@が複数ある場合は一番最後のものを採用する
        /// 数字だけのニックネームは不可
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected string ExtractNickname(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            var matches = Regex.Matches(text, "(?:@|＠)(\\S+)", RegexOptions.Singleline);
            if (matches.Count > 0)
            {
                foreach (Match match in matches.Cast<Match>().Reverse())
                {
                    var val = match.Groups[1].Value;
                    if (!Regex.IsMatch(val, "^[0-9０１２３４５６７８９]+$"))
                    {
                        return val;
                    }
                }
            }
            return null;
        }
        Dictionary<long, PlayItem> _itemDict;
        IMe _me;
        protected virtual Task<Dictionary<long, PlayItem>> GetPlayItemsAsync()
        {
            return Api.GetPlayItemsAsync(_server);
        }
        CancellationTokenSource _cts;
        public void Disconnect()
        {
            _cts?.Cancel();
            _internalCommentProvider?.Disconnect();
        }

        public IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.Whowatch, userId);
        }

        public async Task PostCommentAsync(string text)
        {
            var res = await Api.PostCommentAsync(_server, _live_id, _lastUpdatedAt, text, _cc);
        }
        public Guid SiteContextGuid { get; set; }

        #endregion //ICommentProvider
        public WhowatchCommentProvider(IDataServer server, ICommentOptions options, IWhowatchSiteOptions siteOptions, IUserStoreManager userStoreManager, ILogger logger)
        {
            _server = server;
            _options = options;
            _siteOptions = siteOptions;
            _userStoreManager = userStoreManager;
            _logger = logger;
            CanConnect = true;
            CanDisconnect = false;
        }
    }
}
