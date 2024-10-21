using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using Common;

namespace BLiveSitePlugin
{
    class BLiveWebsocket : IBLiveWebsocket
    {
        private Websocket _websocket;
        private readonly ILogger _logger;
        //public event EventHandler<IBLiveCommentData> CommentReceived;
        public event EventHandler<IPacket> Received;

        public async Task ReceiveAsync(string liveId, string userAgent, List<Cookie> cookies)
        {
            var cookieList = new List<KeyValuePair<string, string>>();

            var origin = "https://live.carol-i.com";

            _websocket = new Websocket();
            _websocket.Received += Websocket_Received;
            // liveIdを渡すためのラムダ式でOpenedイベントを設定
            _websocket.Opened += (sender, e) => Websocket_Opened(sender, e, liveId);

            var url = $"wss://live.carol-i.com:6001/socket.io/?EIO=3&transport=websocket";
            await _websocket.ReceiveAsync(url, cookieList, userAgent, origin);
            //切断後処理
            _heartbeatTimer.Enabled = false;

        }

        private void Websocket_Opened(object sender, EventArgs e, string liveId)
        {
            _heartbeatTimer.Enabled = true;

            // WebSocketが開かれた後にチャンネルにサブスクライブする
            string subscribeMessage = $"42[\"subscribe\", {{\"channel\": \"message.received.{liveId}\"}}]";
            Task.Run(async () => await SendAsync(subscribeMessage));
        }

        private void Websocket_Received(object sender, string e)
        {
            Debug.WriteLine(e);
            IPacket packet = null;
            try
            {
                packet = Packet.Parse(e);
            }
            catch (ParseException ex)
            {
                _logger.LogException(ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            if (packet == null)
                return;
            Received?.Invoke(this, packet);
        }

        public async Task SendAsync(IPacket packet)
        {
            if (packet is PacketPing ping)
            {
                await SendAsync(ping.Raw);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public async Task SendAsync(string s)
        {
            await _websocket.SendAsync(s);
        }
        System.Timers.Timer _heartbeatTimer = new System.Timers.Timer();
        public BLiveWebsocket(ILogger logger)
        {
            _logger = logger;
            _heartbeatTimer.Interval = 25 * 1000;
            _heartbeatTimer.Elapsed += _heartBeatTimer_Elapsed;
            _heartbeatTimer.AutoReset = true;
        }
        private async void _heartBeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                await SendAsync(new PacketPing());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void Disconnect()
        {
            _websocket.Disconnect();
        }
    }
}
