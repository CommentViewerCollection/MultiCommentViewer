using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using System.Net;
using Codeplex.Data;
using System.Web;
using System.Text.RegularExpressions;
using Common;
using System.Threading;
using System.Diagnostics;

namespace YouTubeLiveSitePlugin.Test2
{
    class MetadataProvider
    {
        private readonly ILogger _logger;

        public event EventHandler<IMetadata> MetadataReceived;
        public async Task ReceiveAsync(string ytCfg, string vid, CookieContainer cc)
        {
            _cts = new CancellationTokenSource();
            string encodedToken;
            try
            {
                var ytCfgJson = DynamicJson.Parse(ytCfg);
                var token = ytCfgJson.XSRF_TOKEN;
                encodedToken = HttpUtility.UrlEncode(token);
            }
            catch (Exception ex)
            {
                //TODO:metadataが一切取れない致命的なエラーだということを表したい
                throw new ParseException("", ex);
            }
            var url = "https://www.youtube.com/service_ajax?name=updatedMetadataEndpoint";
            var wc = new MyWebClient(cc);
            //この値接続毎に固定っぽい。continuationも変わらない気がする。
            var encoded = "sej=%7B%22clickTrackingParams%22%3A%22CIIBEMyrARgAIhMI5-DT8siv2QIVGrFYCh1xsQuhKPgd%22%2C%22commandMetadata%22%3A%7B%22webCommandMetadata%22%3A%7B%22url%22%3A%22%2Fservice_ajax%22%2C%22sendPost%22%3Atrue%7D%7D%2C%22updatedMetadataEndpoint%22%3A%7B%22videoId%22%3A%22" + vid + "%22%7D%7D&csn=iX-JWqf4Mpri4gLx4q6ICg&session_token=" + encodedToken;

            string encodedContinuation = "";
            while (!_cts.IsCancellationRequested)
            {
                int timeoutMs = 0;
                try
                {
                    wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.167 Safari/537.36";
                    wc.Headers["origin"] = "https://www.youtube.com";
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Headers[HttpRequestHeader.Accept] = "*/*";
                    var bytes = await wc.UploadDataTaskAsync(url, Encoding.UTF8.GetBytes(encodedContinuation + encoded));//continuationが無くても正常に取得できる。でも一応付けておく。
                    var res = Encoding.UTF8.GetString(bytes);
                    var d = DynamicJson.Parse(res);
                    string continuation;
                    
                    if (d.code != "SUCCESS")
                    {
                        //{"code":"ERROR","error":"不明なエラーです。"}
                        throw new Exception(res);
                    }
                    if (d.data.continuation.IsDefined("invalidationContinuationData"))
                    {
                        throw new ParseException(res);
                    }
                    else
                    {
                        continuation = d.data.continuation.timedContinuationData.continuation;
                        encodedContinuation = "continuation=" + HttpUtility.UrlEncode(continuation) + "&";
                        timeoutMs = (int)d.data.continuation.timedContinuationData.timeoutMs;
                    }
                    var actions = d.data.actions;
                    var metadata = new Metadata();
                    foreach (var action in actions)
                    {
                        if (action.IsDefined("updateViewershipAction"))
                        {
                            var re = action.updateViewershipAction.viewership.videoViewCountRenderer;
                            if (re.IsDefined("isLive"))
                            {
                                var isLive = re.isLive;
                                metadata.IsLive = isLive;
                            }
                            if (re.IsDefined("viewCount"))//viewCountが存在しない場合があった
                            {
                                var lowViewCount = (string)re.viewCount.runs[0].text;
                                metadata.CurrentViewers = new string(lowViewCount.Where(char.IsDigit).ToArray());
                            }
                        }
                        else if (action.IsDefined("updateTitleAction"))
                        {
                            var title = action.updateTitleAction.title.simpleText;
                            metadata.Title = title;
                        }
                        else if (action.IsDefined("updateDescriptionAction"))
                        {

                        }
                    }
                    MetadataReceived?.Invoke(this, metadata);
                }
                catch(WebException ex)
                {
                    if (ex.Response is HttpWebResponse http)
                    {
                        throw;
                    }
                    Debug.WriteLine(ex.Message);
                }
                catch(ParseException ex)
                {
                    _logger.LogException(ex);
                }
                catch (Exception ex)
                {
                    var parseEx = new ParseException("", ex);
                    _logger.LogException(parseEx);
                }
                timeoutMs = timeoutMs == 0 ? 1000 : timeoutMs;
                try
                {
                    await Task.Delay(timeoutMs, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
            _cts = null;
            return;
        }
        CancellationTokenSource _cts;
        public void Disconnect()
        {
            if(_cts != null)
            {
                _cts.Cancel();
            }
        }
        public MetadataProvider(ILogger logger)
        {
            _logger = logger;
        }
    }
}
