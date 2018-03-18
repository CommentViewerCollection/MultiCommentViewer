using System;
using Common;
using SitePlugin;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Linq;

namespace TwicasSitePlugin
{
    class MessageProvider
    {
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
        public async Task ConnectAsync(string broadcasterId)
        {
            _cts = new CancellationTokenSource();
            //TODO:try-catch
            var liveInfo = await API.GetLiveContext(_server, broadcasterId);
            var cnum = liveInfo.MovieCnum;
            var live_id = liveInfo.MovieId;
            long lastCommentId = 0;
            //TODO:try-catch
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
            //Disconnect()が呼ばれた場合以外は接続し続ける。
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var streamChecker = await API.GetUtreamChecker(_server, broadcasterId);
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
                            Received?.Invoke(this, dataCollection);
                            MetaReceived?.Invoke(this, new Metadata
                            {
                                CurrentViewers = streamChecker.CurrentViewers.ToString(),
                                TotalViewers = streamChecker.TotalViewers.ToString()
                            });
                            lastCommentId = dataCollection[dataCollection.Count - 1].Id;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Infoでエラー内容を通知。ただし同じエラーが連続する場合は通知しない
                    _logger.LogException(ex);
                }
                try
                {
                    await Task.Delay(1000 * 4, _cts.Token);
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
        private readonly CookieContainer _cc;
        private readonly ILogger _logger;
        public MessageProvider(IDataServer server, CookieContainer cc, ILogger logger)
        {
            _server = server;
            _cc = cc;
            _logger = logger;
        }
    }
}
