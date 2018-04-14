using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Common;
using WebSocket4Net;

namespace LiveCommuneSitePlugin
{
    internal class MessageProvider
    {
        private IDataServer _server;
        private TwicasSiteOptions _siteOptions;
        private CookieContainer _cc;
        private ILogger _logger;

        public MessageProvider(IDataServer server, TwicasSiteOptions siteOptions, CookieContainer cc, ILogger logger)
        {
            _server = server;
            _siteOptions = siteOptions;
            _cc = cc;
            _logger = logger;
        }

        public Task ConnectAsync(string channelId)
        {
            throw new NotImplementedException();
        }
        public void Disconnect()
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>接続毎にインスタンスを作る</remarks>
    class WebsocketProvider
    {
        public event EventHandler Opened;

        //public event EventHandler<Result> Received;
        WebSocket _ws;
        TaskCompletionSource<object> _tcs;
        public Task ReceiveAsync(string channelId, string sid)
        {
            _tcs = new TaskCompletionSource<object>();
            var cookies = new List<KeyValuePair<string, string>>();
            var url = $"wss://channel001.livecommune.dmm.com/socket.io/?channel_id={channelId}&version=1&EIO=3&transport=websocket&sid={sid}";
            _ws = new WebSocket(url, "", cookies);
            _ws.MessageReceived += _ws_MessageReceived;
            _ws.Opened += _ws_Opened;
            _ws.Error += _ws_Error;
            _ws.Closed += _ws_Closed;
            _ws.Open();
            return _tcs.Task;
        }

        private void _ws_Closed(object sender, EventArgs e)
        {
            _tcs.SetResult(null);
        }

        private void _ws_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            _tcs.SetException(e.Exception);
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
        }

        public void Disconnect()
        {
            _ws?.Close();
            _ws = null;
        }
        public WebsocketProvider()
        {

        }
    }
}