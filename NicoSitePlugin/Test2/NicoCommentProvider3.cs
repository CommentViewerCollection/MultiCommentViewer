using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using NicoSitePlugin.Old;
using ryu_s.BrowserCookie;
using SitePlugin;
using System.Collections.Concurrent;

namespace NicoSitePlugin.Test2
{
    class NicoCommentProvider3 : Old.INicoCommentProvider
    {
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
        private string GetLiveId(string input)
        {
            var match = Regex.Match(input, "(lv\\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            throw new ArgumentException();
        }
        private void SendInfo(string message)
        {
            CommentReceived?.Invoke(this, new InfoCommentViewModel(_options, message));
        }
        RoomInfo _currentRoomInfo;
        IPlayerStatus _ps;
        CookieContainer _cc;
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            CanConnect = false;
            CanDisconnect = true;
            _currentRoomInfo = null;
            _currentRoomComments = new SynchronizedCollection<Chat>();
            _ticket = null;
            try
            {
                var live_id = GetLiveId(input);
                _cc = new CookieContainer();
                try
                {
                    var cookies = browserProfile.GetCookieCollection("nicovideo.jp");

                    _cc.Add(cookies);
                }
                catch { }


                var response = await API.GetPlayerStatusAsync(_nicoServer, live_id, _cc);
                if (!response.Success)
                {
                    //TODO:
                    SendInfo($"放送情報を取得できませんでした ({response.Error.Code})");
                    return;
                }
                else
                {
                    var ps = response.PlayerStatus;
                    _ps = ps;
                    var myRoomInfo = GetRoomInfo(ps);
                    _currentRoomInfo = GetCurrentRoomInfo(ps);

                    //old
                    ////1,俺のサーバからPSの取得を試みる。
                    ////2,取れたらそこにms_listがあるはずだから、それを元にRoomInfo[]を作成
                    ////3,ニコ生サーバから取る。
                    ////4,ms_listがあったら配信者と見做して俺のサーバにアップ。RoomInfo[]に変換
                    ////5,無かったらリスナー。1で失敗した場合のみmsを元に各部屋の情報を計算して取得

                    //new
                    //1,まず、自分の部屋をを認識するために普通にpsを取得。コメント投稿時等必要なことがある。
                    //2,1で取ったpsから計算して出来る限りの部屋を取得
                    //3,コメントの取得を開始する
                    //4,数分毎に

                    if (ps.ProviderType== ProviderType.Channel|| ps.ProviderType== ProviderType.Community)
                    {
                        _psProvider = new ChannelCommunityPlayerStatusProvider(_nicoServer, live_id, 5 * 60 * 1000, _cc);
                    }
                    else if(ps.ProviderType == ProviderType.Official)
                    {
                        _psProvider = new OfficialPlayerStatusProvider();
                    }
                    else
                    {


                        return;
                    }
                    _psProvider.Received += _psProvider_Received;
                    var options = new Old.ResolverOptions(ps);
                    var rooms = await Old.NewRoomResolver.GetRooms(_nicoServer, options);
                    
                    
                    var cpTask = _commentProvider.ReceiveAsync();
                    AddRooms(rooms);
                    var psTask = _psProvider.ReceiveAsync();

                    var heartbeartProvider = new HeartbeatProvider(_nicoServer);
                    var heartbeartTask = heartbeartProvider.ReceiveAsync(live_id, _cc);

                    //TODO:例外が起きた時の挙動が嫌いだからWhenAllは使いたくない
                    var tasks = new List<Task>
                    {
                        cpTask,
                        psTask,
                        heartbeartTask,
                    };
                    while (tasks.Count > 0)
                    {
                        var t = await Task.WhenAny(tasks);
                        if(t == heartbeartTask)
                        {
                            //Heartbeartが終了した。
                            //追い出しによるエラーで終了した場合は復帰できない。
                            try
                            {
                                await heartbeartTask;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                            tasks.Remove(heartbeartTask);
                        }
                        else if (t == psTask)
                        {
                            try
                            {
                                await psTask;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                            tasks.Remove(psTask);
                        }
                        else
                        {
                            heartbeartProvider.Disconnect();
                            _psProvider.Disconnect();
                            try
                            {
                                await heartbeartTask;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                            try
                            {
                                await psTask;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                            try
                            {
                                await cpTask;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                            break;
                        }
                    }
                    Debug.WriteLine("ニコ生 NicoCommentProvider.ReceiveAsync() 終了");
                }
            }
            finally
            {
                CanConnect = true;
                CanDisconnect = false;
            }
        }
        public void Disconnect()
        {
            _commentProvider?.Disconnect();
            _psProvider?.Disconnect();
        }
        private readonly ConcurrentDictionary<string, int> _userCommentCountDict = new ConcurrentDictionary<string, int>();
        private NicoCommentViewModel2 CreateCommentViewModel(Chat chat, RoomInfo roomInfo)
        {
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
            var cvm = new NicoCommentViewModel2(_options, _siteOptions, chat, roomInfo, user, this, isFirstComment);
            return cvm;
        }
        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }
        private int GetBlockNo(int latestCommentNo)
        {
            var blockNo = (latestCommentNo + 1) / 100;
            return blockNo;
        }
        public async Task PostCommentAsync(string text, string mail)
        {
            Debug.Assert(_cc != null);
            Debug.Assert(!string.IsNullOrEmpty(_ticket));
            //自分の部屋はどこ？
            var threadId = _currentRoomInfo.Thread;

            int blockNo;
            if (_currentRoomComments.Count == 0)
            {
                blockNo = 0;
            }
            else
            {
                var latestComment = _currentRoomComments[_currentRoomComments.Count - 1];
                var latestCommentNo = latestComment.No;
                if (latestCommentNo.HasValue)
                {
                    blockNo = GetBlockNo(latestCommentNo.Value);
                }
                else
                {
                    //official
                    blockNo = 0;
                }
            }
            
            
            
            string postKey = null;
            try
            {
                postKey = await API.GetPostKey(_nicoServer, threadId, blockNo, _cc);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                SendInfo("コメント投稿に失敗しました");
                return;
            }
            if (string.IsNullOrEmpty(postKey))
            {
                SendInfo("追い出しを受けたか放送が終了しているためコメントが投稿できませんでした");
                return;
            }
            var nowUnix = ToUnixTime(DateTime.Now);
            var baseUnix = _ps.BaseTime;
            int vpos = (nowUnix - baseUnix) *100;
            var user_id = _ps.UserId;
            var premium = _ps.IsPremium;
            var locale = "ja-jp";
            var encodedText = System.Web.HttpUtility.HtmlEncode(text);
            var xml = $"<chat thread=\"{threadId}\" ticket=\"{_ticket}\" vpos=\"{vpos}\" postkey=\"{postKey}\" mail=\"{mail}\" user_id=\"{user_id}\" premium=\"{premium}\" locale=\"{locale}\">{encodedText}</chat>\0";
            //"<chat thread=\"" + this.PlayerStatus_.MS.Thread + "\" ticket=\"" + this.CommentInfo_.Ticket + "\" vpos=\"" + str2 + "\" postkey=\"" + this.PostKey_ + "\" mail=\"" + A_0 + "\" user_id=\"" + this.PlayerStatus_.User.UserId + "\" premium=\"" + this.PlayerStatus_.User.IsPremium + "\" locale=\"" + str + str3 + "\">" + HttpUtility.HtmlEncode(A_1) + "</chat>\0";
            await _commentProvider.SendAsync(_currentRoomInfo, xml);
        }
        private int ToUnixTime(DateTime dateTime)
        {
            var baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var utc = dateTime.ToUniversalTime();
            var span = utc - baseDate;
            return (int)span.TotalSeconds;
        }
        List<Old.RoomInfo> _rooms = new List<Old.RoomInfo>();
        private readonly object _lockObj = new object();
        /// <summary>
        /// 部屋を追加
        /// </summary>
        /// <param name="rooms"></param>
        private void AddRooms(List<RoomInfo> rooms)
        {
            lock (_lockObj)
            {
                var newRooms = rooms.Except(_rooms).ToList();
                _commentProvider.Add(newRooms);
            }
        }
        private void _psProvider_Received(object sender, Old.IPlayerStatus e)
        {
            var psRooms = GetRoomInfo(e);
            AddRooms(psRooms);
        }
        private List<Old.RoomInfo> GetRoomInfo(Old.IPlayerStatus ps)
        {
            if (ps.MsList.Length > 0)
            {
                return ps.MsList.Select(ms => new Old.RoomInfo(ms, ps.RoomLabel)).ToList();
            }
            else
            {
                return new List<Old.RoomInfo>
                {
                    new Old.RoomInfo(ps.Ms, ps.RoomLabel),
                };
            }
        }
        private RoomInfo GetCurrentRoomInfo(IPlayerStatus ps)
        {
            if (ps == null) throw new ArgumentNullException(nameof(ps));
            var roomInfo = new RoomInfo(ps.Ms, ps.RoomLabel);
            return roomInfo;
        }

        public NicoCommentProvider3(ICommentOptions options, NicoSiteOptions siteOptions, ICommentProvider commentProvider,IDataSource nicoServer, ILogger logger, IUserStore userStore)
        {
            _logger = logger;
            _userStore = userStore;
            _options = options;
            _siteOptions = siteOptions;
            _commentProvider = commentProvider;
            _commentProvider.TicketReceived += _commentProvider_TicketReceived;
            _commentProvider.InitialCommentsReceived += _commentProvider_InitialCommentsReceived;
            _commentProvider.CommentReceived += _commentProvider_CommentReceived;
            _nicoServer = nicoServer;
            CanConnect = true;
            CanDisconnect = false;
        }
        
        private void _commentProvider_TicketReceived(object sender, TicketReceivedEventArgs e)
        {
            if (e.RoomInfo.Equals(_currentRoomInfo))
            {
                _ticket = e.Ticket;
            }
        }

        private bool IsKickCommand(Chat chat)
        {
            return (chat.Text.StartsWith("/hb ifseetno ") && (chat.Premium == 2 || chat.Premium == 3));
        }
        /// <summary>
        /// 現在の部屋のコメント。追い出しコマンドとかも全部
        /// </summary>
        private SynchronizedCollection<Chat> _currentRoomComments;
        /// <summary>
        /// 現在の部屋のticket。コメント投稿に必要
        /// </summary>
        string _ticket;
        private void _commentProvider_InitialCommentsReceived(object sender, InitialChatsReceivedEventArgs e)
        {
            var list = new List<ICommentViewModel>();
            var roomInfo = e.RoomInfo;
            var isCurrentRoom = _currentRoomInfo.Equals(roomInfo);
            foreach (var chat in e.Chat)
            {
                if (isCurrentRoom)
                {
                    _currentRoomComments.Add(chat);
                }
                if (IsKickCommand(chat))
                    continue;
                var cvm = CreateCommentViewModel(chat, roomInfo);
                list.Add(cvm);
            }
            InitialCommentsReceived?.Invoke(this, list);
        }

        private void _commentProvider_CommentReceived(object sender, ChatReceivedEventArgs e)
        {
            var chat = e.Chat;
            var roomInfo = e.RoomInfo;
            var isCurrentRoom = _currentRoomInfo.Equals(roomInfo);
            if (isCurrentRoom)
            {
                _currentRoomComments.Add(chat);
            }
            if (IsKickCommand(chat))
                return;
            var cvm = CreateCommentViewModel(chat, roomInfo);
            CommentReceived?.Invoke(this, cvm);
        }

        private IPlayerStatusProvider _psProvider;
        private readonly ICommentOptions _options;
        private readonly NicoSiteOptions _siteOptions;
        private readonly ICommentProvider _commentProvider;
        private readonly IDataSource _nicoServer;
    }
}
