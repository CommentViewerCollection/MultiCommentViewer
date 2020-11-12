using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NicoSitePlugin.Websocket
{
    class MetaProvider
    {
        public event EventHandler<IInternalMessage> MessageReceived;
        Websocket _wc;
        string _broadcastId;
        public async Task ReceiveAsync(string url, string broadcastId)
        {
            _broadcastId = broadcastId;
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.183 Safari/537.36";
            var origin = "https://live2.nicovideo.jp";
            await _wc.ReceiveAsync(url, new List<KeyValuePair<string, string>>(), userAgent, origin);
        }
        public void Disconnect()
        {
            _wc.Disconnect();
        }
        public MetaProvider()
        {
            _wc = new Websocket();
            _wc.Opened += Wc_Opened;
            _wc.Received += Wc_Received;
            _messageParser = CreateMessageParser();
        }
        private IMessageParser CreateMessageParser()
        {
            return new MessageParser2();
        }
        IMessageParser _messageParser;
        private void Wc_Received(object sender, string e)
        {
            var raw = e;
            IInternalMessage internalMessage;
            try
            {
                internalMessage = _messageParser.Parse(raw);
            }
            catch (ParseException ex)
            {
                //_logger.
                return;
            }
            catch (Exception ex)
            {
                //_logger.
                return;
            }
            switch (internalMessage)
            {
                case PingInternalMessage _:
                    {
                        SendPong();
                    }
                    break;
                default:
                    MessageReceived?.Invoke(this, internalMessage);
                    break;
            }
        }

        private void SendPong()
        {
            _wc.Send("{\"type\":\"pong\"}");
        }
        public void SendKeepSeat()
        {
            _wc.Send("{\"type\":\"keepSeat\"}");
        }

        private async void Wc_Opened(object sender, EventArgs e)
        {
            await _wc.SendAsync($"{{\"type\":\"startWatching\",\"data\":{{\"stream\":{{\"quality\":\"abr\",\"protocol\":\"hls\",\"latency\":\"low\",\"chasePlay\":false}},\"room\":{{\"protocol\":\"webSocket\",\"commentable\":true}},\"reconnect\":false}}}}");
        }
    }
}
