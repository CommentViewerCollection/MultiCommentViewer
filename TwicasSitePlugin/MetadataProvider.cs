using Common;
using SitePlugin;
using SitePluginCommon.AutoReconnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TwicasSitePlugin
{
    class InternalItem
    {
        public string Raw { get; }
        public string ImageUrl { get; }
        public string Name { get; }
        public string Message { get; }
        public string Id { get; }
        public string UserId { get; }
        public string UserName { get; }
        public string ScreenName { get; }
        public string UserImageUrl { get; }
        public InternalItem(Item item)
        {
            Raw = item.Raw;
            if (Tools.IsKiitos(item))
            {
                Name = "キートス";
                Id = item.Id;
                UserId = item.t10;
                UserName = item.SenderName;
                ScreenName = item.t12;
                Message = $"[{item.t13}] {item.Message}{Environment.NewLine}";
                ImageUrl = item.ItemImage;
                UserImageUrl = item.SenderImage;
            }
            else
            {
                Name = item.t13;
                Id = item.Id;
                UserId = item.t10;
                UserName = item.SenderName;
                ScreenName = item.t12;
                Message = item.Message;
                ImageUrl = item.ItemImage;
                UserImageUrl = item.SenderImage;
            }
        }
    }
    class MetadataProvider : IProvider
    {
        public IProvider Master { get; }
        public bool IsFinished { get; }
        public Task Work { get; private set; }
        public ProviderFinishReason FinishReason { get; }
        CancellationTokenSource _cts;
        private readonly ILogger _logger;
        private readonly IDataServer _server;
        private readonly string _broadcasterId;
        private readonly MessageUntara _messenger;
        System.Collections.Concurrent.ConcurrentBag<string> _receivedItemIds;
        public event EventHandler<Metadata> MetadataReceived;
        public event EventHandler<InternalItem> ItemReceived;
        public event EventHandler<long?> LiveIdReceived;
        public int CommentRetrieveIntervalSec { get; set; } = 4;
        private async Task ReceiveAsync()
        {
            if (_cts != null)
            {
                throw new InvalidOperationException();
            }
            _cts = new CancellationTokenSource();
            _receivedItemIds = new System.Collections.Concurrent.ConcurrentBag<string>();
            while (!_cts.IsCancellationRequested)
            {
                var waitTimeMs = 1000 * CommentRetrieveIntervalSec;
                var accWaitTime = 0;
                string lastItemId = null;
                try
                {
                    var (streamChecker, streamCheckerRaw) = await API.GetStreamChecker(_server, _broadcasterId, lastItemId).ConfigureAwait(false);
                    if (streamChecker.Items != null && streamChecker.Items.Count > 0)
                    {
                        var lastItem = streamChecker.Items[streamChecker.Items.Count - 1];
                        var lastItemIdBefore = lastItemId == null ? 0 : long.Parse(lastItemId);
                        lastItemId = Math.Max(lastItemIdBefore, long.Parse(lastItem.Id)).ToString();
                    }
                    MetadataReceived?.Invoke(this, new Metadata
                    {
                        Title = streamChecker.Telop,
                        CurrentViewers = streamChecker.CurrentViewers.ToString(),
                        TotalViewers = streamChecker.TotalViewers.ToString(),
                        IsLive = streamChecker.LiveId.HasValue,
                        LiveId = streamChecker.LiveId,
                    });
                    foreach (var item in streamChecker.Items)
                    {
                        try
                        {
                            if (_receivedItemIds.Contains(item.Id))
                                continue;
                            _receivedItemIds.Add(item.Id);
                            ItemReceived?.Invoke(this, new InternalItem(item));
                        }
                        catch (ParseException ex)
                        {
                            _logger.LogException(ex);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogException(ex);
                        }
                    }
                    LiveIdReceived?.Invoke(this, streamChecker.LiveId);
                }
                catch (HttpRequestException ex)
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
                catch (ParseException ex)
                {
                    _logger.LogException(ex);
                    SendInfo(ex.Message, InfoType.Debug);
                }
                catch (TaskCanceledException)
                {
                    break;
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
                catch (TaskCanceledException)
                {
                    break;
                }
            }
            _cts = null;
        }
        private void SendInfo(string message, InfoType type)
        {
            _messenger.Set(message, type);
        }
        public void Start()
        {
            Work = ReceiveAsync();
        }

        public void Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        public MetadataProvider(ILogger logger, IDataServer server, string broadcasterId, MessageUntara messenger)
        {
            _logger = logger;
            _server = server;
            _broadcasterId = broadcasterId;
            _messenger = messenger;
        }
    }
}
