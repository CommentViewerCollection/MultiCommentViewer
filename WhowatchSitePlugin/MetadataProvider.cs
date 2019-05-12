using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WhowatchSitePlugin
{
    interface IMetadataProvider
    {

    }
    class MetadataProvider : IMetadataProvider
    {
        private readonly IDataServer _server;
        private readonly IWhowatchSiteOptions _siteOptions;
        public event EventHandler<LiveData> MetadataUpdated;

        public MetadataProvider(IDataServer server, IWhowatchSiteOptions siteOptions)
        {
            _server = server;
            _siteOptions = siteOptions;
        }
        public async Task ReceiveAsync(long live_id, long lastUpdatedAt, CookieContainer cc)
        {
            long lua = lastUpdatedAt;
            _isDisconnectRequested = false;
            _cts = new CancellationTokenSource();

            while (true)
            {
                if (_isDisconnectRequested)
                {
                    break;
                }
                var liveData = await Api.GetLiveDataAsync(_server, live_id, lua, cc);
                lua = liveData.UpdatedAt;
                MetadataUpdated?.Invoke(this, liveData);
                try
                {
                    await Task.Delay(_pollingIntervalSec * 1000, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
        const int _pollingIntervalSec = 30;
        bool _isDisconnectRequested;
        public void Disconnect()
        {
            _isDisconnectRequested = true;
            _cts?.Cancel();
        }
        CancellationTokenSource _cts;
    }
}
