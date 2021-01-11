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
using System.Net.Http;

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
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialContinuation"></param>
        /// <returns></returns>
        /// <exception cref="ReloadException"></exception>
        public async Task ReceiveAsync(string initialContinuation)
        {
            _cts = new CancellationTokenSource();

            //var continuation = initialContinuation.Continuation;
            var continuation = initialContinuation;
            while (!_cts.IsCancellationRequested)
            {
                var getLiveChatUrl = $"https://www.youtube.com/live_chat/get_live_chat?continuation={System.Web.HttpUtility.UrlEncode(continuation)}&pbj=1";
                string getLiveChatJson = null;
                try
                {
                    var getLiveChatBytes = await _server.GetBytesAsync(getLiveChatUrl);
                    getLiveChatJson = Encoding.UTF8.GetString(getLiveChatBytes);
                    var (c, a, sessionToken) = Tools.ParseGetLiveChat(getLiveChatJson);
                    continuation = c.Continuation;
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
                        else if (c is IInvalidationContinuation invalid)
                        {
                            ActionsReceived?.Invoke(this, a);
                            await Task.Delay(1000, _cts.Token);
                        }
                        else if (c is IReloadContinuation)
                        {
                            throw new ReloadException();
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        await Task.Delay(1000, _cts.Token);
                    }

                }
                catch (WebException ex)
                {
                    throw new ReloadException(ex);
                }
                catch (HttpRequestException ex)
                {
                    throw new ReloadException(ex);
                }
                catch (ParseException ex)
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
    interface IReloadContinuation : IContinuation
    {
    }
    interface ITimedContinuation : IContinuation
    {
    }
    interface IInvalidationContinuation : IContinuation
    {
        int ObjectSource { get; }
        string ProtoCreationTimestampMs { get; }
        string ObjectId { get; }
    }
    public class ReloadContinuation : IReloadContinuation
    {
        public string Continuation { get; set; }
        public int TimeoutMs { get; } = 0;
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
    public class UnknownContinuation : IContinuation
    {
        public string Continuation { get; } = "";
        public int TimeoutMs { get; } = 0;
        public UnknownContinuation(string raw)
        {
            Raw = raw;
        }

        public string Raw { get; }
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
