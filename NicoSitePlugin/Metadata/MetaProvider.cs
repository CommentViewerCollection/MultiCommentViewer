using System;
using System.Threading.Tasks;
using System.Diagnostics;
using Mcv.PluginV2;

namespace NicoSitePlugin.Metadata
{
    class MetaProvider
    {
        public event EventHandler<IMetaMessage> Received;
        Websocket _ws;
        private readonly ILogger _logger;

        public async Task ReceiveAsync(string websocketUrl)
        {
            if (_ws != null)
            {
                return;
            }
            _ws = new Websocket();
            _ws.Received += Ws_Received;
            _ws.Opened += Ws_Opened;
            try
            {
                await _ws.ReceiveAsync(websocketUrl);
            }
            finally
            {
                _ws.Received -= Ws_Received;
                _ws = null;
            }
        }

        private void Ws_Opened(object sender, EventArgs e)
        {
            var s = "{\"type\":\"startWatching\",\"data\":{\"stream\":{\"quality\":\"abr\",\"protocol\":\"hls\",\"latency\":\"low\",\"chasePlay\":false},\"room\":{\"protocol\":\"webSocket\",\"commentable\":true},\"reconnect\":false}}";
            try
            {
                _ws.Send(s);
            }
            catch (Exception ex)
            {

            }
        }
        public void Send(IMetaMessage message)
        {
            Send(message.Raw);
        }
        public void Send(string message)
        {
            _ws?.Send(message);
        }
        private void Ws_Received(object sender, string e)
        {
            var raw = e;
            Debug.WriteLine(raw);
            try
            {
                var message = MetaParser.Parse(raw);
                Received?.Invoke(this, message);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        public void Disconnect()
        {
            _ws?.Disconnect();
        }
        public MetaProvider(ILogger logger)
        {
            _logger = logger;
        }
    }
}