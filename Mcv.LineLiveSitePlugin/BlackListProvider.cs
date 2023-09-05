using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LineLiveSitePlugin
{
    internal interface IBlackListProvider
    {
        event EventHandler<long[]> Received;
        Task ReceiveAsync(List<Cookie> cookies);
        void Disconnect();
    }
    class BlackListProvider:IBlackListProvider
    {
        private readonly IDataServer _server;
        public event EventHandler<long[]> Received;
        CancellationTokenSource _cts;
        public async Task ReceiveAsync(List<Cookie> cookies)
        {
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                var blockList = await Api.GetBlockList(_server, cookies);
                Received?.Invoke(this, blockList);
                try
                {
                    await Task.Delay(60 * 1000, _cts.Token);
                }
                catch (TaskCanceledException) { break; }
            }
        }
        public void Disconnect()
        {
            _cts?.Cancel();
        }
        public BlackListProvider(IDataServer server)
        {
            _server = server;
        }
    }
}
