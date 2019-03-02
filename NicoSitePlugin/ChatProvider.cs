using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace NicoSitePlugin
{
    class ChatReceivedEventArgs : EventArgs
    {
        public IChat Chat { get; set; }
        public IXmlWsRoomInfo RoomInfo { get; set; }
    }
    class InitialChatsReceivedEventArgs : EventArgs
    {
        public List<IChat> Chat { get; set; }
        public IXmlWsRoomInfo RoomInfo { get; set; }
    }
    class TicketReceivedEventArgs : EventArgs
    {
        public string Ticket { get; set; }
        public IXmlWsRoomInfo RoomInfo { get; set; }
    }
    class ChatProvider
    {
        public event EventHandler<TicketReceivedEventArgs> TicketReceived;
        public event EventHandler<ChatReceivedEventArgs> CommentReceived;
        public event EventHandler<InitialChatsReceivedEventArgs> InitialCommentsReceived;
        class RoomContext
        {
            public Task ReceiveTask { get; set; }
            public RoomCommentProvider RoomCommentProvider { get; set; }
            public IXmlWsRoomInfo RoomInfo { get; set; }
        }
        protected virtual IStreamSocket CreateStreamSocket(string addr, int port)
        {
            return new StreamSocket(addr,port, 8192, new SplitBuffer("\0"));
        }
        public async Task ReceiveAsync()
        {
            _isDisconnectOffered = false;
            while (true)
            {
                _cts = new CancellationTokenSource();
                var cancellableTask = CancellableTask();
                var tasks = new List<Task> { cancellableTask };
                tasks.AddRange(_roomDict.Select(kv => kv.Value.ReceiveTask));
                var t = await Task.WhenAny(tasks).ConfigureAwait(false);
                if (t == cancellableTask)
                {
                    if (_isDisconnectOffered)
                    {
                        foreach (var context in _roomDict.Values)
                        {
                            context.RoomCommentProvider.Disconnect();
                        }
                        _roomDict.Clear();
                        break;
                    }
                    else if (_newRooms != null)
                    {
                        foreach (var newRoom in _newRooms)
                        {
                            var addr = newRoom.XmlSocketAddr;
                            var port = newRoom.XmlSocketPort;
                            var thread = newRoom.ThreadId;
                            var socket = CreateStreamSocket(addr, port);
                            var room = new RoomCommentProvider(newRoom, _resFrom, socket);
                            room.TicketReceived += Room_TicketReceived;
                            room.CommentReceived += Room_CommentReceived;
                            room.InitialCommentsReceived += Room_InitialCommentsReceived;
                            var context = new RoomContext
                            {
                                ReceiveTask = room.ReceiveAsync(),
                                RoomCommentProvider = room,
                                RoomInfo = newRoom,
                            };
                            _roomDict.AddOrUpdate(newRoom, context, (info, con) => con);
                        }
                        _newRooms = null;
                    }
                }
                else
                {
                    var context = _roomDict.Where(kv => kv.Value.ReceiveTask == t).First().Value;
                    _roomDict.TryRemove(context.RoomInfo, out _);

                    //TODO:下の2行を入れるべきか
                    //if (_roomDict.Count == 0)
                    //    break;
                }
            }
        }

        private void Room_TicketReceived(object sender, string e)
        {
            var roomProvider = sender as RoomCommentProvider;
            var roomInfo = roomProvider.RoomInfo;
            TicketReceived?.Invoke(this, new TicketReceivedEventArgs { Ticket = e, RoomInfo = roomInfo });
        }

        private void Room_InitialCommentsReceived(object sender, List<IChat> e)
        {
            var chat = e;
            var roomProvider = sender as RoomCommentProvider;
            var roomInfo = roomProvider.RoomInfo;
            InitialCommentsReceived?.Invoke(this, new InitialChatsReceivedEventArgs { Chat = e, RoomInfo = roomInfo });
        }

        private void Room_CommentReceived(object sender, IChat e)
        {
            var chat = e;
            var roomProvider = sender as RoomCommentProvider;
            var roomInfo = roomProvider.RoomInfo;
            CommentReceived?.Invoke(this, new ChatReceivedEventArgs { Chat = chat, RoomInfo = roomInfo });
        }
        public void Disconnect()
        {
            if (_cts != null)
            {
                _isDisconnectOffered = true;
                _cts.Cancel();
            }
        }
        ConcurrentDictionary<IXmlWsRoomInfo, RoomContext> _roomDict = new ConcurrentDictionary<IXmlWsRoomInfo, RoomContext>();
        public void Add(IEnumerable<IXmlWsRoomInfo> newRooms)
        {
            _newRooms = new ConcurrentBag<IXmlWsRoomInfo>();
            foreach (var newRoom in newRooms)
            {
                _newRooms.Add(newRoom);
            }
            _cts.Cancel();
        }
        public async Task SendAsync(IXmlWsRoomInfo roomInfo, string str)
        {
            //コメントを送れるのは自分の部屋だけ。
            //どうやって自分の部屋を識別する？
            var v = _roomDict.Where(kv => kv.Key.Equals(roomInfo));
            if (v.Count() == 0)
            {
                throw new InvalidOperationException("当該の部屋が存在しない");
            }

            var context = v.First().Value;
            await context.RoomCommentProvider.SendAsync(str);
        }
        async Task CancellableTask()
        {
            while (!_cts.IsCancellationRequested)
            {
                await Task.Delay(500);
            }
        }
        public ChatProvider(int res_from)
        {
            _resFrom = res_from;
        }
        /// <summary>
        /// 立て続けにAddされた場合に備えてconcurrentにしておく
        /// </summary>
        ConcurrentBag<IXmlWsRoomInfo> _newRooms;

        CancellationTokenSource _cts;
        bool _isDisconnectOffered;
        private readonly int _resFrom;
    }
}
