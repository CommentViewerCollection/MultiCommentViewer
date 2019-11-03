using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Common;
using SitePlugin;
namespace MixerSitePlugin
{
    class Message
    {
        public MessageType Type { get; set; }
        public string Id { get; set; }
        public string Comment { get; set; }
        public long CreatedAt { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
    }
    interface IMessageProvider
    {
        event EventHandler<IInternalMessage> MessageReceived;
        Task ReceiveAsync(long channelId, long? myUserId, string token);
        void Disconnect();
        //void Send(string message);
        void Send(IInternalMessage message);
    }
    /// <summary>
    /// サーバとInternalMessageのやり取りを行うクラス
    /// </summary>
    class MessageProvider:IMessageProvider
    {
        private readonly IWebsocket _webSocket;
        private readonly ILogger _logger;

        public event EventHandler<IInternalMessage> MessageReceived;
        long _channelId;
        long? _myUserId;
        string _token;

        public Task ReceiveAsync(long channelId, long? myUserId, string token)
        {
            _methodReplyDict.Clear();
            _channelId = channelId;
            _myUserId = myUserId;
            _token = token;
            return _webSocket.ReceiveAsync("wss://chat.mixer.com/?version=1.0");
        }
        public void Disconnect()
        {
            _webSocket.Disconnect();
        }
        [Obsolete]
        public void Send(string message)
        {
            _webSocket.Send(message);
        }
        public void Send(IInternalMessage message)
        {
            if(message is MethodBase method)
            {
                _methodReplyDict.AddOrUpdate(method.Id, method, (n, t) => t);
            }
            _webSocket.Send(message.Raw);
        }
        public MessageProvider(IWebsocket webSocket, ILogger logger)
        {
            _webSocket = webSocket;
            _logger = logger;
            webSocket.Opened += WebSocket_Opened;
            webSocket.Received += WebSocket_Received;
        }
        ~MessageProvider()
        {
            _webSocket.Opened -= WebSocket_Opened;
            _webSocket.Received -= WebSocket_Received;
        }

        private void WebSocket_Received(object sender, string e)
        {
            var raw = e;
            Debug.WriteLine(raw);
            try
            {
                var internalMessage = InternalMessageParser.Parse(raw, _methodReplyDict);
                if(internalMessage is WelcomeEvent)
                {
                //    var optOutMethod = new OptOutEventsMethod(0, new string[] { "UserJoin", "UserLeave" });
                //    Send(optOutMethod);
                //}
                //else if(internalMessage is OptOutEventsReply)
                //{
                    AuthMethod authMethod;
                    if (_myUserId.HasValue)
                    {
                        authMethod = new AuthMethod(1, _channelId, _myUserId.Value, _token);
                    }
                    else
                    {
                        authMethod = new AuthMethod(1, _channelId);
                    }
                    Send(authMethod);
                }
                else if(internalMessage is UnknownMessage unknown)
                {
                    _logger.LogException(new ParseException(unknown.Raw));
                }
                MessageReceived?.Invoke(this, internalMessage);
            }
            catch (ParseException ex)
            {
                _logger.LogException(ex);
            }
            catch(Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        ConcurrentDictionary<long, MethodBase> _methodReplyDict = new System.Collections.Concurrent.ConcurrentDictionary<long, MethodBase>();

        private void WebSocket_Opened(object sender, EventArgs e)
        {
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>接続毎にインスタンスを作る</remarks>
    public class WebSocket : IWebsocket
    {
        public event EventHandler Opened;

        public event EventHandler<string> Received;
        WebSocket4Net.WebSocket _ws;
        TaskCompletionSource<object> _tcs;

        public Task ReceiveAsync(string url)
        {
            _tcs = new TaskCompletionSource<object>();
            var cookies = new List<KeyValuePair<string, string>>();
            _ws = new WebSocket4Net.WebSocket(url, "", cookies);
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
            _ws.Send(s);// + "\r\n");
        }
        public void Send(string s)
        {
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
        public WebSocket()
        {
        }
    }
}
