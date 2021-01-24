using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;
namespace Common
{
    public class Websocket : IWebsocket
    {
        public bool EnableAutoSendPing { get; set; } = false;
        public int AutoSendPingInterval { get; set; } = 0;
        public int ReceiveBufferSize { get; set; } = 8192;
        public bool NoDelay { get; set; } = true;
        public string SubProtocol { get; set; } = "";
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36";
        public bool IsConnected { get; private set; }
        public string Origin { get; set; }
        public event EventHandler Opened;

        public event EventHandler<string> Received;
        public event EventHandler<byte[]> DataReceived;
        WebSocket _ws;
        TaskCompletionSource<object> _tcs;
        public Task ReceiveAsync(string url)
        {
            if (IsConnected) return Task.CompletedTask;
            IsConnected = true;
            _tcs = new TaskCompletionSource<object>();
            var cookies = new List<KeyValuePair<string, string>>();
            _ws = new WebSocket(url, SubProtocol, cookies, null, UserAgent, Origin)
            {
                EnableAutoSendPing = EnableAutoSendPing,
                AutoSendPingInterval = AutoSendPingInterval,
                ReceiveBufferSize = ReceiveBufferSize,
                NoDelay = NoDelay,
            };
            _ws.MessageReceived += Ws_MessageReceived;
            _ws.DataReceived += Ws_DataReceived;
            _ws.Opened += Ws_Opened;
            _ws.Error += Ws_Error;
            _ws.Closed += Ws_Closed;
            _ws.Open();
            return _tcs.Task;
        }

        private void Ws_DataReceived(object sender, DataReceivedEventArgs e)
        {
            DataReceived?.Invoke(this, e.Data);
        }

        private void Ws_Closed(object sender, EventArgs e)
        {
            IsConnected = false;
            _tcs.TrySetResult(null);
        }

        private void Ws_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            IsConnected = false;
            _tcs.TrySetException(e.Exception);
        }

        private void Ws_Opened(object sender, EventArgs e)
        {
            Opened?.Invoke(this, e);
        }

        public async Task SendAsync(string s)
        {
            await Task.Yield();
            _ws.Send(s);
        }
        public void Send(string s)
        {
            _ws.Send(s);
        }

        private void Ws_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Received?.Invoke(this, e.Message);
        }

        public void Disconnect()
        {
            _ws?.Close();
            _ws = null;
        }
    }
}
