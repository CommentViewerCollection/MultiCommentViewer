using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
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
        public WhowatchMessageContext(IWhowatchMessage message, MessageMetadata metadata, IMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
    }
    internal class MessageMetadata : IMessageMetadata
    {
        private readonly IWhowatchMessage _message;
        private readonly ICommentOptions _options;
        private readonly IWhowatchSiteOptions _siteOptions;

        public Color BackColor
        {
            get
            {
                if (_message is IWhowatchItem item)
                {
                    return _siteOptions.ItemBackColor;
                }
                else
                {
                    return _options.BackColor;
                }
            }
        }

        public Color ForeColor
        {
            get
            {
                if (_message is IWhowatchItem item)
                {
                    return _siteOptions.ItemForeColor;
                }
                else
                {
                    return _options.ForeColor;
                }
            }
        }

        public FontFamily FontFamily
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontFamily;
                }
                else
                {
                    return _options.FontFamily;
                }
            }
        }

        public int FontSize
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontSize;
                }
                else
                {
                    return _options.FontSize;
                }
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontWeight;
                }
                else
                {
                    return _options.FontWeight;
                }
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontStyle;
                }
                else
                {
                    return _options.FontStyle;
                }
            }
        }

        public bool IsNgUser => User != null ? User.IsNgUser : false;
        public bool IsSiteNgUser => false;//TODO:IUserにIsSiteNgUserを追加する
        public bool IsFirstComment { get; }
        public bool Is184 { get; }
        public IUser User { get; }
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
        public MessageMetadata(IWhowatchMessage message, ICommentOptions options, IWhowatchSiteOptions siteOptions, IUser user, ICommentProvider cp, bool isFirstComment)
        {
            _message = message;
            _options = options;
            _siteOptions = siteOptions;
            IsFirstComment = isFirstComment;
            User = user;
            CommentProvider = cp;

            //TODO:siteOptionsのpropertyChangedが発生したら関係するプロパティの変更通知を出したい

            options.PropertyChanged += Options_PropertyChanged;
            siteOptions.PropertyChanged += SiteOptions_PropertyChanged;
        }

        private void SiteOptions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_siteOptions.ItemBackColor):
                    if (_message is IWhowatchItem)
                    {
                        RaisePropertyChanged(nameof(BackColor));
                    }
                    break;
                case nameof(_siteOptions.ItemForeColor):
                    if (_message is IWhowatchItem)
                    {
                        RaisePropertyChanged(nameof(ForeColor));
                    }
                    break;
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
        private readonly IUserStore _userStore;
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
                cc.Add(cookies);
            }
            catch { }
            return cc;
        }
        private void SendSystemInfo(string message, InfoType type)
        {
            CommentReceived?.Invoke(this, new SystemInfoCommentViewModel(_options, message, type));
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
            SendMessage(new WhowatchConnected(""), null);
        }
        private void AfterDisconnected()
        {
            CanConnect = true;
            CanDisconnect = false;
            _me = null;
            SendMessage(new WhowatchDisconnected(""), null);
        }
        private void SendMessage(IWhowatchMessage message,IUser user, bool isFirstComment = false)
        {
            MessageReceived?.Invoke(this, new WhowatchMessageContext(message, new MessageMetadata(message, _options, _siteOptions,user,this, isFirstComment), new WhowatchMessageMethods()));
        }
        public virtual async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            //lastUpdatedAt==0でLiveDataを取る
            //配信中だったらそこに入っているInitialCommentsを送る
            //配信してなかったら始まるまでループ
            //websocketでコメントを取り始める

            BeforeConnect();
            try
            {
                var cc = CreateCookieContainer(browserProfile);
                var itemDict = await GetPlayItemsAsync();
                MessageParser.Resolver = new ItemNameResolver(itemDict);

                _me = await Api.GetMeAsync(_server, cc);

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
                var liveData = await Api.GetLiveDataAsync(_server, live_id, lastUpdatedAt, cc);
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
                    var user = GetUser(initialComment.User.Id.ToString());
                    var message = new WhowatchComment("")
                    {
                        AccountName = initialComment.User.AccountName,
                        CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(initialComment.Message) },
                        Id = initialComment.Id.ToString(),
                        NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(initialComment.User.Name) },
                        PostTime = SitePluginCommon.Utils.UnixtimeToDateTime(initialComment.PostedAt / 1000).ToString("HH:mm:ss"),
                        UserIcon = new Common.MessageImage
                        {
                            Url = initialComment.User.IconUrl,
                            Alt = "",
                            Height = 40,//_optionsにcolumnの幅を動的に入れて、ここで反映させたい。propertyChangedはどうやって発生させるか
                            Width = 40,
                        },
                        UserId = initialComment.User.Id.ToString(),
                        UserPath = initialComment.User.UserPath,
                    };
                    var messageMetadata = new MessageMetadata(message, _options, _siteOptions, user, this, false)
                    {
                        IsInitialComment = true,
                    };
                    var methods = new WhowatchMessageMethods();
                    MessageReceived?.Invoke(this, new WhowatchMessageContext(message, messageMetadata, methods));
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
                IMessageContext commentContext;
                if (whowatchMessage is IWhowatchComment comment)
                {
                    var user = GetUser(comment.UserId.ToString());
                    var isFirstComment = false;//TODO:要初コメ検知機能追加
                    var messageText = Common.MessagePartsTools.ToText(comment.CommentItems);
                    SetNickname(messageText, user);
                    commentContext = CreateCommentContext(whowatchMessage, _options, _siteOptions, user, isFirstComment);
                }
                else if(whowatchMessage is IWhowatchItem item)
                {
                    var user = GetUser(item.UserId.ToString());
                    var isFirstComment = false;//TODO:要初コメ検知機能追加
                    var messageText = Common.MessagePartsTools.ToText(item.CommentItems);
                    SetNickname(messageText, user);
                    commentContext = CreateCommentContext(whowatchMessage, _options, _siteOptions, user, isFirstComment);
                }
                else
                {
                    commentContext = null;
                }
                if (commentContext != null)
                {
                    MessageReceived?.Invoke(this, commentContext);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(whowatchMessage.Raw);
                _logger.LogException(ex);
            }
        }

        private IMessageContext CreateCommentContext(IWhowatchMessage message, ICommentOptions options, IWhowatchSiteOptions siteOptions, IUser user, bool isFirstComment)
        {
            var metadata = new MessageMetadata(message, options, siteOptions, user, this, isFirstComment);
            var methods = new WhowatchMessageMethods();
            return new WhowatchMessageContext(message, metadata, methods);
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
            return _userStore.GetUser(userId);
        }

        public async Task PostCommentAsync(string text)
        {
            var res = await Api.PostCommentAsync(_server, _live_id, _lastUpdatedAt, text, _cc);
        }


        #endregion //ICommentProvider
        public WhowatchCommentProvider(IDataServer server, ICommentOptions options, IWhowatchSiteOptions siteOptions, IUserStore userStore, ILogger logger)
        {
            _server = server;
            _options = options;
            _siteOptions = siteOptions;
            _userStore = userStore;
            _logger = logger;
            CanConnect = true;
            CanDisconnect = false;
        }
    }
}
