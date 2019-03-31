using System;
using Common;
using SitePlugin;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.Concurrent;
using SitePluginCommon;

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
        public event EventHandler<IMetadata> MetaReceived;
        public event EventHandler<IMessageContext> MessageReceived;
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
        private readonly ConcurrentDictionary<string, int> _userCommentCountDict = new ConcurrentDictionary<string, int>();
        private TwicasMessageContext CreateMessageContext(LowObject.Comment lowComment, bool isInitialComment, string raw) 
        {
            var commentData = Tools.Parse(lowComment);
            var userId = commentData.UserId;
            bool isFirstComment;
            if (_userCommentCountDict.ContainsKey(userId))
            {
                _userCommentCountDict[userId]++;
                isFirstComment = false;
            }
            else
            {
                _userCommentCountDict.AddOrUpdate(userId, 1, (s, n) => n);
                isFirstComment = true;
            }
            var user = _userStore.GetUser(userId);

            var message = new TwicasComment(raw)
            {
                CommentItems = commentData.Message,
                Id = commentData.Id.ToString(),
                NameItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(commentData.Name) },
                PostTime = commentData.Date.ToString("HH:mm:ss"),
                UserId = commentData.UserId,
                UserIcon = new MessageImage
                {
                    Url = commentData.ThumbnailUrl,
                    Alt = null,
                    Height = commentData.ThumbnailHeight,
                    Width = commentData.ThumbnailWidth,
                 },
            };
            var metadata = new MessageMetadata(message, _options, _siteOptions, user, _cp, isFirstComment)
            {
                IsInitialComment = isInitialComment,
            };
            var methods = new TwicasMessageMethods();
            var messageContext = new TwicasMessageContext(message, metadata, methods);
            return messageContext;
        }
        private void SendInfo(string message, InfoType type)
        {
            InfoOccured?.Invoke(this, new InfoData { Message = message, Type = type });
        }
        System.Collections.Concurrent.ConcurrentBag<string> _receivedItemIds;
        FirstCommentDetector _first = new FirstCommentDetector();
        public async Task ConnectAsync(string broadcasterId, int cnum,long live_id)
        {
            _first.Reset();
            _cts = new CancellationTokenSource();
            _receivedItemIds = new System.Collections.Concurrent.ConcurrentBag<string>();
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
                    foreach(var lowComment in initialComments)
                    {
                        //showがfalseのデータが時々ある。
                        //{"id":15465669455,"show":false}
                        //よく分からないけど、有用な情報は無さそうだからスルー
                        if (!lowComment.show) continue;

                        var context = CreateMessageContext(lowComment, true, initialRaw);
                        MessageReceived?.Invoke(this, context);
                    }
                    //var initialDataList = LowComment2Data(initialComments, initialRaw);
                    //if (initialDataList.Count > 0)
                    //{
                    //    InitialCommentsReceived?.Invoke(this, initialDataList);
                    //}
                    var lastComment = initialComments[initialComments.Length - 1];
                    lastCommentId = lastComment.id;
                }
            }
            catch(HttpRequestException ex)
            {
                _logger.LogException(ex);
                string message;
                if (ex.InnerException != null)
                {
                    message = ex.InnerException.Message;
                }
                else
                {
                    message = ex.Message;
                }
                SendInfo(message, InfoType.Debug);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                SendInfo(ex.Message, InfoType.Debug);
            }
            //Disconnect()が呼ばれた場合以外は接続し続ける。
            while (!_cts.IsCancellationRequested)
            {
                var waitTimeMs = 1000 * _siteOptions.CommentRetrieveIntervalSec;
                var accWaitTime = 0;
                string lastItemId = null;
                try
                {
                    var (streamChecker, streamCheckerRaw) = await API.GetUtreamChecker(_server, broadcasterId, lastItemId).ConfigureAwait(false);
                    if(streamChecker.Items != null && streamChecker.Items.Count > 0)
                    {
#if DEBUG
                        try
                        {
                            using (var sw = new System.IO.StreamWriter("アイテムあり.txt", true))
                            {
                                sw.WriteLine(streamCheckerRaw);
                            }
                        }
                        catch (Exception) { }
#endif
                        var lastItem = streamChecker.Items[streamChecker.Items.Count - 1];
                        var lastItemIdBefore = lastItemId == null ? 0 : long.Parse(lastItemId);
                        lastItemId = Math.Max(lastItemIdBefore, long.Parse(lastItem.Id)).ToString();
                    }
                    MetaReceived?.Invoke(this, new Metadata
                    {
                        Title = streamChecker.Telop,
                        CurrentViewers = streamChecker.CurrentViewers.ToString(),
                        TotalViewers = streamChecker.TotalViewers.ToString()
                    });
                    foreach (var item in streamChecker.Items)
                    {
                        try
                        {
                            if (_receivedItemIds.Contains(item.Id))
                                continue;

                            ITwicasItem itemMessage = null;
                            if (Tools.IsKiitos(item))
                            {
                                itemMessage = Tools.CreateKiitosMessage(item);
                            }
                            else
                            {
                                var image = new MessageImage
                                {
                                    Url = item.ItemImage,
                                    Alt = item.t13,
                                    Height = 40,
                                    Width = 40,
                                };
                                itemMessage = new TwicasItem(item.Raw)
                                {
                                    UserIcon = new MessageImage
                                    {
                                        Url = item.SenderImage,
                                        Alt = item.t13,
                                        Height = 40,
                                        Width = 40,
                                    },
                                    ItemName = item.t13,
                                    //CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(item.t13) },
                                    CommentItems = new List<IMessagePart> { image },
                                    NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(item.t12) },
                                    UserId = item.SenderName,
                                    ItemId = item.Id,
                                };
                            }
                            if (itemMessage != null)
                            {
                                var user = _userStore.GetUser(item.SenderName);
                                var metadata = new MessageMetadata(itemMessage, _options, _siteOptions, user, _cp, false);
                                var methods = new TwicasMessageMethods();
                                var context = new TwicasMessageContext(itemMessage, metadata, methods);
                                MessageReceived?.Invoke(this, context);
                            }
                            SendInfo(item.SenderName + " " + item.ItemImage, InfoType.Debug);
                            _receivedItemIds.Add(item.Id);
                        }
                        catch(ParseException ex)
                        {
                            _logger.LogException(ex);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogException(ex);
                        }
                    }

                    if (streamChecker.LiveId == null)
                    {
                        //放送してない。live_idは更新しない。
                    }
                    else
                    {
                        live_id = streamChecker.LiveId.Value;
                        SendInfo($"Twicas live_id={live_id}", InfoType.Debug);
                    }
                    var (lowComments, newCnum, updateRaw) = await API.GetListUpdate(_server, broadcasterId, live_id, cnum, lastCommentId, _cc);
                    if (lowComments != null && lowComments.Count > 0)
                    {
                        cnum = newCnum;


                        lastCommentId = lowComments[lowComments.Count - 1].id;
                        var eachInterval = waitTimeMs / lowComments.Count;
                        foreach (var lowComment in lowComments)
                        {
                            //showがfalseのデータが時々ある。
                            //{"id":15465669455,"show":false}
                            //よく分からないけど、有用な情報は無さそうだからスルー
                            if (!lowComment.show) continue;

                            var context = CreateMessageContext(lowComment, false, updateRaw);
                            MessageReceived?.Invoke(this, context);

                            await Task.Delay(eachInterval);
                            accWaitTime += eachInterval;
                        }
                        ////htmlが""のことがある。コメントを削除した？省いておく
                        //var dataCollection = LowComment2Data(lowComments, updateRaw);//.Where(s=>!string.IsNullOrEmpty(s.html)).Select(Tools.Parse).ToList();
                        //if (dataCollection.Count > 0)
                        //{
                        //    lastCommentId = dataCollection[dataCollection.Count - 1].Id;

                        //    var eachInterval = waitTimeMs / dataCollection.Count;
                        //    foreach (var data in dataCollection)
                        //    {
                        //        Received?.Invoke(this, new List<ICommentData> { data });

                        //        await Task.Delay(eachInterval);
                        //        accWaitTime += eachInterval;
                        //    }
                        //}
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
        private readonly ITwicasSiteOptions _siteOptions;
        private readonly CookieContainer _cc;
        private readonly IUserStore _userStore;
        private readonly ICommentOptions _options;
        private readonly ICommentProvider _cp;
        private readonly ILogger _logger;
        public MessageProvider(IDataServer server, ITwicasSiteOptions siteOptions, CookieContainer cc,IUserStore userStore,ICommentOptions options,ICommentProvider cp, ILogger logger)
        {
            _server = server;
            _siteOptions = siteOptions;
            _cc = cc;
            _userStore = userStore;
            _options = options;
            _cp = cp;
            _logger = logger;
        }
    }
}
