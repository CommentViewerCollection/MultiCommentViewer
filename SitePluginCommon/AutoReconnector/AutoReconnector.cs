using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitePluginCommon.AutoReconnector
{
    public class AutoReconnector
    {
        private readonly IConnector _connector;
        private readonly MessageUntara _message;

        protected virtual DateTime GetCurrentDateTime() => DateTime.Now;
        public async Task AutoConnect()
        {
            DateTime? beforeTrialTime = null;
            SendSystemInfo("接続しました", InfoType.Notice);
            while (true)
            {
                var disconnectReason = DisconnectReason.Unknown;
                if (await _connector.IsLivingAsync())
                {
                    beforeTrialTime = GetCurrentDateTime();
                    disconnectReason = await _connector.ConnectAsync();
                }
                else
                {
                    SendSystemInfo("配信が終了しました", InfoType.Notice);
                    break;
                }
                if (disconnectReason == DisconnectReason.User)
                {
                    SendSystemInfo("切断しました", InfoType.Notice);
                    break;
                }
                if (disconnectReason == DisconnectReason.Finished)
                {
                    SendSystemInfo("配信が終了しました", InfoType.Notice);
                    break;
                }
                //再接続が、暴走しないようにインターバルを設ける。
                if (beforeTrialTime.HasValue)
                {
                    var elapsed = GetCurrentDateTime() - beforeTrialTime.Value;
                    if (elapsed < new TimeSpan(0, 0, ReconnectionIntervalMinimamSec))
                    {
                        var waitTime = new TimeSpan(0, 0, ReconnectionIntervalMinimamSec) - elapsed;
                        await Task.Delay(waitTime);
                    }
                }
            }
        }
        private void SendSystemInfo(string message, InfoType type)
        {
            _message.Set(message, type);
        }
        /// <summary>
        /// 前回接続試行時から最低限経過しているべき秒数
        /// </summary>
        public int ReconnectionIntervalMinimamSec { get; set; } = 5;
        public AutoReconnector(IConnector connector, MessageUntara message)
        {
            _connector = connector;
            _message = message;
        }

        public void Disconnect()
        {
            _connector.Disconnect();
        }
    }
}
