using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System.Threading;
using SitePlugin;
using Common;
using System.Diagnostics;

namespace YouTubeLiveSitePlugin.Test2
{
    class ChatProvider
    {
        public event EventHandler<List<CommentData>> ActionsReceived;
        public event EventHandler<InfoData> InfoReceived;
        CancellationTokenSource _cts;
        private readonly IYouTubeLibeServer _server;
        private readonly ILogger _logger;
        public int IntervalAfterWebException { get; set; } = 5000;
        private void SendInfo(string message, InfoType type)
        {
            InfoReceived?.Invoke(this, new InfoData { Comment = message, Type = type });
        }
        public void Disconnect()
        {
            if(_cts != null)
            {
                _cts.Cancel();
            }
        }
        public async Task ReceiveAsync(string vid,IContinuation initialContinuation, CookieContainer cc)
        {
            _cts = new CancellationTokenSource();

            var continuation = initialContinuation;
            while (!_cts.IsCancellationRequested)
            {
                var getLiveChatUrl = $"https://www.youtube.com/live_chat/get_live_chat?continuation={System.Web.HttpUtility.UrlEncode( continuation.Continuation)}&pbj=1";
                //var wc = new MyWebClient(cc);
                var ua="Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.2924.87 Safari/537.36";
                string getLiveChatJson = null;
                try
                {
                    var getLiveChatBytes = await _server.GetBytesAsync(getLiveChatUrl);
                    getLiveChatJson = Encoding.UTF8.GetString(getLiveChatBytes);
                    var (c, a, sessionToken) = Tools.ParseGetLiveChat(getLiveChatJson);
                    continuation = c;
                    if (a.Count > 0)
                    {
                        if (c is ITimedContinuation timed)
                        {
                            var interval = c.TimeoutMs / a.Count;
                            foreach (var action in a)
                            {
                                ActionsReceived?.Invoke(this, new List<CommentData> { action });
                                await Task.Delay(interval, _cts.Token);
                            }
                        }
                        else
                        {
                            ActionsReceived?.Invoke(this, a);
                            await Task.Delay(1000, _cts.Token);
                        }
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                    
                }
                catch(WebException ex) when(ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var httpRes = (HttpWebResponse)ex.Response;
                    var code = httpRes.StatusCode;
                    //
                    SendInfo(ex.Message, InfoType.Debug);

                    if (code == HttpStatusCode.BadRequest || code == HttpStatusCode.ServiceUnavailable)
                    {
                        //回復の見込みが無いと判断
                        _logger.LogException(ex, "", $"vid={vid},url={getLiveChatUrl},getLiveChatJson={getLiveChatJson}");
                        break;
                    }
                    else
                    {
                        await Task.Delay(IntervalAfterWebException);
                    }
                }
                catch(WebException ex)
                {
                    _logger.LogException(ex, "", $"vid={vid},url={getLiveChatUrl},getLiveChatJson={getLiveChatJson}");
                    Debug.WriteLine(ex.Message);
                    SendInfo(ex.Message, InfoType.Debug);
                    await Task.Delay(IntervalAfterWebException);
                }
                catch(ParseException ex)
                {
                    _logger.LogException(ex, "get_live_chatのパースに失敗", getLiveChatJson);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (ContinuationContentsNullException)
                {
                    //放送終了
                    break;
                }
                catch (ContinuationNotExistsException)
                {
                    break;
                }
            }
        }
        public ChatProvider(IYouTubeLibeServer server, ILogger logger)
        {
            _server = server;
            _logger = logger;
        }
    }
    class CommentData
    {
        public string Raw { get; set; }
        public bool IsPaidMessage => !string.IsNullOrEmpty(PurchaseAmount);
        public string PurchaseAmount { get; set; }
        public List<IMessagePart> MessageItems { get; set; }
        public List<IMessagePart> NameItems { get; set; }
        public string UserId { get; set; }
        /// <summary>
        /// UNIX Time（マイクロ秒）
        /// </summary>
        public long TimestampUsec { get; internal set; }
        public string Id { get; internal set; }
        public IMessageImage Thumbnail { get; set; }
    }
    interface IContinuation
    {
        string Continuation { get; }
        int TimeoutMs { get; }
    }
    interface ITimedContinuation: IContinuation
    {
    }
    interface IInvalidationContinuation : IContinuation
    {
        int ObjectSource { get; }
        string ProtoCreationTimestampMs { get; }
        string ObjectId { get; }
    }
    public class TimedContinuation : ITimedContinuation
    {
        public string Continuation { get; set; }
        public int TimeoutMs { get; set; }
    }
    public class InvalidationContinuation : IInvalidationContinuation
    {
        public string Continuation { get; set; }
        public int TimeoutMs { get; set; }
        public int ObjectSource { get; set; }
        public string ProtoCreationTimestampMs { get; set; }
        public string ObjectId { get; set; }
    }
    class Photo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Url { get; set; }
    }
    public class Badge
    {
        public string Url { get; set; }
        public string Alt { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
