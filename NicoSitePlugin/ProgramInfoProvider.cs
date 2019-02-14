using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
namespace NicoSitePlugin
{
    class ProgramInfoProvider
    {
        CancellationTokenSource _cts;
        const int _interval = 5 * 60 * 1000;
        private readonly IDataSource _dataSource;
        private readonly string _liveId;
        private readonly CookieContainer _cc;
        public event EventHandler<IProgramInfo> ProgramInfoReceived;
        public async Task ReceiveAsync()
        {
            if(_cts != null)
            {
                throw new InvalidOperationException();
            }
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                var programInfo = await API.GetProgramInfo(_dataSource, _liveId, _cc);
                ProgramInfoReceived?.Invoke(this, programInfo);
                try
                {
                    await Task.Delay(_interval, _cts.Token);
                }
                catch (TaskCanceledException) { break; }
            }
            _cts = null;
        }
        public void Disconnect()
        {
            if(_cts != null)
            {
                _cts.Cancel();
            }
        }
        public ProgramInfoProvider(IDataSource dataSource,string liveId,CookieContainer cc)
        {
            _dataSource = dataSource;
            _liveId = liveId;
            _cc = cc;
        }
    }
}
