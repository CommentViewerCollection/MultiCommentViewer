using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using Common;

namespace MixchSitePlugin
{
    class MixchWebsocket : IMixchWebsocket
    {
        private Websocket _websocket;
        private readonly ILogger _logger;
        //public event EventHandler<IMixchCommentData> CommentReceived;
        public event EventHandler<IPacket> Received;

        public async Task ReceiveAsync(string userId, string userAgent, List<Cookie> cookies)
        {
            var origin = "https://mixch.tv";

            _websocket = new Websocket();
            _websocket.Received += Websocket_Received;
            _websocket.Opened += Websocket_Opened;

            var url = $"wss://chat.mixch.tv/torte/room/{userId}";
            await _websocket.ReceiveAsync(url, userAgent, origin);
            //切断後処理
            _heartbeatTimer.Enabled = false;

        }

        private void Websocket_Opened(object sender, EventArgs e)
        {
            _heartbeatTimer.Enabled = true;
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
        public MixchWebsocket(ILogger logger)
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
