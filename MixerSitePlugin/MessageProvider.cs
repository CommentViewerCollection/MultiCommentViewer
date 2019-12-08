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
            return _webSocket.ReceiveAsync();
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

}
