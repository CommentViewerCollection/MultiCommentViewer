using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using SitePluginCommon;

namespace NicoSitePlugin
{
    internal interface INicoCommentProviderInternal
    {
        void BeforeConnect();
        void AfterDisconnected();
        Task ConnectAsync(string input, CookieContainer cc);
        void Disconnect();
        event EventHandler<IMessageContext> MessageReceived;
        event EventHandler<IMetadata> MetadataUpdated;
        event EventHandler<ConnectedEventArgs> Connected;
        bool IsValidInput(string input);
    }
    
    class NicoCasCommentProvider: CommentProviderInternalBase, INicoCommentProviderInternal
    {
        public event EventHandler<IMessageContext> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler<ConnectedEventArgs> Connected;
        public void AfterDisconnected()
        {
        }

        public void BeforeConnect()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="liveId">jk\d+</param>
        /// <param name="cc"></param>
        /// <returns></returns>
        public async Task ConnectAsync(string input, CookieContainer cc)
        {
            //https://cas.nicovideo.jp/user/38655/lv316253164
            var (_, userId, liveId) = IsValidInputWithUserId(input);
            var userInfo = API.GetNicoCasUserInfo(_dataSource, userId);
            await Task.CompletedTask;
        }
        public void Disconnect()
        {
        }
        public bool IsValidInput(string input)
        {
            var (isValid, b, c) = IsValidInputWithUserId(input);
            return isValid;
        }
        public static (bool isValid, string userId, string liveId) IsValidInputWithUserId(string input)
        {
            var match = Regex.Match(input, "cas\\.nicovideo\\.jp/user/(?<userid>\\d+)/(?<liveid>lv\\d+)");
            if (match.Success)
            {
                
                return (true, match.Groups["userid"].Value, match.Groups["liveid"].Value);
            }
            else
            {
                return (false, null, null);
            }
        }
        public NicoCasCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager, IDataSource dataSource, ILogger logger, ICommentProvider commentProvider)
            :base(options,siteOptions,userStoreManager,dataSource, logger)
        {
        }
    }

    class CommentProviderInternalBase
    {
        protected readonly ICommentOptions _options;
        protected readonly INicoSiteOptions _siteOptions;
        protected readonly IUserStoreManager _userStoreManager;
        protected readonly IDataSource _dataSource;
        protected readonly ILogger _logger;
        protected readonly ConcurrentDictionary<string, int> _userCommentCountDict = new ConcurrentDictionary<string, int>();
        /// <summary>
        /// 現在の放送の部屋のThreadIdで一番小さいもの。
        /// 基本的にはアリーナがこれに該当するが、自分の部屋しか取れない場合もあるためそれを考慮してこういう形にした。
        /// </summary>
        protected string _mainRoomThreadId;
        private IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.NicoLive, userId);
        }
        public async Task<NicoMessageContext> CreateMessageContextAsync(IChat chat, string roomName, bool isInitialComment)
        {
            NicoMessageContext messageContext = null;
            INicoMessageMetadata metadata;

            var userId = chat.UserId;


            INicoMessage message;
            var messageType = Tools.GetMessageType(chat, _mainRoomThreadId);
            switch (messageType)
            {
                case NicoMessageType.Comment:
                    {
                        var user = GetUser(userId);
                        var comment = await Tools.CreateNicoComment(chat, user, _siteOptions, roomName, async userid => await API.GetUserInfo(_dataSource, userid), _logger);

                        bool isFirstComment;
                        if (_userCommentCountDict.ContainsKey(userId))
                        {
                            _userCommentCountDict[userId]++;
                            isFirstComment = false;
                        }
                        else
                        {
                            _userCommentCountDict.AddOrUpdate(userId, 1, (s, n) => n);
                            isFirstComment = true;
                        }
                        message = comment;
                        metadata = new CommentMessageMetadata(comment, _options, _siteOptions, user, _cp, isFirstComment)
                        {
                            IsInitialComment = isInitialComment,
                            SiteContextGuid = SiteContextGuid,
                        };
                    }
                    break;
                case NicoMessageType.Info:
                    {
                        var info = Tools.CreateNicoInfo(chat, roomName, _siteOptions);
                        message = info;
                        metadata = new InfoMessageMetadata(info, _options, _siteOptions);
                    }
                    break;
                case NicoMessageType.Ad:
                    {
                        var ad = Tools.CreateNicoAd(chat, roomName, _siteOptions);
                        message = ad;
                        metadata = new AdMessageMetadata(ad, _options, _siteOptions);
                    }
                    break;
                default:
                    message = null;
                    metadata = null;
                    break;
            }
            if (message == null || metadata == null)
            {
                return null;
            }
            else
            {
                var methods = new NicoMessageMethods();
                messageContext = new NicoMessageContext(message, metadata, methods);
                return messageContext;
            }
        }
        public CommentProviderInternalBase(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager, IDataSource dataSource, ILogger logger)
        {
            _options = options;
            _siteOptions = siteOptions;
            _userStoreManager = userStoreManager;
            _dataSource = dataSource;
            _logger = logger;
        }
        public Guid SiteContextGuid { get; set; }
        public ICommentProvider _cp;
    }
    class JikkyoCommentProvider : CommentProviderInternalBase, INicoCommentProviderInternal
    {
        public event EventHandler<IMessageContext> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler<ConnectedEventArgs> Connected;

        public void AfterDisconnected()
        {
        }

        public void BeforeConnect()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="liveId">jk\d+</param>
        /// <param name="cc"></param>
        /// <returns></returns>
        public async Task ConnectAsync(string input, CookieContainer cc)
        {
            var channelId = ExtractJikkyoId(input);
            if (!channelId.HasValue)
            {
                //TODO:throwしたい
                return;
            }
            var jkInfo = await API.GetJikkyoInfoAsync(_dataSource, channelId.Value);
            _mainRoomThreadId = jkInfo.ThreadId;
            _roomCommentProvider = new Next20181012.XmlSocketRoomCommentProvider(jkInfo.Name, jkInfo.ThreadId, 1000, CreateStreamSoket(jkInfo.XmlSocketAddr, jkInfo.XmlSocketPort));
            _roomCommentProvider.CommentReceived +=async (s, e) =>
            {
                var chat = e;
                Debug.WriteLine(chat.Text);
                try
                {
                    var context = await CreateMessageContextAsync(chat, jkInfo.Name, false);
                    if (context != null)
                    {
                        MessageReceived?.Invoke(this, context);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            };
            _roomCommentProvider.InitialCommentsReceived +=async (s, e) =>
            {
                var chats = e;
                foreach (var chat in chats)
                {
                    Debug.WriteLine(chat.Text);
                    try
                    {
                        var context = await CreateMessageContextAsync(chat, jkInfo.Name, false);
                        if (context != null)
                        {
                            MessageReceived?.Invoke(this, context);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                }
            };
            await _roomCommentProvider.ReceiveAsync();
            await Task.CompletedTask;
        }

        private NicoComment Convert(Chat chat, string roomName)
        {
            string id;
            if (chat.No.HasValue)
            {
                var shortName = Tools.GetShortRoomName(roomName);
                id = $"{shortName}:{chat.No}";
            }
            else
            {
                id = roomName;
            }
            var message = new NicoComment(chat.Raw, _siteOptions)
            {
                ChatNo = chat.No,
                CommentItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(chat.Text) },
                Id = id,
                Is184 = Tools.Is184UserId(chat.UserId),
                NameItems = null,
                PostTime = chat.Date.ToString("HH:mm:ss"),
                RoomName = roomName,
                UserIcon = null,
                UserId = chat.UserId,
            };
            return message;
        }
        Next20181012.XmlSocketRoomCommentProvider _roomCommentProvider;
        protected virtual IStreamSocket CreateStreamSoket(string host, int port)
        {
            return new StreamSocket(host, port, 4096, new SplitBuffer("\0"));
        }
        public void Disconnect()
        {
            _roomCommentProvider?.Disconnect();
        }
        public int? ExtractJikkyoId(string input)
        {
            var match = Regex.Match(input, "jk(\\d+)");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            else
            {
                return null;
            }
        }
        public bool IsValidInput(string input)
        {
            var id = ExtractJikkyoId(input);
            return id.HasValue;
        }
        public JikkyoCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager, IDataSource dataSource, ILogger logger, ICommentProvider commentProvider)
            :base(options, siteOptions, userStoreManager, dataSource, logger)
        {
        }
    }
    class CommunityCommentProvider: CommentProviderInternalBase, INicoCommentProviderInternal
    {
        #region INicoCommentProviderInternal
        public event EventHandler<IMessageContext> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler<ConnectedEventArgs> Connected;
        public void BeforeConnect()
        {
            _rooms = new List<IXmlWsRoomInfo>();
            _mainRoomThreadId = int.MaxValue.ToString();
        }
        public void AfterDisconnected()
        {

        }
        public void Disconnect()
        {
            _chatProvider?.Disconnect();
            _programInfoProvider?.Disconnect();
        }
        public string ExtractLiveId(string input)
        {
            var match = Regex.Match(input, "(lv\\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        public string ExtractId(string prefix, string input)
        {
            var match = Regex.Match(input, "(" + prefix + "\\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        public bool ContainsLiveId(string input)
        {
            return !string.IsNullOrEmpty(ExtractId("lv", input));
        }
        public bool ContainsCommunityId(string input)
        {
            return !string.IsNullOrEmpty(ExtractId("co", input));
        }
        public bool ContainsChannelId(string input)
        {
            return !string.IsNullOrEmpty(ExtractId("ch", input));
        }
        bool IsValidChannelUrl(string input)
        {
            return Regex.IsMatch(input, "http://ch.nicovideo.jp/\\S+");
        }
        public bool IsValidInput(string input)
        {
            return ContainsLiveId(input) || ContainsCommunityId(input) || ContainsChannelId(input) || IsValidChannelUrl(input);
        }
        #endregion

        //TimeSpan _serverTimeDiff;

        private void SendSystemInfo(string message, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(message) },
                NameItems = null,
                SiteType = SiteType.NicoLive,
                Type = type,
            }, _options);
            MessageReceived?.Invoke(this, context);
        }
        protected virtual ChatProvider CreateChatProvider(int res_from)
        {
            return new ChatProvider(res_from);
        }
        protected virtual ProgramInfoProvider CreateProgramInfoProvider(IDataSource dataSource, string liveId, CookieContainer cc)
        {
            return new ProgramInfoProvider(dataSource, liveId, cc);
        }
        public async Task ConnectAsync(string input, CookieContainer cc)
        {
            string liveId;
            if (ContainsLiveId(input))
            {
                liveId = ExtractLiveId(input);
                if (string.IsNullOrEmpty(liveId))
                {
                    SendSystemInfo("", InfoType.Error);
                    return;
                }
            }
            else if (ContainsCommunityId(input))
            {
                var communityId = ExtractId("co", input);
                liveId = await API.GetCurrentCommunityLiveId(_dataSource, communityId,cc);
            }
            else if (ContainsChannelId(input))
            {
                var channelId = ExtractId("ch", input);
                liveId = await API.GetCurrentChannelLiveId(_dataSource, channelId);
            }
            else if (IsValidChannelUrl(input))
            {
                var id = Tools.ExtractChannelScreenName(input);
                liveId = await API.GetCurrentChannelLiveId(_dataSource, id);
            }
            else
            {
                throw new ArgumentException("", nameof(input));
            }


            Connected?.Invoke(this, new ConnectedEventArgs
            {
                IsInputStoringNeeded = false,
                UrlToRestore = null,
            });
            //TODO:現在の部屋情報を取得する。これはコメント投稿にのみ必要
            var chatProvider = CreateChatProvider(_siteOptions.ResNum);
            _chatProvider = chatProvider;
            chatProvider.TicketReceived += ChatProvider_TicketReceived;
            chatProvider.InitialCommentsReceived += ChatProvider_InitialCommentsReceived;
            chatProvider.CommentReceived += ChatProvider_CommentReceived;

            var chatTask = chatProvider.ReceiveAsync();
            var programInfoProvider = CreateProgramInfoProvider(_dataSource, liveId, cc);
            _programInfoProvider = programInfoProvider;
            programInfoProvider.ProgramInfoReceived += ProgramInfoProvider_ProgramInfoReceived;
            var piTask = programInfoProvider.ReceiveAsync();

            var tasks = new List<Task>
            {
                chatTask,
                piTask,
            };

            while (tasks.Count > 0)
            {
                var t = await Task.WhenAny(tasks);
                if (t == piTask)
                {
                    try
                    {
                        await piTask;
                    }
                    catch (Exception ex)
                    {
                        SendSystemInfo($"{ex.Message}", InfoType.Notice);
                    }
                    tasks.Remove(piTask);
                }
                else
                {
                    programInfoProvider.Disconnect();
                    chatProvider.Disconnect();
                    try
                    {
                        await piTask;
                    }
                    catch (Exception ex)
                    {
                        SendSystemInfo($"{ex.Message}", InfoType.Notice);
                    }
                    try
                    {
                        await chatTask;
                    }
                    catch (Exception ex)
                    {
                        SendSystemInfo($"{ex.Message}", InfoType.Notice);
                    }
                    break;
                }
            }
        }
        ChatProvider _chatProvider;
        ProgramInfoProvider _programInfoProvider;
        List<IXmlWsRoomInfo> _rooms;

        private void ProgramInfoProvider_ProgramInfoReceived(object sender, IProgramInfo e)
        {
            var metadata = new Metadata
            {
                Title = e.Title,
            };
            MetadataUpdated?.Invoke(this, metadata);

            var newRooms = Tools.Distinct(_rooms, e.Rooms.Cast<IXmlWsRoomInfo>().ToList());
            if (newRooms.Count > 0)
            {
                _mainRoomThreadId = Math.Min(int.Parse(_mainRoomThreadId), newRooms.Select(r => int.Parse(r.ThreadId)).Min()).ToString();
                _rooms.AddRange(newRooms);
                _chatProvider.Add(newRooms);
            }
        }
        private void ChatProvider_TicketReceived(object sender, TicketReceivedEventArgs e)
        {
        }

        private async void ChatProvider_InitialCommentsReceived(object sender, InitialChatsReceivedEventArgs e)
        {
            try
            {                
                foreach(var chat in e.Chat)
                {
                    var messageContext = await CreateMessageContextAsync(chat, e.RoomInfo.Name, true);
                    if (messageContext == null) continue;
                    MessageReceived?.Invoke(this, messageContext);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                SendSystemInfo(ex.Message, InfoType.Debug);
            }

        }

        private async void ChatProvider_CommentReceived(object sender, ChatReceivedEventArgs e)
        {
            try
            {
                var messageContext = await CreateMessageContextAsync(e.Chat, e.RoomInfo.Name, false);
                if (messageContext == null) return;
                MessageReceived?.Invoke(this, messageContext);

                //var cvm = CreateCommentViewModel(e.Chat, e.RoomInfo);
                //if (cvm == null) return;
                //CommentReceived?.Invoke(this, cvm);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                SendSystemInfo(ex.Message, InfoType.Debug);
            }
        }

        public CommunityCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager, IDataSource dataSource, ILogger logger,ICommentProvider commentProvider)
            :base(options, siteOptions, userStoreManager, dataSource,logger)
        {
        }
        private IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.NicoLive, userId);
        }
        private readonly ConcurrentDictionary<string, int> _userCommentCountDict = new ConcurrentDictionary<string, int>();
    }
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

        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
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
        static List<INicoCommentProviderInternal> GetCommentProviderInternals(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager,IDataSource dataSource,ILogger logger, ICommentProvider cp,Guid SiteContextGuid)
        {
            var list = new List<INicoCommentProviderInternal>
            {
                new NicoCasCommentProvider(options, siteOptions, userStoreManager, dataSource, logger, cp)
                {
                    SiteContextGuid=SiteContextGuid,
                },
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
            foreach(var cpin in GetCommentProviderInternals(options, siteOptions, userStoreManager, dataSource, logger, cp, siteContextGuid))
            {
                if(cpin.IsValidInput(input))
                {
                    return true;
                }
            }
            return false;
        }
        INicoCommentProviderInternal _internal;
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
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
                var f = new NicoCasCommentProvider(_options, _siteOptions, _userStoreManager, _dataSource, _logger, this);
                var isValid = f.IsValidInput(input);
                if (isValid)
                {
                    _internal = f;
                }
            }
            if(_internal == null)
            {
                //非対応のInput
                //AfterDisconnected();
                return;
            }
            BeforeConnect();
            _internal.MetadataUpdated += (s, e) => MetadataUpdated?.Invoke(s, e);
            _internal.MessageReceived += (s, e) => MessageReceived?.Invoke(s, e);
            try
            {
                await _internal.ConnectAsync(input, cc);
            }
            catch(Exception ex)
            {
                throw new NicoException("", $"input={input},browser={browserProfile.Type}({browserProfile.ProfileName})", ex);
            }
            finally
            {
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
            throw new NotImplementedException();
        }
        public async Task PostCommentAsync(string comment)
        {
            Debug.WriteLine(comment);
            await Task.CompletedTask;
        }

        public async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var cc = GetCookieContainer(browserProfile);
            string userId = null;
            var cookies = Tools.ExtractCookies(cc);
            foreach(var cookie in cookies)
            {
                if(cookie.Name == "user_session")
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
                var displayName = await API.GetDisplayNameFromUserId(_dataSource, userId);
                info.IsLoggedIn = true;
                info.Username = displayName;
                info.UserId = userId;
            }
            return info;
        }
        public Guid SiteContextGuid { get; set; }
        public NicoCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions,IDataSource dataSource, ILogger logger, IUserStoreManager userStoreManager)
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
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
