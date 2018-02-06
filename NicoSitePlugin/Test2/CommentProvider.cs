using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NicoSitePlugin.Test;
using System.Threading;
namespace NicoSitePlugin.Test2
{
    public class CommentContext
    {
        public chat Chat { get; set; }
        public RoomInfo RoomInfo { get; set; }
    }
    /// <summary>
    /// コメントを取得する機構はCommunity,Channel,Official問わず全て同じ。違うのはRoomInfoの取得方法。
    /// よって、このクラスでは共通項であるコメント取得機構を提供する
    /// </summary>
    class CommentProvider
    {
        //とりあえずstring。のちのちchat+RoomInfoとかにしたい
        public event EventHandler<CommentContext> CommentReceived;
        public event EventHandler<List<string>> InitialCommentsReceived;
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
                        }
                        //await room.ReceiveTask;するべきだろう。
                        break;
                    }
                    else if(_newRooms != null)
                    {
                        //新しい部屋を追加
                        foreach(var newRoom in _newRooms)
                        {
                            var socket = new StreamSocket(newRoom.Addr, newRoom.Port, 8192, new SplitBuffer2("\0"));
                            socket.Received += Socket_Received;
                            await socket.ConnectAsync();
                            var xml = $"<thread thread=\"{newRoom.Thread}\" version=\"20061206\" res_from=\"-{_res_from}\" scores=\"1\" />\0";
                            await socket.SendAsync(xml);
                            _roomDict.Add(newRoom, new RoomContext { Info = newRoom, Socket = socket, ReceiveTask = socket.ReceiveAsync() });
                        }
                        _newRooms = null;
                    }
                }
                else
                {
                    var context = _roomDict.Where(kv => kv.Value.ReceiveTask == t).First().Value;
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
                    }
                    else
                    {
                        _roomDict.Remove(context.Info);
                    }
                }

            }
        }

        private void Socket_Received(object sender, List<string> e)
        {
            var context = _roomDict.Where(kv => kv.Value.Socket == sender).First().Value;
            if(context.IsInitialCommentsBeingSent)
            {
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
                var chat = new chat(comment);
                var commentContext = new CommentContext { Chat = chat, RoomInfo = context.Info };
                CommentReceived?.Invoke(this, commentContext);
            }
        }

        class RoomContext
        {
            public RoomInfo Info { get; set; }
            public StreamSocket Socket { get; set; }
            public Task ReceiveTask { get; set; }
            /// <summary>
            /// InitialCommentsが送られてきている
            /// </summary>
            public bool IsInitialCommentsBeingSent { get; set; } = true;
            public int RetryCount { get; set; }
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
        public CommentProvider(int res_from)
        {
            _res_from = res_from;
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
