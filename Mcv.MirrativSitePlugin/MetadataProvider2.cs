using Mcv.PluginV2.AutoReconnection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MirrativSitePlugin
{
    class MetadataProvider2 : IProvider
    {
        private readonly IDataServer _server;
        private readonly IMirrativSiteOptions _siteOptions;
        public event EventHandler<ILiveInfo> MetadataUpdated;
        bool _isDisconnectRequested;
        CancellationTokenSource _cts;

        public IProvider Master { get; set; }
        public bool IsFinished { get; private set; }
        public Task Work { get; private set; }
        public ProviderFinishReason FinishReason { get; private set; }
        public string LiveId { get; set; }
        public void Start()
        {
            Work = ReceiveAsync();
        }
        private async Task ReceiveAsync()
        {
            FinishReason = ProviderFinishReason.Unknown;
            _isDisconnectRequested = false;
            _cts = new CancellationTokenSource();

            while (true)
            {
                if (_isDisconnectRequested)
                {
                    FinishReason = ProviderFinishReason.ByStopMethod;
                    break;
                }
                var liveInfo = await GetLiveInfoAsync();
                MetadataUpdated?.Invoke(this, liveInfo);
                await Wait();
            }
        }

        protected virtual async Task<ILiveInfo> GetLiveInfoAsync()
        {
            return await Api.PollLiveAsync(_server, LiveId);
        }

        protected virtual async Task Wait()
        {
            try
            {
                await Task.Delay(_siteOptions.PollingIntervalSec * 60 * 1000, _cts.Token);
            }
            catch (TaskCanceledException) { }
        }

        public void Stop()
        {
            _isDisconnectRequested = true;
            _cts?.Cancel();
        }
        public MetadataProvider2(IDataServer server, IMirrativSiteOptions siteOptions)
        {
            _server = server;
            _siteOptions = siteOptions;
        }
    }
}
