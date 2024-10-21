using System;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Authentication;
using WebSocket4Net;
using System.Diagnostics;
using System.Collections.Generic;

namespace BLiveSitePlugin
{
    class Websocket
    {
        public event EventHandler Opened;
        public event EventHandler<string> Received;
        WebSocket _ws;

        private void Log(string str)
        {
            Debug.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            Debug.WriteLine(str);
        }
        public Task ReceiveAsync(string url, List<KeyValuePair<string, string>> cookies, string userAgent, string origin)
        {
            if (_ws != null)
                throw new InvalidOperationException("_ws is not null");

            var tcs = new TaskCompletionSource<object>();
            var subProtocol = "";
            _ws = new WebSocket4Net.WebSocket(url, subProtocol, cookies, null, userAgent, origin, WebSocket4Net.WebSocketVersion.Rfc6455)
            {
                EnableAutoSendPing = false,
                AutoSendPingInterval = 0,
                ReceiveBufferSize = 8192,
                NoDelay = true
            };

            // Tls12
            _ws.Security.EnabledSslProtocols = SslProtocols.Tls12;

            _ws.MessageReceived += (s, e) =>
            {
                Log("_ws.MessageReceived: " + e.Message);
                Received?.Invoke(this, e.Message);
            };
            _ws.DataReceived += (s, e) =>
            {
                //ここに来たことは今のところ一度もない。
                Debug.WriteLine("Dataが送られてきた");
            };
            _ws.Opened += (s, e) =>
            {
                Log("_ws.Opened");
                Opened?.Invoke(this, EventArgs.Empty);
            };
            _ws.Closed += (s, e) =>
            {
                Log("_ws.Closed");
                try
                {
                    tcs.TrySetResult(null);
                }
                finally
                {
                    if (_ws != null)
                    {
                        _ws.Dispose();
                        _ws = null;
                    }
                }
            };
            _ws.Error += (s, e) =>
            {
                Log("_ws.Error");
                try
                {
                    tcs.SetException(e.Exception);
                }
                finally
                {
                    if (_ws != null)
                    {
                        _ws.Dispose();
                        _ws = null;
                    }
                }
            };
            _ws.Open();
            return tcs.Task;
        }
        public async Task SendAsync(string str)
        {
            await Task.Yield();
            if (_ws != null)
            {
                _ws.Send(str);
                Debug.WriteLine("websocket send:" + str);
            }
            await Task.CompletedTask;
        }
        public void Disconnect()
        {
            if (_ws != null)
            {
                _ws.Close();
            }
        }
        public Websocket()
        {

        }
    }
}
