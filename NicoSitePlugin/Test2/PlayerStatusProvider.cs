using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
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
    class ChannelCommunityPlayerStatusProvider : IPlayerStatusProvider
    {
        public event EventHandler<IPlayerStatus> Received;
        private async Task UploadStringAsync(string s, string filename)
        {
            var fileStreamContent = new StreamContent(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(s)));
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                var userAgent = "NicoSitePlugin";
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                formData.Add(fileStreamContent, "ps", filename);
                var response =await client.PostAsync("http://int-main.net/upload/playerstatus", formData);
                if (response.IsSuccessStatusCode)
                {
                }
                else
                {
                }
            }
        }
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
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var res = await API.GetPlayerStatusAsync(_nicoServer, _liveId, _cc);
                    if (res.Success)
                    {
                        var ps = res.PlayerStatus;
                        Received?.Invoke(this, ps);
                        if (ps.MsList.Length > 0)
                        {
                            try
                            {
                                await UploadStringAsync(ps.Raw, _liveId + ".txt");
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        //何もしなくていい気がする。切断とかは上位層に任せればいい
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                    try
                    {
                        var myServerRes = await API.GetPlayerStatusFromUrlAsync(_nicoServer, "http://int-main.net/api/playerstatus/" + _liveId, _cc);
                        if (myServerRes.Success)
                        {
                            Received?.Invoke(this, myServerRes.PlayerStatus);
                        }
                        else
                        {
                            //何もしなくていい気がする。切断とかは上位層に任せればいい
                        }
                    }
                    catch (Exception ex)
                    {

                    }

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
