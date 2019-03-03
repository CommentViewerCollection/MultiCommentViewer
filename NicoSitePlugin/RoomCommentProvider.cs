using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NicoSitePlugin
{
    class CommentContext
    {
        public ChatProvider Chat { get; set; }
        public IXmlWsRoomInfo RoomInfo { get; set; }
    }
    class RoomStatusChangedEventArgs
    {
        public IXmlWsRoomInfo RoomInfo { get; set; }
        public string Status { get; set; }
    }
    [Obsolete("Use XmlSocketRoomCommentProvider")]
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
                else if (_retryCount > 10)
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
            if (_socket == null)
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
        private readonly IXmlWsRoomInfo _thisRoomInfo;
        public IXmlWsRoomInfo RoomInfo => _thisRoomInfo;
        private readonly IStreamSocket _socket;
        public RoomCommentProvider(IXmlWsRoomInfo info, int res_from, IStreamSocket socket)
        {
            _thisRoomInfo = info;
            _res_from = res_from;
            _socket = socket;
            _socket.Received += Socket_Received;

            _addr = _thisRoomInfo.XmlSocketAddr;
            _port = _thisRoomInfo.XmlSocketPort;
            _thread = _thisRoomInfo.ThreadId;
            _roomName = _thisRoomInfo.Name;
        }

        private void Socket_Received(object sender, List<string> e)
        {
            var list = e;
            if (!_isThreadReceived)
            {
                var threadStr = list[0];
                var thread = new Thread(threadStr);
                //<thread resultcode="0" thread="1622377992" last_res="12" ticket="0x1f8f7b00" revision="1" server_time="1520078989"/>
                _isThreadReceived = true;
                if (thread.Resultcode == 0)
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
                if (list.Count > 3)
                {
                    InitialCommentsReceived?.Invoke(this, list.Select(s => new Chat(s)).Cast<IChat>().ToList());
                    return;
                }
                else
                {
                    _isInitialCommentsBeingSent = false;
                }
            }
            foreach (var chatStr in list)
            {
                if (chatStr.StartsWith("<chat "))
                {
                    var chat = new Chat(chatStr);
                    CommentReceived?.Invoke(this, chat);
                }
                else if (chatStr.StartsWith("<chat_result "))
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
        public event EventHandler<IChat> CommentReceived;
        public event EventHandler<List<IChat>> InitialCommentsReceived;
    }
}
