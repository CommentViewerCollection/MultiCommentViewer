using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;
using System.Diagnostics;
namespace TwitchSitePlugin
{
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
