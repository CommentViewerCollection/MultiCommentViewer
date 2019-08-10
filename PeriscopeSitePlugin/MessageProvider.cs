using System;
using Common;
using SitePlugin;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;

namespace PeriscopeSitePlugin
{
    class MessageProvider : IMessageProvider
    {
        public event EventHandler<IInternalMessage> Received;
        private readonly IWebsocket _ws;
        private readonly ILogger _logger;
        string _accessToken;
        string _roomId;
        public async Task ReceiveAsync(string hostname, string accessToken, string roomId)
        {
            _accessToken = accessToken;
            _roomId = roomId;
            await _ws.ReceiveAsync("wss://" + hostname + "/chatapi/v1/chatnow");
        }
        public MessageProvider(IWebsocket websocket, ILogger logger)
        {
            _ws = websocket;
            _logger = logger;
            websocket.Opened += Websocket_Opened;
            websocket.Received += Websocket_Received;
        }
        public void Disconnect()
        {
            _ws.Disconnect();
        }

        private void Websocket_Received(object sender, string e)
        {
            var raw = e;
            Debug.WriteLine(raw);
            try
            {
                var websocketMessage = MessageParser.ParseWebsocketMessage(raw);
                var internalMessage = MessageParser.Parse(websocketMessage);
                if (internalMessage != null)
                {
                    Received?.Invoke(this, internalMessage);
                }
            }
            catch (ParseException ex)
            {
                _logger.LogException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }

        private async void Websocket_Opened(object sender, EventArgs e)
        {
            //{"payload":"{\"access_token\":\"\"}","kind":3}
            //{"payload":"{\"body\":\"{\\\"room\\\":\\\"1MnxnvRQOAoxO\\\"}\",\"kind\":1}","kind":2}
            await _ws.SendAsync(CreateInitialMessage1(_accessToken));
            await _ws.SendAsync(CreateInitialMessage2(_roomId));
        }
        protected virtual string CreateInitialMessage1(string accessToken)
        {
            return "{\"payload\":\"{\\\"access_token\\\":\\\"" + accessToken + "\\\"}\",\"kind\":3}";
        }
        protected virtual string CreateInitialMessage2(string roomId)
        {
            return "{\"payload\":\"{\\\"body\\\":\\\"{\\\\\\\"room\\\\\\\":\\\\\\\"" + roomId + "\\\\\\\"}\\\",\\\"kind\\\":1}\",\"kind\":2}";
        }
    }
}
