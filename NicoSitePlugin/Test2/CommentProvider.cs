using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NicoSitePlugin.Old;
using System.Threading;
using Common;
using System.Diagnostics;

namespace NicoSitePlugin.Test2
{
    public class CommentContext
    {
        public Chat Chat { get; set; }
        public RoomInfo RoomInfo { get; set; }
    }
    public class RoomStatusChangedEventArgs
    {
        public RoomInfo RoomInfo { get; set; }
        public string Status { get; set; }
    }
    class RoomCommentProvider
    {
        bool _isInitialCommentsBeingSent;
        bool _isThreadReceived;
        bool _isExpectedDisconnect;
        public void Disconnect()
        {
            _isExpectedDisconnect = true;
            _socket.Disconnect();
        }
        int _retryCount;
        /// <summary>
        /// 意図的な切断時以外は終了しない
        /// </summary>
        /// <returns></returns>
        public async Task ReceiveAsync()
        {
            //接続が切れる要因
            //・ユーザによる切断 => Disconnect()
            //・"/disconnect"が送られてきた => このクラスのクライアントにDisconnect()を呼んでもらう
            //・ネットワークの不調 => reconnectの一番の原因だろう。
            //・resultcode="0"のthreadが送られてきた => 内部からDisconnect()
            _retryCount = 0;
            _isExpectedDisconnect = false;
            Debug.WriteLine($"ニコ生 接続開始 ({_roomName},thread={_thread},addr={_addr},port={_port})");
            while (true)
            {
                int res_from;
                if (_retryCount == 0)
                {
                    _isInitialCommentsBeingSent = true;
                    res_from = _res_from;
                }
                else
                {
                    res_from = 0;
                }
                _isThreadReceived = false;
                _isExpectedDisconnect = false;
                await CallConnectAsync();
                await CallSendXmlAsync(res_from);
                await _socket.ReceiveAsync();
                Debug.WriteLine($"ニコ生 切断 ({_roomName},thread={_thread},addr={_addr},port={_port})");
                if (_isExpectedDisconnect)
                {
                    Debug.WriteLine($"ニコ生 意図的な切断 ({_roomName},thread={_thread},addr={_addr},port={_port})");
                    break;
                }
                else if(_retryCount > 10)
                {
                    Debug.WriteLine($"ニコ生 再接続回数が{_retryCount}に達したため終了 ({_roomName},thread={_thread},addr={_addr},port={_port})");
                    break;
                }
                Debug.WriteLine($"ニコ生 意図しない切断のため再接続 ({_roomName},thread={_thread},addr={_addr},port={_port})");
                _retryCount++;
            }
        }
        public async Task SendAsync(string str)
        {
            if(_socket == null)
            {
                throw new InvalidOperationException("_socket is null");
            }
            await _socket.SendAsync(str);
        }
        protected virtual async Task CallConnectAsync()
        {
            await _socket.ConnectAsync();
        }
        protected virtual async Task CallSendXmlAsync(int res_from)
        {
            var xml = $"<thread thread=\"{_thread}\" version=\"20061206\" res_from=\"-{res_from}\" scores=\"1\" />\0";
            await _socket.SendAsync(xml);
        }

        private readonly string _addr;
        private readonly int _port;
        private readonly string _thread;
        private readonly string _roomName;
        private readonly int _res_from;
        private readonly IStreamSocket _socket;
        public RoomCommentProvider(RoomInfo info, int res_from, IStreamSocket socket)
            : this(info.Addr, info.Port, info.Thread, info.RoomLabel, res_from, socket)
        {
        }
        public RoomCommentProvider(string addr, int port, string thread, string roomName,int res_from, IStreamSocket socket)
        {
            this._addr = addr;
            this._port = port;
            this._thread = thread;
            this._roomName = roomName;
            _res_from = res_from;
            _socket = socket;
            _socket.Received += Socket_Received;
        }

        private void Socket_Received(object sender, List<string> e)
        {
            var list = e;
            if (!_isThreadReceived)
            {
                var threadStr = list[0];
                var thread = new Old.Thread(threadStr);
                //<thread resultcode="0" thread="1622377992" last_res="12" ticket="0x1f8f7b00" revision="1" server_time="1520078989"/>
                _isThreadReceived = true;
                if(thread.Resultcode == 0)
                {
                    //ok
                    TicketReceived?.Invoke(this, thread.Ticket);
                    _retryCount = 0;
                    list.RemoveAt(0);
                    if (list.Count == 0) return;
                }
                else
                {
                    Disconnect();
                    return;
                }
            }
            if (_isInitialCommentsBeingSent)
            {
                if(list.Count > 3)
                {
                    InitialCommentsReceived?.Invoke(this, list.Select(s => new Chat(s)).ToList());
                    return;
                }
                else
                {
                    _isInitialCommentsBeingSent = false;
                }
            }
            foreach (var chatStr in list)
            {
                if(chatStr.StartsWith("<chat "))
                {
                    var chat = new Chat(chatStr);
                    CommentReceived?.Invoke(this, chat);
                }
                else if(chatStr.StartsWith("<chat_result "))
                {
                    Debug.WriteLine(chatStr);
                    //<chat_result thread="1622396675" status="0" no="4"/>
                    //status=0で成功、失敗の場合4を確認済み
                    //status=1は連投規制？
                }
                else
                {
                    //<leave_thread thread="1622163911" reason="2"/>
#if DEBUG
                    using (var sw = new System.IO.StreamWriter("nico_unknownData.txt", true))
                    {
                        sw.WriteLine(chatStr);
                    }
#endif
                }
            }
        }
        public event EventHandler<string> TicketReceived;
        public event EventHandler<Chat> CommentReceived;
        public event EventHandler<List<Chat>> InitialCommentsReceived;
    }
    /// <summary>
    /// コメントを取得する機構はCommunity,Channel,Official問わず全て同じ。違うのはRoomInfoの取得方法。
    /// よって、このクラスでは共通項であるコメント取得機構を提供する
    /// </summary>
    [Obsolete]
    class CommentProvider
    {
        //とりあえずstring。のちのちchat+RoomInfoとかにしたい
        public event EventHandler<CommentContext> CommentReceived;
        public event EventHandler<List<string>> InitialCommentsReceived;

        protected virtual IStreamSocket CreateStreamSocket(string addr, int port)
        {
            return new StreamSocket(addr, port, 8192, new SplitBuffer2("\0"));
        }
        protected virtual void OnRoomAdded(RoomInfo info, IStreamSocket socket)
        {

        }

        public async Task ReceiveAsync()
        {
            _isDisconnectOffered = false;
            
            //List<Task> tasks;

            while (true)
            {
                _cts = new CancellationTokenSource();
                var cancellableTask= CancellableTask();
                var tasks = new List<Task> { cancellableTask };
                tasks.AddRange(_roomDict.Select(kv => kv.Value.ReceiveTask));

                var t = await Task.WhenAny(tasks).ConfigureAwait(false);
                if(t == cancellableTask)
                {
                    if (_isDisconnectOffered)
                    {
                        //全部屋.Disconnect()
                        foreach(var room in _roomDict.Select(kv => kv.Value))
                        {
                            room.Socket.Disconnect();
                            OnDisconnected(room.Info);
                            
                        }
                        //await room.ReceiveTask;するべきだろう。
                        _roomDict.Clear();
                        
                        break;
                    }
                    else if(_newRooms != null)
                    {
                        //新しい部屋を追加
                        foreach(var newRoom in _newRooms)
                        {
                            var socket = CreateStreamSocket(newRoom.Addr, newRoom.Port);
                            socket.Received += Socket_Received;

                            await socket.ConnectAsync();
                            var xml = $"<thread thread=\"{newRoom.Thread}\" version=\"20061206\" res_from=\"-{_res_from}\" scores=\"1\" />\0";
                            await socket.SendAsync(xml);
                            var context = new RoomContext { Info = newRoom, Socket = socket, ReceiveTask = socket.ReceiveAsync() };
                            AddContext(context);
                            OnRoomAdded(context.Info, context.Socket);                            
                        }
                        _newRooms = null;
                    }
                }
                else
                {
                    var context = _roomDict.Where(kv => kv.Value.ReceiveTask == t).First().Value;
                    //この時点でサーバとの接続はどうなってる？切断されていない可能性を考えてDisconnectしてみる
                    try
                    {
                        var oldSocket = context.Socket;
                        oldSocket.Disconnect();
                        oldSocket.Received -= Socket_Received;
                        context.Socket = null;
                        if (context.IsExpectedDisconnect)
                        {
                            RemoveContext(context);
                            continue;
                        }
                        OnUnexpectedDisconnected(context.Info);
                    }
                    catch (Exception ex)
                    {
                        OnUnexpectedDisconnectedAndExceptionThrown(context.Info);
                        _logger.LogException(ex, "大事なエラー", "サーバとの接続状況を調査したい");
                    }
                    //どっかの部屋がエラーで終了した
                    //"/disconnect"無しに終了した場合もここで処理することになるだろう。
                    //"/disconnect"が送られてきた場合はこのクラスが直接感知するのではなく、このクラスのユーザがDisconnect()を呼び出すことになる。
                    //エラーの場合は、当然エラーの内容によって処理が異なるだろう。
                    //存在しない部屋だった場合は、threadのresultcodeが1になる。よってここに来ることはない。
                    //3回くらい再接続して、それでもだめなら_roomDictから外す。

                    //いつRetryCountを0に戻せば良いんだ？？
                    if (context.RetryCount < 3)
                    {                        
                        context.RetryCount++;
                        Debug.WriteLine($"再接続{context.RetryCount}回目");
                        OnReconnecting(context.Info);
                        
                        context.IsInitialCommentsBeingSent = true;
                        var addr = context.Info.Addr;
                        var port = context.Info.Port;
                        var socket = CreateStreamSocket(addr, port);
                        context.Socket = socket;
                        socket.Received += Socket_Received;
                        context.IsExpectedDisconnect = false;
                        await socket.ConnectAsync();
                        var xml = $"<thread thread=\"{context.Info.Thread}\" version=\"20061206\" res_from=\"-{0}\" scores=\"1\" />\0";
                        await socket.SendAsync(xml);
                        context.ReceiveTask = socket.ReceiveAsync();
                    }
                    else
                    {
                        RemoveContext(context);
                        OnRemoved(context.Info);
                    }
                }
            }
        }

        protected virtual void OnDisconnected(RoomInfo info)
        {
        }

        protected virtual void OnReconnecting(RoomInfo info)
        {
        }

        protected virtual void OnUnexpectedDisconnectedAndExceptionThrown(RoomInfo info)
        {
        }

        protected virtual void OnUnexpectedDisconnected(RoomInfo info)
        {
        }

        private void AddContext(RoomContext context)
        {
            _roomDict.Add(context.Info, context);
        }
        private void RemoveContext(RoomContext context)
        {
            _roomDict.Remove(context.Info);
        }
        private void Socket_Received(object sender, List<string> e)
        {
            var context = _roomDict.Where(kv => kv.Value.Socket == sender).First().Value;
            if(context.IsInitialCommentsBeingSent)
            {
                if(e[0].StartsWith("<thread "))
                {
                    var thread = new Old.Thread(e[0]);
                    if(thread.Resultcode == 0)
                    {
                        //接続が成功したからRetryCountを0にする
                        OnReceiveSuccessThread(context.Info);
                        
                        context.RetryCount = 0;
                    }
                    else
                    {
                        //この部屋は存在しない
                        
                        OnReceiveFailedThread(context.Info);
                        context.IsExpectedDisconnect = true;
                        var socket = context.Socket;
                        socket.Disconnect();
                        //RemoveContext(context);
                        //OnRemoved(context.Info);
                        

                        //socket.Received -= Socket_Received;
                        return;
                    }
                    e.RemoveAt(0);
                    if (e.Count == 0)
                        return;
                }

                if (e.Count >= 3)
                {
                    //最初コメント数が3つ以上送られてくる間はInitialCommentsとする。
                    InitialCommentsReceived?.Invoke(this, e);
                    return;
                }
                else
                {
                    context.IsInitialCommentsBeingSent = false;
                }
            }
            foreach (var comment in e)
            {

                try
                {
                    var chat = new Chat(comment);
                    var commentContext = new CommentContext { Chat = chat, RoomInfo = context.Info };
                    CommentReceived?.Invoke(this, commentContext);
                }catch(Exception ex)
                {
                    _logger.LogException(ex);
                }
            }
        }

        protected virtual void OnRemoved(RoomInfo info)
        {
        }

        protected virtual void OnReceiveFailedThread(RoomInfo info)
        {
        }

        protected virtual void OnReceiveSuccessThread(RoomInfo info)
        {

        }

        class RoomContext
        {
            public RoomInfo Info { get; set; }
            public IStreamSocket Socket { get; set; }
            public Task ReceiveTask { get; set; }
            /// <summary>
            /// InitialCommentsが送られてきている
            /// </summary>
            public bool IsInitialCommentsBeingSent { get; set; } = true;
            public int RetryCount { get; set; }
            /// <summary>
            /// 意図的な切断であるか
            /// </summary>
            public bool IsExpectedDisconnect { get; set; }
        }
        Dictionary<RoomInfo, RoomContext> _roomDict = new Dictionary<RoomInfo, RoomContext>();
        bool _isDisconnectOffered;
        public void Disconnect()
        {
            if (_cts != null)
            {
                _isDisconnectOffered = true;
                _cts.Cancel();
            }
        }

        /// <summary>
        /// 立て続けにAddされた場合に備えてconcurrentにしておく
        /// </summary>
        System.Collections.Concurrent.ConcurrentBag<RoomInfo> _newRooms;

        CancellationTokenSource _cts;
        async Task CancellableTask()
        {
            while (!_cts.IsCancellationRequested)
            {
                await Task.Delay(500);
            }
        }
        public void Add(IEnumerable<RoomInfo> newRooms)
        {
            _newRooms = new System.Collections.Concurrent.ConcurrentBag<RoomInfo>();
            foreach(var newRoom in newRooms)
            {
                _newRooms.Add(newRoom);
            }
            _cts.Cancel();
        }
        public async Task SendAsync(RoomInfo romInfo, string str)
        {

        }
        int _res_from;
        private readonly ILogger _logger;
        public CommentProvider(int res_from, ILogger logger)
        {
            _res_from = res_from;
            _logger = logger;
        }
    }
    public interface IRoomInfoProvider
    {
        event EventHandler<RoomInfo[]> RoomAdded;
        Task ReceiveAsync();
        void Disconnect();
    }
    class ChannelRoomInfoProvider : IRoomInfoProvider
    {
        public event EventHandler<RoomInfo[]> RoomAdded;
        public async Task ReceiveAsync()
        {

        }
        public void Disconnect()
        {

        }
        private readonly NicoSiteOptions _siteOptions;
        public ChannelRoomInfoProvider(NicoSiteOptions siteOptions)
        {
            _siteOptions = siteOptions;
        }
    }
    class CommunityRoomInfoProvider : IRoomInfoProvider
    {
        public event EventHandler<RoomInfo[]> RoomAdded;
        public async Task ReceiveAsync()
        {

        }
        public void Disconnect()
        {

        }
        private readonly NicoSiteOptions _siteOptions;
        public CommunityRoomInfoProvider(NicoSiteOptions siteOptions)
        {
            _siteOptions = siteOptions;
        }
    }
    class OfficialRoomInfoProvider : IRoomInfoProvider
    {
        public event EventHandler<RoomInfo[]> RoomAdded;
        public async Task ReceiveAsync()
        {
            //追加されることは無いから、最初取得したあとはDisconnect()されるまで待機ループ

        }
        public void Disconnect()
        {

        }
        private readonly NicoSiteOptions _siteOptions;
        public OfficialRoomInfoProvider(NicoSiteOptions siteOptions)
        {
            _siteOptions = siteOptions;
        }
    }

}
