using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Codeplex.Data;

namespace NicoSitePlugin.Websocket
{
    class ReceivedChat
    {
        public bool IsInitialComment { get; set; }
        public IChat Message { get; set; }
    }
    class MessageProvider
    {
        Websocket _wc;
        string _threadId;
        string _url;
        public bool IsConnected { get; private set; }
        public async Task ReceiveAsync(string url, string threadId)
        {
            if (IsConnected) return;
            IsConnected = true;
            _isInitialComment = true;

            _url = url;
            _threadId = threadId;
            var userAgent = "";
            var origin = "https://live2.nicovideo.jp";
            var subProtocol = "msg.nicovideo.jp#json";
            await _wc.ReceiveAsync(url, new List<KeyValuePair<string, string>>(), userAgent, origin, subProtocol);
            IsConnected = false;
            return;
        }
        public void Disconnect()
        {
            if (!IsConnected) return;
            _wc.Disconnect();
        }
        public MessageProvider()
        {
            _wc = new Websocket();
            _wc.Opened += Wc_Opened;
            _wc.Received += Wc_Received;
        }
        private IThread ParseThread(string raw)
        {
            var obj = Tools.Deserialize<Low.Thread.RootObject>(raw);
            var lowThread = obj.Thread;
            var thread = new Thread(raw)
            {
                LastRes = lowThread.LastRes,
                Resultcode = lowThread.Resultcode,
                Revision = lowThread.Revision,
                ServerTime = lowThread.ServerTime,
                ThreadId = lowThread.ThreadThread,
                Ticket = lowThread.Ticket,
            };
            return thread;
        }
        private IChat ParseChat(string raw)
        {
            var obj = Tools.Deserialize<Low.Chat.RootObject>(raw);
            var lowChat = obj.Chat;
            var chat = new Chat()
            {
                Anonymity = lowChat.Anonymity,
                DateStr = lowChat.Date.ToString(),
                Locale = lowChat.Locale,
                Mail = lowChat.Mail,
                No = lowChat.No,
                //Origin=
                Premium = lowChat.Premium,
                Raw = raw,
                Score = lowChat.Score,
                Text = lowChat.Content,
                Thread = lowChat.Thread,
                UserId = lowChat.UserId,
                VposStr = lowChat.Vpos.ToString(),
                //Yourpost=
            };
            return chat;
        }
        private bool _isInitialComment;
        public void SetMessage(string raw)
        {
            //{"ping":{"content":"rs:0"}}
            //{"ping":{"content":"ps:0"}}
            //{"thread":{"resultcode":0,"thread":1651546717,"last_res":179,"ticket":"0x5ad4480","revision":1,"server_time":1559313215}}
            //{"chat":{"thread":1651546717,"no":1,"vpos":12833,"date":1559311996,"date_usec":168105,"user_id":"89121734","content":"待ってた"}}
            //{"ping":{"content":"pf:0"}}
            //{"ping":{"content":"rf:0"}}
            //{"chat":{"thread":1651546717,"no":658,"vpos":527866,"date":1559317149,"date_usec":731458,"mail":"184","user_id":"c1_CPPYyCY1VBN2nBvbUSSfwF3g","anonymity":1,"content":"このきちげぇにチェーンソーで切られるぞ"}}

            Debug.WriteLine(raw);
            var d = DynamicJson.Parse(raw);
            if (d.IsDefined("chat"))
            {
                var chat = ParseChat(raw);
                ChatReceived?.Invoke(this, new ReceivedChat { IsInitialComment = _isInitialComment, Message = chat });
            }
            else if (d.IsDefined("ping"))
            {
                var content = (string)d.ping.content;
                if (content == "rf:0")
                {
                    _isInitialComment = false;
                }
            }
            else if (d.IsDefined("thread"))
            {
                var thread = ParseThread(raw);
                ThreadReceived?.Invoke(this, thread);
            }
            else
            {
                throw new ParseException(raw);
            }
        }
        private void Wc_Received(object sender, string e)
        {
            var raw = e;
            SetMessage(raw);
        }

        private void Wc_Opened(object sender, EventArgs e)
        {
            var threadId = _threadId;
            var userId = "guest";
            var res_from = 1000;
            var data = $"[{{\"ping\":{{\"content\":\"rs:0\"}}}},{{\"ping\":{{\"content\":\"ps:0\"}}}},{{\"thread\":{{\"thread\":\"{threadId}\",\"version\":\"20061206\",\"fork\":0,\"user_id\":\"{userId}\",\"res_from\":-{res_from},\"with_global\":1,\"scores\":1,\"nicoru\":0}}}},{{\"ping\":{{\"content\":\"pf:0\"}}}},{{\"ping\":{{\"content\":\"rf:0\"}}}}]";
            Send(data);


        }
        public void Send(string str)
        {
            _wc.Send(str);
        }
        public event EventHandler<ReceivedChat> ChatReceived;
        public event EventHandler<IThread> ThreadReceived;

        internal void PostComment(string threadId, string vpos, string ticket, string userId, string premium, string postkey, string mail, string comment)
        {
            var a1 = "{\"ping\":{\"content\":\"rs:1\"}}";
            var a2 = "{\"ping\":{\"content\":\"ps:5\"}}";
            var a3 = $"{{\"chat\":{{\"thread\":\"{threadId}\",\"vpos\":{vpos},\"mail\":\"{mail}\",\"ticket\":\"{ticket}\",\"user_id\":\"{userId}\",\"premium\":{premium},\"content\":\"{comment}\",\"postkey\":\"{postkey}\"}}}}";
            var a4 = "{\"ping\":{\"content\":\"pf:5\"}}";
            var a5 = "{\"ping\":{\"content\":\"rf:1\"}}";
            var k = "[" + string.Join(",", a1, a2, a3, a4, a5) + "]";
            Send(k);
        }
    }
}
