using System;
using Common;
using SitePlugin;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Linq;
using System.Net.Http;

namespace TwicasSitePlugin
{
    class InfoData
    {
        public string Message { get; set; }
        public InfoType Type { get; set; }
    }
    class MessageProvider
    {
        public event EventHandler<InfoData> InfoOccured;
        public event EventHandler<IEnumerable<ICommentData>> InitialCommentsReceived;
        public event EventHandler<IEnumerable<ICommentData>> Received;
        public event EventHandler<IMetadata> MetaReceived;
        private List<ICommentData> LowComment2Data(IEnumerable<LowObject.Comment> lows, string raw)
        {
            var initialDataList = new List<ICommentData>();
            foreach (var c in lows)
            {
                if (string.IsNullOrEmpty(c.uid))
                {
                    continue;
                }
                try
                {
                    if (c.@class != "other")
                    {
                        _logger.LogException(new ParseException("キートス候補" + raw));
                    }
                    var data = Tools.Parse(c);
                    initialDataList.Add(data);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            }
            return initialDataList;
        }
        private void SendInfo(string message, InfoType type)
        {
            InfoOccured?.Invoke(this, new InfoData { Message = message, Type = type });
        }
        public async Task ConnectAsync(string broadcasterId, int cnum,long live_id)
        {
            _cts = new CancellationTokenSource();
            //TODO:try-catch
            //var liveInfo = await API.GetLiveContext(_server, broadcasterId);
            //var cnum = liveInfo.MovieCnum;
            //var live_id = liveInfo.MovieId;
            long lastCommentId = 0;
            try
            {
                var (initialComments, initialRaw) = await API.GetListAll(_server, broadcasterId, live_id, lastCommentId, 0, 20, _cc);
                if (initialComments.Length > 0)
                {
                    var initialDataList = LowComment2Data(initialComments, initialRaw);
                    if (initialDataList.Count > 0)
                    {
                        InitialCommentsReceived?.Invoke(this, initialDataList);
                    }
                    var lastComment = initialComments[initialComments.Length - 1];
                    lastCommentId = lastComment.id;
                }
            }
            catch (Exception ex)
            {
                SendInfo(ex.Message, InfoType.Debug);
            }
            //Disconnect()が呼ばれた場合以外は接続し続ける。
            while (!_cts.IsCancellationRequested)
            {
                var waitTimeMs = 1000 * _siteOptions.CommentRetrieveIntervalSec;
                var accWaitTime = 0;
                try
                {
                    var (streamChecker, streamCheckerRaw) = await API.GetUtreamChecker(_server, broadcasterId).ConfigureAwait(false);
                    if (streamChecker.LiveId == null)
                    {
                        //放送してない。live_idは更新しない。
                    }
                    else
                    {
                        live_id = streamChecker.LiveId.Value;
                    }
                    var (lowComments, newCnum, updateRaw) = await API.GetListUpdate(_server, broadcasterId, live_id, cnum, lastCommentId, _cc);
                    if (lowComments != null && lowComments.Count > 0)
                    {
                        cnum = newCnum;
                        //htmlが""のことがある。コメントを削除した？省いておく
                        var dataCollection = LowComment2Data(lowComments, updateRaw);//.Where(s=>!string.IsNullOrEmpty(s.html)).Select(Tools.Parse).ToList();
                        if (dataCollection.Count > 0)
                        {
                            MetaReceived?.Invoke(this, new Metadata
                            {
                                CurrentViewers = streamChecker.CurrentViewers.ToString(),
                                TotalViewers = streamChecker.TotalViewers.ToString()
                            });
                            lastCommentId = dataCollection[dataCollection.Count - 1].Id;

                            var eachInterval = waitTimeMs / dataCollection.Count;
                            foreach (var data in dataCollection)
                            {
                                Received?.Invoke(this, new List<ICommentData> { data });

                                await Task.Delay(eachInterval);
                                accWaitTime += eachInterval;
                            }
                        }
                    }
                }
                catch(HttpRequestException ex)
                {
                    _logger.LogException(ex);
                    string message;
                    if(ex.InnerException != null)
                    {
                        message = ex.InnerException.Message;
                    }
                    else
                    {
                        message = ex.Message;
                    }
                    SendInfo(message, InfoType.Debug);
                }
                catch(ParseException ex)
                {
                    _logger.LogException(ex);
                    SendInfo(ex.Message, InfoType.Debug);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                    //Infoでエラー内容を通知。ただし同じエラーが連続する場合は通知しない
                    SendInfo(ex.Message, InfoType.Debug);
                }
                try
                {
                    var restWait = waitTimeMs - accWaitTime;
                    if (restWait > 0)
                    {
                        await Task.Delay(restWait, _cts.Token);
                    }
                }
                catch(TaskCanceledException)
                {
                    break;
                }
            }
            _cts = null;
        }
        public void Disconnect()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        private CancellationTokenSource _cts;
        private readonly IDataServer _server;
        private readonly TwicasSiteOptions _siteOptions;
        private readonly CookieContainer _cc;
        private readonly ILogger _logger;
        public MessageProvider(IDataServer server,TwicasSiteOptions siteOptions, CookieContainer cc, ILogger logger)
        {
            _server = server;
            _siteOptions = siteOptions;
            _cc = cc;
            _logger = logger;
        }
    }
}
