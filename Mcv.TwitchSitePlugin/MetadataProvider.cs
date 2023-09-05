using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchSitePlugin
{
    class MetadataProvider : IMetadataProvider
    {
        private readonly IDataServer _server;
        private readonly ITwitchSiteOptions _siteOptions;
        private readonly string _channelName;
        public event EventHandler<Stream>? MetadataUpdated;

        public MetadataProvider(IDataServer server, ITwitchSiteOptions siteOptions, string channelName)
        {
            _server = server;
            _siteOptions = siteOptions;
            _channelName = channelName;
        }
        public async Task ReceiveAsync()
        {
            _isDisconnectRequested = false;
            _cts = new CancellationTokenSource();

            while (true)
            {
                if (_isDisconnectRequested)
                {
                    break;
                }
                //var liveInfo = await API.GetStreamAsync(_server, _channelName);
                //if (liveInfo != null)
                //{
                //    MetadataUpdated?.Invoke(this, liveInfo);
                //}
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
        CancellationTokenSource? _cts;
    }
}
