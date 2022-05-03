using Mcv.PluginV2;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WhowatchSitePlugin
{
    internal class InternalCommentProvider
    {
        public event EventHandler<IWhowatchMessage> MessageReceived;
        Websocket _mp;
        long _live_id;
        string _jwt;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="live_id"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public async Task ReceiveAsync(long live_id, string jwt)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                throw new ArgumentNullException(nameof(jwt));
            }

            _live_id = live_id;
            _jwt = jwt;

            _mp = new Websocket("wss://ws.whowatch.tv/socket/websocket?vsn=2.0.0");
            _mp.Opened += Mp_Opened;
            _mp.Received += Mp_Received;
            await _mp.ReceiveAsync();
            _heartBeatTimer.Enabled = false;
            _mp.Disconnect();
        }
        public void Disconnect()
        {
            _mp?.Disconnect();
        }
        private void Mp_Received(object sender, string e)
        {
            var rawMessage = e;
            try
            {
                var internalMessage = MessageParser.ParseRawString2InternalMessage(rawMessage);

                var raw = internalMessage.Raw;
                Debug.WriteLine(raw);

                if (internalMessage.InternalMessageType == WhowatchInternalMessageType.Shout)
                {
                    var message = MessageParser.ParseShoutMessage(internalMessage);
                    if (message is IWhowatchNgComment)
                    {
                        //NGコメント（"この投稿は視聴者には表示されません。"と表示されるだけ）は無視する
                        return;
                    }
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private System.Timers.Timer _heartBeatTimer = new System.Timers.Timer();
        ~InternalCommentProvider()
        {
            _heartBeatTimer.Elapsed -= HeartBeatTimer_Elapsed;
        }
        private async void HeartBeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var s = $"[null,\"{_ref++}\",\"phoenix\",\"heartbeat\",{{}}]";
                await _mp.SendAsync(s);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        int _ref = 1;
        private readonly ILogger _logger;

        private async void Mp_Opened(object sender, EventArgs e)
        {
            //["1","1","room:9150523","phx_join",{"p":"eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJtdXRlX2NoZWNrX3VzZXIiOi0xLCJ1c2VyX2lkIjoxNjc3Njg2NSwiaXNfcHVibGlzaGVyIjpmYWxzZSwiaXNfY29uY2VhbG1lbnQiOmZhbHNlfQ.toMPD_xwaCFPBQnR3pl4ilyCpyRB2SeAS_K3zE5iNg8"}]
            var joinMessageSent = false;
            try
            {
                var s = $"[\"1\",\"{_ref++}\",\"room:{_live_id}\",\"phx_join\",{{\"p\":\"{_jwt}\"}}]";
                await _mp.SendAsync(s);
                joinMessageSent = true;
                _heartBeatTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            if (!joinMessageSent)
            {
                _mp.Disconnect();
            }
        }
        public InternalCommentProvider(ILogger logger)
        {
            _logger = logger;
            _heartBeatTimer.Interval = 30 * 1000;//30sec
            _heartBeatTimer.Elapsed += HeartBeatTimer_Elapsed;
        }
    }
}
