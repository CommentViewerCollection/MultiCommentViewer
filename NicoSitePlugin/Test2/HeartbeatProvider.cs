using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NicoSitePlugin.Test2
{

    [Serializable]
    public class HeartbeartFailedException : Exception
    {
        public string Code { get; }
        public HeartbeartFailedException(string code)
        {
            Code = code;
        }
    }
    /// <summary>
    /// 定期的にHeartbeatを送信する
    /// </summary>
    class HeartbeatProvider
    {
        private readonly Old.IDataSource _dataSource;
        CancellationTokenSource _cts;
        public async Task ReceiveAsync(string live_id, CookieContainer cc)
        {
            _cts = new CancellationTokenSource();
            int waitSec;
            while (!_cts.IsCancellationRequested)
            {
                var res = await Old.API.GetHeartbeatAsync(_dataSource, live_id, cc);
                if (res.Success)
                {
                    var heartbeart = res.Heartbeat;
                    waitSec = int.Parse(heartbeart.WaitTime);
                }
                else
                {
                    var fail = res.Fail;
                    throw new HeartbeartFailedException(fail.Code);
                }
                try
                {
                    await Task.Delay(waitSec * 1000, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
        public void Disconnect()
        {
            if(_cts != null)
            {
                _cts.Cancel();
            }
        }
        public HeartbeatProvider(Old.IDataSource dataSource)
        {
            _dataSource = dataSource;
        }
    }
}
