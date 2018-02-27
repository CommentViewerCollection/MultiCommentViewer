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
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            CanConnect = false;
            CanDisconnect = true;
            try
            {
                var live_id = GetLiveId(input);
                var cc = new CookieContainer();
                try
                {
                    var cookies = browserProfile.GetCookieCollection("nicovideo.jp");

                    cc.Add(cookies);
                }
                catch { }


                var response = await API.GetPlayerStatusAsync(_nicoServer, live_id, cc);
                if (!response.Success)
                {
                    //TODO:
                    Debug.WriteLine(response.Error.Code);
                    return;
                }
                else
                {
                    var ps = response.PlayerStatus;
                    var myRoomInfo = GetRoomInfo(ps);


                    //old
                    ////1,俺のサーバからPSの取得を試みる。
                    ////2,取れたらそこにms_listがあるはずだから、それを元にRoomInfo[]を作成
                    ////3,ニコ生サーバから取る。
                    ////4,ms_listがあったら配信者と見做して俺のサーバにアップ。RoomInfo[]に変換
                    ////5,無かったらリスナー。1で失敗した場合のみmsを元に各部屋の情報を計算して取得

                    //new
                    //1,まず、自分の部屋をを認識するために普通にpsを取得。コメント投稿時等必要なことがある。
                    //2,1で取ったpsから計算して出来る限りの部屋を取得
                    //3,

                    if (ps.ProviderType== ProviderType.Channel)
                    {
                        _psProvider = new ChannelPlayerStatusProvider();
                    }
                    else if(ps.ProviderType == ProviderType.Community)
                    {
                        _psProvider = new CommunityPlayerStatusProvider();
                    }
                    else if(ps.ProviderType == ProviderType.Official)
                    {
                        _psProvider = new OfficialPlayerStatusProvider();
                    }
                    else
                    {


                        return;
                    }
                    var options = new Old.ResolverOptions(ps);
                    var rooms = await Old.NewRoomResolver.GetRooms(_nicoServer, options);                    

                    var cpTask = _commentProvider.ReceiveAsync();
                    _commentProvider.Add(rooms);
                    var psTask = _psProvider.ReceiveAsync();
                    
                    await Task.WhenAll(cpTask, psTask);
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
        private readonly Dictionary<IUser, ObservableCollection<NicoCommentViewModel2>> _userCommentDict = new Dictionary<IUser, ObservableCollection<NicoCommentViewModel2>>();
        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
        {
            var comments = _userCommentDict[user];
            return comments;
        }

        public Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }
        List<Old.RoomInfo> _rooms = new List<Old.RoomInfo>();
        private readonly object _lockObj = new object();
        private void _psProvider_Received(object sender, Old.IPlayerStatus e)
        {
            var psRooms = GetRoomInfo(e);
            lock (_lockObj)
            {
                var newRooms = _rooms.Except(psRooms);
                _commentProvider.Add(newRooms);
            }
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

        public NicoCommentProvider3(ICommentOptions options, NicoSiteOptions siteOptions, ICommentProvider commentProvider,IDataSource nicoServer, ILogger logger, IUserStore userStore)
        {
            _logger = logger;
            _userStore = userStore;
            _options = options;
            _siteOptions = siteOptions;
            _commentProvider = commentProvider;
            _commentProvider.InitialCommentsReceived += _commentProvider_InitialCommentsReceived;
            _commentProvider.CommentReceived += _commentProvider_CommentReceived;
            _nicoServer = nicoServer;
            CanConnect = true;
            CanDisconnect = false;
        }
        private bool IsKickCommand(Chat chat)
        {
            return (chat.Text.StartsWith("/hb ifseetno ") && (chat.Premium == 2 || chat.Premium == 3));
        }
        private void _commentProvider_InitialCommentsReceived(object sender, InitialChatsReceivedEventArgs e)
        {
            var list = new List<ICommentViewModel>();
            foreach (var chat in e.Chat)
            {
                if (IsKickCommand(chat))
                    continue;
                var user = _userStore.GetUser(chat.UserId);
                if (!_userCommentDict.TryGetValue(user, out ObservableCollection<NicoCommentViewModel2> userComments))
                {
                    userComments = new ObservableCollection<NicoCommentViewModel2>();
                    _userCommentDict.Add(user, userComments);
                }
                var isFirstComment = userComments.Count == 0;
                var cvm = new Old.NicoCommentViewModel2(_options, _siteOptions, chat, e.RoomInfo, user, this, isFirstComment);
                list.Add(cvm);
            }
            InitialCommentsReceived?.Invoke(this, list);
        }

        private void _commentProvider_CommentReceived(object sender, ChatReceivedEventArgs e)
        {
            var chat = e.Chat;
            if (IsKickCommand(chat))
            {
                return;
            }
            var user = _userStore.GetUser(chat.UserId);
            if (!_userCommentDict.TryGetValue(user, out ObservableCollection<NicoCommentViewModel2> userComments))
            {
                userComments = new ObservableCollection<NicoCommentViewModel2>();
                _userCommentDict.Add(user, userComments);
            }
            var isFirstComment = userComments.Count == 0;
            var cvm = new Old.NicoCommentViewModel2(_options, _siteOptions, chat, e.RoomInfo, user, this, isFirstComment);
            userComments.Add(cvm);
            CommentReceived?.Invoke(this, cvm);
        }

        private IPlayerStatusProvider _psProvider;
        private readonly ICommentOptions _options;
        private readonly NicoSiteOptions _siteOptions;
        private readonly ICommentProvider _commentProvider;
        private readonly IDataSource _nicoServer;
    }
}
