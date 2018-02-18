using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using System.Net;
using Codeplex.Data;
using System.Web;
namespace YouTubeLiveSitePlugin.Test2
{
    class MetadataProvider
    {
        public event EventHandler<IMetadata> MetadataReceived;
        public async Task ReceiveAsync(string ytCfg, string vid, CookieContainer cc)
        {
            var ytCfgJson = DynamicJson.Parse(ytCfg);
            var token = ytCfgJson.XSRF_TOKEN;
            var encodedToken = HttpUtility.UrlEncode(token);

            var url = "https://www.youtube.com/service_ajax?name=updatedMetadataEndpoint";
            var wc = new MyWebClient(cc);
            //この値接続毎に固定っぽい。continuationも変わらない気がする。
            var encoded = "sej=%7B%22clickTrackingParams%22%3A%22CIIBEMyrARgAIhMI5-DT8siv2QIVGrFYCh1xsQuhKPgd%22%2C%22commandMetadata%22%3A%7B%22webCommandMetadata%22%3A%7B%22url%22%3A%22%2Fservice_ajax%22%2C%22sendPost%22%3Atrue%7D%7D%2C%22updatedMetadataEndpoint%22%3A%7B%22videoId%22%3A%22" + vid + "%22%7D%7D&csn=iX-JWqf4Mpri4gLx4q6ICg&session_token=" + encodedToken;

            string encodedContinuation = "";
            while (true)
            {
                wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.167 Safari/537.36";
                wc.Headers["origin"] = "https://www.youtube.com";
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                wc.Headers[HttpRequestHeader.Accept] = "*/*";
                var bytes = await wc.UploadDataTaskAsync(url, Encoding.UTF8.GetBytes(encodedContinuation + encoded));//continuationが無くても正常に取得できる。でも一応付けておく。
                var res = Encoding.UTF8.GetString(bytes);
                var d = DynamicJson.Parse(res);
                string continuation;
                int timeoutMs;
                if (d.code != "SUCCESS")
                {
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
                        var isLive = re.isLive;
                        metadata.IsLive = isLive;
                        var viewCount = re.viewCount.runs[0].text;//～人が視聴中
                        metadata.CurrentViewers = viewCount;
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
                await Task.Delay(timeoutMs);
            }
            return;
        }
        public void Disconnect()
        {

        }
        public MetadataProvider()
        {

        }
    }
}
