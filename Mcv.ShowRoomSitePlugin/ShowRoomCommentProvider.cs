using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Mcv.PluginV2;
using Mcv.PluginV2.AutoReconnector;
using System.Net;

namespace ShowRoomSitePlugin
{
    internal class ShowRoomCommentProvider : CommentProviderBase
    {
        private void MessageProvider_Received(object sender, IInternalMessage e)
        {

        }
        protected override void BeforeConnect()
        {
            base.BeforeConnect();
        }
        protected override void AfterDisconnected()
        {
            base.AfterDisconnected();

            _cts = new CancellationTokenSource();
        }
        CancellationTokenSource _cts = new CancellationTokenSource();
        private async Task ConnectInternalAsync(string input, List<Cookie> cookies)
        {
            var roomUrlKey = GetRoomUrlKeyFromInput(input);

            //直近の過去コメントを取得する
            var st = await ShowRoom.Api.Status.GetStatusAsync(_server, roomUrlKey);
            var commentLog = await ShowRoom.Api.CommentLog.GetCommentLogAsync(_server, st.RoomId);
            foreach (var comment in Enumerable.Reverse(commentLog.Comments))
            {
                ProcessT1(comment, true);
            }

            while (true)
            {
                RaiseMetadataUpdated(new Metadata
                {
                    Title = "（次の配信が始まるまで待機中...）",
                });
                var status = await WaitForLiveAsync(roomUrlKey, 30, _cts);
                if (status == null)
                {
                    RaiseMetadataUpdated(new Metadata
                    {
                        Title = "-",
                    });
                    return;
                }
                RaiseMetadataUpdated(new Metadata
                {
                    Title = status.RoomName,
                });
                try
                {
                    await _messageProvider.ReceiveAsync(status.BroadcastHost, status.BroadcastKey);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="roomUrlKey"></param>
        /// <param name="intervalSec"></param>
        /// <param name="cts"></param>
        /// <returns>ctsがCancel()された場合nullを返す</returns>
        private async Task<ShowRoom.Api.Status> WaitForLiveAsync(string roomUrlKey, int intervalSec, CancellationTokenSource cts)
        {
            while (true)
            {
                var status = await ShowRoom.Api.Status.GetStatusAsync(_server, roomUrlKey);
                if (status.IsLive)
                {
                    return status;
                }
                try
                {
                    await Task.Delay(intervalSec * 1000, cts.Token);
                }
                catch (TaskCanceledException) { }
                if (cts.IsCancellationRequested)
                {
                    return null;
                }
            }
        }
        private void ProcessT1(T1 t1, bool isInitialComment)
        {
            if (_siteOptions.IsIgnore50Counts && int.TryParse(t1.Comment, out int count))
            {
                if (count >= 1 && count <= 50)
                {
                    //1-50の数字のみのコメントは無視する
                    return;
                }
            }
            var message = new ShowRoomComment(t1);
            var userId = message.UserId;
            var isFirstComment = _first.IsFirstComment(userId);
            var name = MessagePartFactory.CreateMessageItems(message.UserName);
            RaiseMessageReceived(new MessageContext(message, userId, null));
        }
        private void ProcessInternalMessage(IInternalMessage e)
        {
            switch (e)
            {
                case T1 t1:
                    {
                        ProcessT1(t1, false);
                    }
                    break;
            }
        }
        private void MessageProvider_Received1(object sender, IInternalMessage e)
        {
            ProcessInternalMessage(e);
        }

        private void Me_SystemInfoReiceved(object sender, SystemInfoEventArgs e)
        {
            SendSystemInfo(e.Message, e.Type);
        }
        private string GetRoomUrlKeyFromInput(string input)
        {
            //https://www.showroom-live.com/r/1077f4117133
            var match = Regex.Match(input, "showroom-live.com/r/([0-9a-zA-Z_]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        private string GetRoomNameFromInput(string input)
        {
            var match = Regex.Match(input, "showroom-live.com/([^/?#]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        public override async Task ConnectAsync(string input, List<Cookie> cookies)
        {
            BeforeConnect();
            try
            {
                await ConnectInternalAsync(input, cookies);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "", $"input={input}");
            }
            finally
            {
                AfterDisconnected();
            }
        }
        FirstCommentDetector _first = new FirstCommentDetector();
        public override void Disconnect()
        {
            _cts?.Cancel();
        }
        public override async Task PostCommentAsync(string text)
        {
            await Task.FromResult<object>(null);
        }
        public override async Task<ICurrentUserInfo> GetCurrentUserInfo(List<Cookie> cookies)
        {
            var userInfo = new CurrentUserInfo
            {

            };
            return await Task.FromResult(userInfo);
        }

        public override void SetMessage(string raw)
        {
            throw new NotImplementedException();
        }

        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly IShowRoomSiteOptions _siteOptions;
        private readonly MessageProvider _messageProvider;
        public ShowRoomCommentProvider(IDataServer server, ILogger logger, IShowRoomSiteOptions siteOptions)
            : base(logger)
        {
            _server = server;
            _logger = logger;
            _siteOptions = siteOptions;
            _messageProvider = new MessageProvider(new Websocket(), _logger);
            _messageProvider.Received += MessageProvider_Received1;
        }

    }
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
