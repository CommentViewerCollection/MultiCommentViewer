using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

namespace NicoSitePlugin.Next
{

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

        TimeSpan _serverTimeDiff;

        private void SendInfo(string message)
        {
            var info = new InfoCommentViewModel(_options, message);
            CommentReceived?.Invoke(this, info);
        }
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            _rooms = new List<IXmlWsRoomInfo>();
            var liveId = Tools.ExtractLiveId(input);
            if (string.IsNullOrEmpty(liveId))
            {
                SendInfo("");
                return;
            }
            //psを取得する
            var cc = new CookieContainer();
            try
            {
                var cookies = browserProfile.GetCookieCollection("nicovideo.jp");

                cc.Add(cookies);
            }
            catch { }

            var response = await API.GetPlayerStatusAsync(_dataSource, liveId, cc);
            if (!response.Success)
            {
                SendInfo($"放送情報を取得できませんでした ({response.Error.Code})");
                return;
            }
            var ps = response.PlayerStatus;
            //TODO:現在の部屋情報を取得する。これはコメント投稿にのみ必要
            _chatProvider = new ChatProvider(_siteOptions.ResNum);
            _chatProvider.TicketReceived += ChatProvider_TicketReceived;
            _chatProvider.InitialCommentsReceived += ChatProvider_InitialCommentsReceived;
            _chatProvider.CommentReceived += ChatProvider_CommentReceived;

            var chatTask = _chatProvider.ReceiveAsync();
            var programInfoProvider = new ProgramInfoProvider(_dataSource, liveId, cc);
            programInfoProvider.ProgramInfoReceived += ProgramInfoProvider_ProgramInfoReceived;
            var piTask = programInfoProvider.ReceiveAsync();

            var t = await Task.WhenAny(chatTask, piTask);
            if(t == piTask)
            {

            }
            else
            {

            }
        }
        ChatProvider _chatProvider;
        List<IXmlWsRoomInfo> _rooms;
        private void ProgramInfoProvider_ProgramInfoReceived(object sender, IProgramInfo e)
        {
            var metadata = new Metadata
            {
                Title =e.Title,
            };
            MetadataUpdated?.Invoke(this, metadata);

            var newRooms = Tools.Distinct(_rooms, e.Rooms.Cast<IXmlWsRoomInfo>());
            if (newRooms.Count > 0)
            {
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
                  }).ToList();
                InitialCommentsReceived?.Invoke(this, items);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            
        }

        private void ChatProvider_CommentReceived(object sender, ChatReceivedEventArgs e)
        {

        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task PostCommentAsync(string comment, string mail)
        {
            throw new NotImplementedException();
        }
        public NicoCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions,IDataSource dataSource,ILogger logger, IUserStore userStore)
        {
            _options = options;
            _siteOptions = siteOptions;
            _dataSource = dataSource;
            _logger = logger;
            _userStore = userStore;

            CanConnect = true;
            CanDisconnect = false;
        }
        private readonly ConcurrentDictionary<string, int> _userCommentCountDict = new ConcurrentDictionary<string, int>();
        private NicoCommentViewModel CreateCommentViewModel(Chat chat, IXmlWsRoomInfo roomInfo)
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
            var cvm = new NicoCommentViewModel(_options, _siteOptions, chat, roomInfo, user, this, isFirstComment);
            return cvm;
        }
    }
    class ProgramInfoProvider
    {
        CancellationTokenSource _cts;
        const int _interval = 5 * 60 * 1000;
        private readonly IDataSource _dataSource;
        private readonly string _liveId;
        private readonly CookieContainer _cc;
        public event EventHandler<IProgramInfo> ProgramInfoReceived;
        public async Task ReceiveAsync()
        {
            if(_cts != null)
            {
                throw new InvalidOperationException();
            }
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                var programInfo = await API.GetProgramInfo(_dataSource, _liveId, _cc);
                ProgramInfoReceived?.Invoke(this, programInfo);
                try
                {
                    await Task.Delay(_interval);
                }
                catch (TaskCanceledException) { break; }
            }
            _cts = null;
        }
        public void Disconnect()
        {
            if(_cts != null)
            {
                _cts.Cancel();
            }
        }
        public ProgramInfoProvider(IDataSource dataSource,string liveId,CookieContainer cc)
        {
            _dataSource = dataSource;
            _liveId = liveId;
            _cc = cc;
        }
    }
    public class Metadata : IMetadata
    {
        public string Title { get; set; }
        public string Elapsed { get; set; }
        public string CurrentViewers { get; set; }
        public string Active { get; set; }
        public string TotalViewers { get; set; }
        public bool? IsLive { get; set; }
    }
}
