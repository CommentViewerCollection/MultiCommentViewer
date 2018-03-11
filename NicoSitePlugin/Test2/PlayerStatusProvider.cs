using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NicoSitePlugin.Old;

namespace NicoSitePlugin.Test2
{
    internal interface IPlayerStatusProvider
    {
        void Disconnect();
        Task ReceiveAsync();
        event EventHandler<List<RoomInfo>> Received;
    }
    class ChannelCommunityPlayerStatusProvider : IPlayerStatusProvider
    {
        public event EventHandler<List<RoomInfo>> Received;
        public void Disconnect()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        CancellationTokenSource _cts;
        private readonly IDataSource _nicoServer;
        private readonly string _liveId;
        private readonly int _interval;
        private readonly CookieContainer _cc;

        public async Task ReceiveAsync()
        {
            var liveId = _liveId;
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                var programInfo = await API.GetProgramInfo(_nicoServer, liveId, _cc);
                var list = new List<RoomInfo>();
                foreach(var room in programInfo.data.rooms)
                {
                    var match = Regex.Match(room.xmlSocketUri.Replace("xmlsocket://", ""), "^(.+):(\\d+)$");
                    if (!match.Success)
                        continue;
                    var addr = match.Groups[1].Value;
                    var port = int.Parse(match.Groups[2].Value);
                    var ms = new MsTest(addr, port, room.threadId);
                    list.Add(new RoomInfo(ms, room.name));
                }
                Received?.Invoke(this, list);
                try
                {
                    await Task.Delay(_interval, _cts.Token);
                }
                catch (TaskCanceledException) { break; }
            }
            _cts = null;
        }
        public ChannelCommunityPlayerStatusProvider(IDataSource nicoServer, string liveId,int interval, CookieContainer cc)
        {
            _nicoServer = nicoServer;
            _liveId = liveId;
            _interval = interval;
            _cc = cc;
        }
    }
    class OfficialPlayerStatusProvider : IPlayerStatusProvider
    {
        public event EventHandler<List<RoomInfo>> Received;

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
}
