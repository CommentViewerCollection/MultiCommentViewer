using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Codeplex.Data;
using System.Web;
using System.Text.RegularExpressions;
using Common;
using System.Threading;
using System.Diagnostics;

namespace YouTubeLiveSitePlugin.Test2
{
    /// <summary>
    /// 
    /// </summary>
    class MetaDataYoutubeiProvider : IMetadataProvider
    {
        public override async Task ReceiveAsync(string ytCfg, string vid, CookieContainer cc)
        {
            _cts = new CancellationTokenSource();

            string innerTubeKey;
            try
            {
                var ytCfgJson = DynamicJson.Parse(ytCfg);
                innerTubeKey = ytCfgJson.INNERTUBE_API_KEY;
            }catch(Exception ex)
            {
                throw new FatalException("", ex);
            }
            var url = "https://www.youtube.com/youtubei/v1/updated_metadata?alt=json&key=" + innerTubeKey;
            var payload = "{\"context\":{\"client\":{\"hl\":\"ja\",\"gl\":\"JP\",\"clientName\":1,\"clientVersion\":\"1.20180224\",\"screenDensityFloat\":\"1.25\"}},\"videoId\":\"" + vid + "\"}";
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            var wc = new MyWebClient(cc);
            
            while (!_cts.IsCancellationRequested)
            {
                int timeoutMs = 0;
                wc.Headers["origin"] = "https://www.youtube.com";
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Headers[HttpRequestHeader.Accept] = "*/*";

                try
                {
                    var bytes = await wc.UploadDataTaskAsync(url, payloadBytes);
                    var res = Encoding.UTF8.GetString(bytes);

                    var json = DynamicJson.Parse(res);
                    if (json.continuation.IsDefined("invalidationContinuationData"))
                    {
                        throw new ParseException(res);
                    }
                    else
                    {
                        timeoutMs = (int)json.continuation.timedContinuationData.timeoutMs;
                    }
                    var metadata = ActionsToMetadata(json.actions);
                    metaReceived?.Invoke(this, metadata);
                }
                catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var httpRes = (HttpWebResponse)ex.Response;
                    var code = httpRes.StatusCode;
                    SendNotice($"メタデータの取得でエラーが発生 ({code})");
                }
                catch (WebException ex)
                {
                    SendNotice($"メタデータの取得でエラーが発生 ({ex.Status})");
                    Debug.WriteLine(ex.Message);
                }
                catch (ParseException ex)
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
        }
        public MetaDataYoutubeiProvider(ILogger logger):base(logger)
        {
        }
    }
    class MetadataProvider : IMetadataProvider
    {
        public override async Task ReceiveAsync(string ytCfg, string vid, CookieContainer cc)
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
                throw new FatalException("", ex);
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
                    //wc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.167 Safari/537.36";
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
                        throw new ParseException(res);
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
                    try
                    {
                        var metadata = ActionsToMetadata(d.data.actions);
                        metaReceived?.Invoke(this, metadata);
                    }
                    catch (Exception ex)
                    {
                        throw new ParseException(res, ex);
                    }
                }
                catch(WebException ex)when(ex.Status== WebExceptionStatus.ProtocolError)
                {
                    var httpRes = (HttpWebResponse)ex.Response;
                    var code = httpRes.StatusCode;
                    SendNotice($"メタデータの取得でエラーが発生 ({code})");
                }
                catch(WebException ex)
                {
                    SendNotice($"メタデータの取得でエラーが発生 ({ex.Status})");
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
        public MetadataProvider(ILogger logger):base(logger)
        {
        }
    }
}
