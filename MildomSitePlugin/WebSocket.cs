using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MildomSitePlugin
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>接続毎にインスタンスを作る</remarks>
    public class WebSocket : IWebSocket
    {
        public event EventHandler Opened;

        public event EventHandler<string> Received;
        WebSocket4Net.WebSocket _ws;
        TaskCompletionSource<object> _tcs;
        private readonly string _url;

        public Task ReceiveAsync()
        {
            _tcs = new TaskCompletionSource<object>();
            var cookies = new List<KeyValuePair<string, string>>();
            _ws = new WebSocket4Net.WebSocket(_url, "", cookies);
            _ws.MessageReceived += _ws_MessageReceived;
            //_ws.NoDelay = true;
            _ws.Opened += _ws_Opened;
            _ws.Error += _ws_Error;
            _ws.Closed += _ws_Closed;
            _ws.Open();
            return _tcs.Task;
        }

        private void _ws_Closed(object sender, EventArgs e)
        {
            _tcs.TrySetResult(null);
        }

        private void _ws_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            _tcs.TrySetException(e.Exception);
        }

        private void _ws_Opened(object sender, EventArgs e)
        {
            Opened?.Invoke(this, e);
        }

        public async Task SendAsync(string s)
        {
            Debug.WriteLine("send: " + s);
            await Task.Yield();
            _ws.Send(s);
        }

        private void _ws_MessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
        {
            Received?.Invoke(this, e.Message);
        }

        public void Disconnect()
        {
            _ws?.Close();
            _ws = null;
        }
        public WebSocket(string url)
        {
            _url = url;
        }
    }
}
