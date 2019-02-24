using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace LineLiveSitePlugin
{
    class PromptyStatsProvider
    {
        private readonly IDataServer _server;
        public event EventHandler<IPromptyStats> Received;
        CancellationTokenSource _cts;
        public async Task ReceiveAsync(string channelId, string liveId)
        {
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                var promptyStats = await Api.GetPromptyStats(_server, channelId, liveId);
                Received?.Invoke(this, promptyStats);
                try
                {
                    await Task.Delay(10 * 1000, _cts.Token);
                }
                catch (TaskCanceledException) { break; }
            }
        }
        public void Disconnect()
        {
            _cts?.Cancel();
        }
        public PromptyStatsProvider(IDataServer server)
        {
            _server = server;
        }
    }
}
