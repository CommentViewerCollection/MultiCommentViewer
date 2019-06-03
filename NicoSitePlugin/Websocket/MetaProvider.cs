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
            var userAgent = "";
            var origin = "https://live2.nicovideo.jp";
            await _wc.ReceiveAsync(url, new List<KeyValuePair<string, string>>(), userAgent, origin);
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
            return new MessageParser();
        }
        IMessageParser _messageParser;
        private void Wc_Received(object sender, string e)
        {
            //{"type":"watch","body":{"command":"statistics","params":["10","0","0","0"]}}
            //{"type":"watch","body":{"command":"servertime","params":["1559238662759"]}}
            //{"type":"watch","body":{"command":"permit","params":["11638528803398"]}}
            //{"type":"watch","body":{"currentStream":{"uri":"https://pa03503c0e0.dmc.nico/hlslive/ht2_nicolive/nicolive-production-pg11638528803398_f8b21634cfbd88ba8cb0d160255872f680386cb5cb850fabca8b791423079e13/master.m3u8?ht2_nicolive=2297426.sh06gy_psbw92_do4o0yyhuyix","name":null,"quality":"abr","qualityTypes":["abr","high","normal","low","super_low"],"mediaServerType":"dmc","mediaServerAuth":null,"streamingProtocol":"hls"},"command":"currentstream"}}
            //{"type":"watch","body":{"room":{"messageServerUri":"wss://msg.live2.nicovideo.jp/u10199/websocket","messageServerType":"niwavided","roomName":"co3860652","threadId":"1651504725","forks":[0],"importedForks":[]},"command":"currentroom"}}
            //{"type":"watch","body":{"command":"statistics","params":["10","0","0","0"]}}
            //{"type":"watch","body":{"command":"watchinginterval","params":["30"]}}
            //{"type":"watch","body":{"update":{"begintime":1559237439000,"endtime":1559248239000},"command":"schedule"}}
            //{"type":"ping","body":{}}
            //{"type":"watch","body":{"command":"statistics","params":["10","0","0","0"]}}
            //{"type":"ping","body":{}}
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
            catch(Exception ex)
            {
                //_logger.
                return;
            }
            switch (internalMessage)
            {
                case IPing _:
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
            _wc.Send("{\"type\":\"pong\",\"body\":{}}");
        }

        private async void Wc_Opened(object sender, EventArgs e)
        {
            var broadcastId = _broadcastId;
            await _wc.SendAsync("{\"type\":\"watch\",\"body\":{\"command\":\"playerversion\",\"params\":[\"leo\"]}}").ConfigureAwait(false);
            await _wc.SendAsync($"{{\"type\":\"watch\",\"body\":{{\"command\":\"getpermit\",\"requirement\":{{\"broadcastId\":\"{broadcastId}\",\"route\":\"\",\"stream\":{{\"protocol\":\"hls\",\"requireNewStream\":true,\"priorStreamQuality\":\"abr\",\"isLowLatency\":false,\"isChasePlay\":false}},\"room\":{{\"isCommentable\":true,\"protocol\":\"webSocket\"}}}}}}}}");
        }
    }
}
