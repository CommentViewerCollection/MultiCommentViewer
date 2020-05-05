using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin
{
    public class WebSocket : IWebsocket
    {
        public event EventHandler Opened;
        public event EventHandler<string> Received;

        public void Disconnect()
        {
            _cts?.Cancel();
        }
        CancellationTokenSource _cts;
        private readonly string _url;

        private bool IsClosing(WebSocketCloseStatus? closeStatus)
        {
            return closeStatus.HasValue;
        }
        public async Task ReceiveAsync()
        {
            _cts = new CancellationTokenSource();
            var w = new ClientWebSocket();
            _ws = w;
            await w.ConnectAsync(new Uri(_url), _cts.Token);
            Opened?.Invoke(this, EventArgs.Empty);
            while (true)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await w.ReceiveAsync(new ArraySegment<byte>(buf), _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                if (IsClosing(result.CloseStatus))
                {
                    break;
                }
                if (result.MessageType == WebSocketMessageType.Text && result.EndOfMessage)
                {
                    var s = Encoding.UTF8.GetString(buf, 0, result.Count);
                    Received?.Invoke(this, s);
                }
            }
            _cts = null;
        }
        ClientWebSocket _ws;
        public Task SendAsync(string s)
        {
            if (_cts == null)
            {
                throw new InvalidOperationException();
            }
            var bytes = Encoding.UTF8.GetBytes(s);
            return _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        private byte[] buf;
        public WebSocket(string url)
        {
            _url = url;
            buf = new byte[8192];
        }
    }
}
