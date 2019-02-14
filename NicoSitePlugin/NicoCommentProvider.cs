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

namespace NicoSitePlugin
{
    internal interface INicoCommentProviderInternal
    {
        void BeforeConnect();
        void AfterDisconnected();
        Task ConnectAsync(string input, CookieContainer cc);
        void Disconnect();
        event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        event EventHandler<ICommentViewModel> CommentReceived;
        event EventHandler<IMetadata> MetadataUpdated;
        event EventHandler<ConnectedEventArgs> Connected;
        bool IsValidInput(string input);
    }
    
    class NicoCasCommentProvider: INicoCommentProviderInternal
    {
        private readonly ICommentOptions _options;
        private readonly INicoSiteOptions _siteOptions;
        private readonly IUserStore _userStore;
        private readonly IDataSource _dataSource;
        private readonly ILogger _logger;
        private readonly ICommentProvider _commentProvider;
        private readonly ConcurrentDictionary<string, int> _userCommentCountDict = new ConcurrentDictionary<string, int>();

        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
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
        public NicoCasCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IUserStore userStore, IDataSource dataSource, ILogger logger, ICommentProvider commentProvider)
        {
            _options = options;
            _siteOptions = siteOptions;
            _userStore = userStore;
            _dataSource = dataSource;
            _logger = logger;
            _commentProvider = commentProvider;
        }
    }
    class JikkyoCommentProvider : INicoCommentProviderInternal
    {
        private readonly ICommentOptions _options;
        private readonly INicoSiteOptions _siteOptions;
        private readonly IUserStore _userStore;
        private readonly IDataSource _dataSource;
        private readonly ILogger _logger;
        private readonly ICommentProvider _commentProvider;
        private readonly ConcurrentDictionary<string, int> _userCommentCountDict = new ConcurrentDictionary<string, int>();

        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
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
            _roomCommentProvider = new Next20181012.XmlSocketRoomCommentProvider(jkInfo.Name, jkInfo.ThreadId, 1000, CreateStreamSoket(jkInfo.XmlSocketAddr, jkInfo.XmlSocketPort));
            _roomCommentProvider.CommentReceived += (s, e) =>
            {
                var chat = e;
                Debug.WriteLine(chat.Text);
                var userId = chat.UserId;
                bool isFirstComment;
                if (_userCommentCountDict.ContainsKey(userId))
                {
                    _userCommentCountDict[userId]++;
                    isFirstComment = false;
                }
                else
                {
                    _userCommentCountDict.AddOrUpdate(userId, 1, (s0, n) => n);
                    isFirstComment = true;
                }
                var user = _userStore.GetUser(userId);
                var cvm = new NicoCommentViewModel2(_options, _siteOptions, e, jkInfo.Name, user, _commentProvider, isFirstComment);
                CommentReceived?.Invoke(this, cvm);
            };
            await _roomCommentProvider.ReceiveAsync();
            await Task.CompletedTask;
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
        public JikkyoCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IUserStore userStore, IDataSource dataSource, ILogger logger, ICommentProvider commentProvider)
        {
            _options = options;
            _siteOptions = siteOptions;
            _userStore = userStore;
            _dataSource = dataSource;
            _logger = logger;
            _commentProvider = commentProvider;
        }
    }
    class CommunityCommentProvider:INicoCommentProviderInternal
    {
        #region INicoCommentProviderInternal
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
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
            var info = new SystemInfoCommentViewModel(_options, message, type);
            CommentReceived?.Invoke(this, info);
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
                liveId = await API.GetCurrentCommunityLiveId(_dataSource, communityId);
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
        /// <summary>
        /// 現在の放送の部屋のThreadIdで一番小さいもの。
        /// 基本的にはアリーナがこれに該当するが、自分の部屋しか取れない場合もあるためそれを考慮してこういう形にした。
        /// </summary>
        string _mainRoomThreadId;
        private readonly ICommentOptions _options;
        private readonly INicoSiteOptions _siteOptions;
        private readonly IUserStore _userStore;
        private readonly IDataSource _dataSource;
        private readonly ILogger _logger;
        private readonly ICommentProvider _commentProvider;

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

        private void ChatProvider_InitialCommentsReceived(object sender, InitialChatsReceivedEventArgs e)
        {
            try
            {
                var items = e.Chat.Select(c =>
                {
                    ICommentViewModel cvm = CreateCommentViewModel(c, e.RoomInfo);
                    return cvm;
                }).Where(c => c != null).ToList();
                InitialCommentsReceived?.Invoke(this, items);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                SendSystemInfo(ex.Message, InfoType.Debug);
            }

        }

        private void ChatProvider_CommentReceived(object sender, ChatReceivedEventArgs e)
        {
            try
            {
                var cvm = CreateCommentViewModel(e.Chat, e.RoomInfo);
                if (cvm == null) return;
                CommentReceived?.Invoke(this, cvm);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                SendSystemInfo(ex.Message, InfoType.Debug);
            }
        }

        public CommunityCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IUserStore userStore, IDataSource dataSource, ILogger logger,ICommentProvider commentProvider)
        {
            _options = options;
            _siteOptions = siteOptions;
            _userStore = userStore;
            _dataSource = dataSource;
            _logger = logger;
            _commentProvider = commentProvider;
        }
        internal static string GetAdComment(string nicoad)
        {
            string text = null;
            var match = Regex.Match(nicoad, "\"latestNicoad\":{\"advertiser\":\"([^\"]+)\",\"point\":(\\d+)(?:,\"message\":\"([^\"]+)\")?}");
            if (match.Success)
            {
                var name = match.Groups[1].Value;
                var points = int.Parse(match.Groups[2].Value);
                string message = match.Groups[3].Value;
                if (!string.IsNullOrEmpty(message))
                {
                    text = $"提供：{name}「{message}」（{points}pt）";
                }
                else
                {
                    text = $"提供：{name}（{points}pt）";
                }
            }
            return text;
        }
        private readonly ConcurrentDictionary<string, int> _userCommentCountDict = new ConcurrentDictionary<string, int>();
        private NicoCommentViewModel2 CreateCommentViewModel(Chat chat, IXmlWsRoomInfo roomInfo)
        {
            if (chat.Premium.HasValue)
            {
                Debug.WriteLine($"{chat.Thread},{chat.Premium.Value},{chat.Text}");
            }
            //追い出しコマンドは除外
            if (chat.Text.StartsWith("/hb ifseetno "))
            {
                SendSystemInfo($"kick command={chat.Text}", InfoType.Debug);
                return null;
            }
            //アリーナ以外の運営コメントは除外
            if ((chat.Premium == 2 || chat.Premium == 3) && chat.Thread != _mainRoomThreadId)
            {
                SendSystemInfo($"{chat.Text}", InfoType.Debug);
                return null;
            }
            if ((chat.Premium == 2 || chat.Premium == 3) && chat.Text.StartsWith("/uadpoint"))
            {
                SendSystemInfo($"{chat.Text}", InfoType.Debug);
                return null;
            }
            //アリーナ以外のBSPコメントは除外
            if (chat.IsBsp && chat.Thread != _mainRoomThreadId)
            {
                SendSystemInfo($"{chat.Text}", InfoType.Debug);
                return null;
            }
            if (chat.Text.StartsWith("/nicoad "))
            {
                chat.Text = GetAdComment(chat.Text);
            }
            var userId = chat.UserId;
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
            var user = _userStore.GetUser(userId);
            var cvm = new NicoCommentViewModel2(_options, _siteOptions, chat, roomInfo.Name, user, _commentProvider, isFirstComment);
            return cvm;
        }


    }
    class NicoCommentProvider : INicoCommentProvider
    {
        private readonly ICommentOptions _options;
        private readonly INicoSiteOptions _siteOptions;
        private readonly IDataSource _dataSource;
        private readonly ILogger _logger;
        private readonly IUserStore _userStore;

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

                cc.Add(cookies);
            }
            catch { }
            return cc;
        }
        static List<INicoCommentProviderInternal> GetCommentProviderInternals(ICommentOptions options, INicoSiteOptions siteOptions, IUserStore userStore,IDataSource dataSource,ILogger logger, ICommentProvider cp)
        {
            var list = new List<INicoCommentProviderInternal>
            {
                new NicoCasCommentProvider(options,siteOptions,userStore,dataSource,logger,cp),
                new CommunityCommentProvider(options,siteOptions,userStore,dataSource,logger,cp),
                new JikkyoCommentProvider(options,siteOptions,userStore,dataSource,logger,cp),

            };
            return list;
        }
        public static bool IsValidInput(ICommentOptions options, INicoSiteOptions siteOptions, IUserStore userStore, IDataSource dataSource, ILogger logger, ICommentProvider cp, string input)
        {
            foreach(var cpin in GetCommentProviderInternals(options, siteOptions, userStore, dataSource, logger, cp))
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

            var list = GetCommentProviderInternals(_options, _siteOptions, _userStore, _dataSource, _logger, this);
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
                var f = new NicoCasCommentProvider(_options, _siteOptions, _userStore, _dataSource, _logger, this);
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
            _internal.InitialCommentsReceived += (s, e) => InitialCommentsReceived?.Invoke(s, e);
            _internal.CommentReceived += (s, e) => CommentReceived?.Invoke(s, e);
            _internal.MetadataUpdated += (s, e) => MetadataUpdated?.Invoke(s, e);
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
            return _userStore.GetUser(userId);
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

        public NicoCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions,IDataSource dataSource, ILogger logger, IUserStore userStore)
        {
            _options = options;
            _siteOptions = siteOptions;
            _dataSource = dataSource;
            _logger = logger;
            _userStore = userStore;

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
