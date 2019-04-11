using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using System.Windows.Threading;

namespace PeriscopeSitePlugin
{
    internal class LiveId
    {
        public LiveId(string liveId)
        {
            Value = liveId;
        }

        public string Value { get; }
    }
    internal class ChannelId
    {
        public ChannelId(string channelId)
        {
            Value = channelId;
        }

        public string Value { get; }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>接続毎にインスタンスを作る</remarks>
    class MessageProvider1
    {
        public event EventHandler Opened;

        public event EventHandler<string> Received;
        ClientWebSocket _ws;
        CancellationTokenSource _cts;
        const int BufferSize = 4096;
        public async Task ReceiveAsync(string url)
        {
            _cts = new CancellationTokenSource();
            var cookies = new List<KeyValuePair<string, string>>();
            _ws = new ClientWebSocket(); // "wss://irc-ws.chat.twitch.tv/", "", cookies);
            //_ws.Options
            await _ws.ConnectAsync(new Uri(url), _cts.Token);
            while(_ws.State == WebSocketState.Open)
            {
                var buff = new ArraySegment<byte>(new byte[BufferSize]);
                var ret = await _ws.ReceiveAsync(buff, _cts.Token);

            }
        }

        public async Task SendAsync(string s)
        {
            Debug.WriteLine("send: " + s);
            await Task.Yield();
            //_ws.SendAsync(.Send(s + "\r\n");
        }

        public void Disconnect()
        {
            _cts.Cancel();
        }
        public MessageProvider1()
        {

        }
    }
}
