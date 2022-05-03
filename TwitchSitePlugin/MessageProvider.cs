using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebSocket4Net;
using System.Diagnostics;
using Mcv.PluginV2;

namespace TwitchSitePlugin
{
    public class MessageProvider2 : IMessageProvider
    {
        public event EventHandler? Opened;

        public event EventHandler<string>? Received;
        IWebsocket? _ws;
        TaskCompletionSource<object?> _tcs = new();
        public Task ReceiveAsync()
        {
            _tcs = new TaskCompletionSource<object?>();
            var cookies = new List<KeyValuePair<string, string>>();
            if (_ws != null)
            {
                _ws.Received -= Ws_MessageReceived;
                _ws.Opened -= Ws_Opened;
            }
            _ws = new Websocket();
            _ws.Received += Ws_MessageReceived;
            _ws.Opened += Ws_Opened;
            return _ws.ReceiveAsync("wss://irc-ws.chat.twitch.tv/");
        }

        private void _ws_Closed(object sender, EventArgs e)
        {
            _tcs.TrySetResult(null);
        }

        private void _ws_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            _tcs.TrySetException(e.Exception);
        }

        private void Ws_Opened(object? sender, EventArgs e)
        {
            Opened?.Invoke(this, e);
        }

        public async Task SendAsync(string s)
        {
            Debug.WriteLine("send: " + s);
            await Task.Yield();
            if (_ws is not null)
            {
                await _ws.SendAsync(s + "\r\n");
            }
        }

        private void Ws_MessageReceived(object? sender, string e)
        {
            var arr = e.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var message in arr)
            {
                Received?.Invoke(this, message);
            }
        }

        public void Disconnect()
        {
            _ws?.Disconnect();
            _ws = null;
        }
        public MessageProvider2()
        {

        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>接続毎にインスタンスを作る</remarks>
    public class MessageProvider : IMessageProvider
    {
        public event EventHandler Opened;

        public event EventHandler<string> Received;
        WebSocket _ws;
        TaskCompletionSource<object> _tcs;
        public Task ReceiveAsync()
        {
            _tcs = new TaskCompletionSource<object>();
            var cookies = new List<KeyValuePair<string, string>>();
            _ws = new WebSocket("wss://irc-ws.chat.twitch.tv/", "", cookies);
            _ws.MessageReceived += _ws_MessageReceived;
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
            _ws.Send(s + "\r\n");
        }

        private void _ws_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var arr = e.Message.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var message in arr)
            {
                Received?.Invoke(this, message);
            }
        }

        public void Disconnect()
        {
            _ws?.Close();
            _ws = null;
        }
        public MessageProvider()
        {

        }
    }
}
