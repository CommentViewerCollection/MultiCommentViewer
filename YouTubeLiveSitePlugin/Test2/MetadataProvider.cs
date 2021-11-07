using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;
using Common;
using System.Threading;
using System.Diagnostics;
using System.Net.Http;
using SitePluginCommon;
using Newtonsoft.Json;
using YouTubeLiveSitePlugin.Next;
using ryu_s.YouTubeLive.Message;

namespace YouTubeLiveSitePlugin.Test2
{
    internal class InfoData
    {
        public string Comment { get; set; }
        public InfoType Type { get; set; }
    }
    /// <summary>
    /// youtubeiを使ってメタデータを取得するクラス
    /// </summary>
    class MetaDataYoutubeiProvider : IMetadataProvider
    {
        private readonly IYouTubeLiveServer _server;

        public override async Task ReceiveAsync(YtCfg ytCfg, string vid, CookieContainer cc)
        {
            if (_cts != null)
            {
                throw new InvalidOperationException("receiving");
            }
            _cts = new CancellationTokenSource();
            try
            {
                await ReceiveInternalAsync(ytCfg, vid, cc);
            }
            finally
            {
                _cts = null;
            }
        }
        public async Task ReceiveInternalAsync(YtCfg ytCfg, string vid, CookieContainer cc)
        {
            var url = "https://www.youtube.com/youtubei/v1/updated_metadata?alt=json&key=" + ytCfg.InnertubeApiKey;
            var payload = "{\"context\":{\"client\":{\"hl\":\"ja\",\"gl\":\"JP\",\"clientName\":1,\"clientVersion\":\"2.20210114.08.00\",\"screenDensityFloat\":\"1.25\"}},\"videoId\":\"" + vid + "\"}";
            //var payloadBytes = Encoding.UTF8.GetBytes(payload);

            //var wc = new MyWebClient(cc);

            while (!_cts.IsCancellationRequested)
            {
                int timeoutMs = 0;
                //wc.Headers["origin"] = "https://www.youtube.com";
                //wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                //wc.Headers[HttpRequestHeader.Accept] = "*/*";

                try
                {
                    var res = await GetMetadata(cc, url, payload);

                    dynamic json = JsonConvert.DeserializeObject(res);
                    if (!json.ContainsKey("continuation"))
                    {
                        SendInfo($"updated_metadataにcontinuationが無い", InfoType.Debug);
                        throw new ContinuationNotExistsException(res);
                    }
                    if (json.continuation.ContainsKey("invalidationContinuationData"))
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
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var httpRes = (HttpWebResponse)ex.Response;
                    var code = httpRes.StatusCode;
                    _logger.LogException(ex, "", $"url={url}");
                    SendInfo($"メタデータの取得でエラーが発生 ({code})", InfoType.Notice);
                }
                catch (WebException ex)
                {
                    SendInfo($"メタデータの取得でエラーが発生 ({ex.Status})", InfoType.Notice);
                    Debug.WriteLine(ex.Message);
                }
                catch (ParseException ex)
                {
                    _logger.LogException(ex);
                }
                catch(ContinuationNotExistsException ex)
                {
                    _logger.LogException(ex);
                    break;
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

        protected virtual async Task<string> GetMetadata(CookieContainer cc, string url, string payload)
        {
            return await _server.PostAsync(new HttpOptions { Url = url, Cc = cc }, new StringContent(payload, Encoding.UTF8, "application/json"));
        }

        public MetaDataYoutubeiProvider(IYouTubeLiveServer server, ILogger logger) : base(logger)
        {
            _server = server;
        }
    }
    /// <summary>
    /// service_ajaxでメタデータを取得するクラス
    /// </summary>
    class MetadataProvider : IMetadataProvider
    {
        private readonly IYouTubeLiveServer _server;

        public override async Task ReceiveAsync(YtCfg ytCfg, string vid, CookieContainer cc)
        {
            _cts = new CancellationTokenSource();
            var token = ytCfg.XsrfToken;
            //このAPIを呼び出すとき、Cookieに"YSC"と"VISITOR_INFO1_LIVE"が必須。
            //2つとも配信ページか $"https://www.youtube.com/live_chat?v={vid}&is_popout=1"にアクセスした時にCookieをセットしなければもらえる。
            //コメビュではtokenとかを取得するために"/live_chat"に必ずアクセスする必要があるため、未ログイン時にはそれで取得した値を使えば良い。
            var url = "https://www.youtube.com/service_ajax?name=updatedMetadataEndpoint";
            //この値接続毎に固定っぽい。continuationも変わらない気がする。
            var sej = "{\"clickTrackingParams\":\"CIQBEMyrARgAIhMIrdnBx6fm2wIVQ5VYCh14dQoMKPgd\",\"commandMetadata\":{\"webCommandMetadata\":{\"url\":\"/service_ajax\",\"sendPost\":true}},\"updatedMetadataEndpoint\":{\"videoId\":\"" + vid + "\"}}";
            var data = new Dictionary<string, string>
            {
                {"sej", sej},
                //{"csn", "E90sW4DVBdaP4ALlho8Q" },
                {"session_token", token },
            };
            var encodedData = string.Join("&", data.Select(kv => kv.Key + "=" + HttpUtility.UrlEncode(kv.Value)));//HttpUtility.UrlEncode()の戻り値は小文字だけど問題ない。
            string encodedContinuation = "";
            bool isFatalError = false;//続行不能なエラーが出た場合にtrueにする。
            while (!_cts.IsCancellationRequested)
            {
                int timeoutMs = 0;
                try
                {
                    var res = await GetMetadata(cc, url, encodedContinuation + encodedData);
                    dynamic d = JsonConvert.DeserializeObject(res);
                    string continuation;

                    if (!d.ContainsKey("code") || d.code != "SUCCESS")
                    {
                        if (res == "{\"errors\":[\"Invalid Request\"]}")
                        {
                            isFatalError = true;
                        }
                        var v = string.Join("&", data.Select(kv => kv.Key + "=" + kv.Value));
                        //{"code":"ERROR","error":"不明なエラーです。"}
                        //{"code":"ERROR","error":"この機能は現在利用できません。しばらくしてからもう一度お試しください。"}
                        throw new ParseException($"res={res},ytCfg={ytCfg},encoded={v}");
                    }
                    if (d.data.continuation.ContainsKey("invalidationContinuationData"))
                    {
                        throw new ParseException(res);
                    }
                    else
                    {
                        continuation = d.data.continuation.timedContinuationData.continuation;
                        //if (!data.ContainsKey("continuation"))
                        //{
                        //    data.Add("continuation", continuation);
                        //}
                        //else
                        //{
                        //    data["continuation"] = continuation;
                        //}
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
                catch (HttpRequestException ex)
                {
                    _logger.LogException(ex);
                    SendInfo($"メタデータの取得でエラーが発生 ({ex.Message})", InfoType.Notice);
                    break;
                }
                catch (HttpException ex)
                {
                    _logger.LogException(ex);
                    SendInfo($"メタデータの取得でエラーが発生 ({ex.Message})", InfoType.Notice);
                    break;
                }
                catch (ParseException ex)
                {
                    _logger.LogException(ex);
                    if (isFatalError)
                    {
                        SendInfo("メタデータの取得がキャンセルされました（Invalid Request）", InfoType.Notice);
                        break;
                    }
                }
                catch (TaskCanceledException ex)
                {
                    SendInfo("メタデータの取得がキャンセルされました（" + ex.Message + "）", InfoType.Notice);
                    break;
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

        protected virtual async Task<string> GetMetadata(CookieContainer cc, string url, string data)
        {
            return await _server.PostAsync(new HttpOptions
            {
                Url = url,
                Cc = cc,
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.120 Safari/537.36",

            }, new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded"));
        }

        public MetadataProvider(IYouTubeLiveServer server, ILogger logger) : base(logger)
        {
            _server = server;
        }
    }
}
