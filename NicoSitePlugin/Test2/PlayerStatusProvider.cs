using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NicoSitePlugin.Old;

namespace NicoSitePlugin.Test2
{
    internal interface IPlayerStatusProvider
    {
        void Disconnect();
        Task ReceiveAsync();
        event EventHandler<Old.IPlayerStatus> Received;
    }
    class ChannelPlayerStatusProvider : IPlayerStatusProvider
    {
        public event EventHandler<IPlayerStatus> Received;

        public void Disconnect()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        CancellationTokenSource _cts;
        public async Task ReceiveAsync()
        {
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(500, _cts.Token);
                }
                catch (TaskCanceledException) { break; }
            }
            _cts = null;
        }
    }
    class CommunityPlayerStatusProvider : IPlayerStatusProvider
    {
        public event EventHandler<IPlayerStatus> Received;

        public void Disconnect()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        CancellationTokenSource _cts;
        public async Task ReceiveAsync()
        {
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(500, _cts.Token);
                }
                catch (TaskCanceledException) { break; }
            }
            _cts = null;
        }
    }
    class OfficialPlayerStatusProvider : IPlayerStatusProvider
    {
        public event EventHandler<IPlayerStatus> Received;

        public void Disconnect()
        {
            if(_cts != null)
            {
                _cts.Cancel();
            }
        }
        CancellationTokenSource _cts;
        public async Task ReceiveAsync()
        {
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(500, _cts.Token);
                }
                catch (TaskCanceledException) { break; }
            }
            _cts = null;
        }
    }
    internal interface IMyServer
    {
        Task Upload(Old.IPlayerStatus ps);
        Task<Old.IPlayerStatus> GetPlayerStatus(string live_id);
    }
    internal interface INicoServer
    {

    }
}
