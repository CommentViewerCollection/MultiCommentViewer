﻿using Common;
using SitePlugin;
using System;
using System.Net;
using System.Threading.Tasks;
using SitePluginCommon;
using System.Text.RegularExpressions;
using NicoSitePlugin.Websocket;
using System.Collections.Generic;

namespace NicoSitePlugin
{
    class NewLiveInternalProvider : CommentProviderInternalBase
    {
        private readonly IDataSource _server;

        public override void BeforeConnect()
        {
            IsConnected = true;
        }

        public override void AfterDisconnected()
        {
            IsConnected = false;
            _elapsedTimer.Enabled = false;
            _keepSeatTimer.Enabled = false;
        }
        public bool IsConnected { get; protected set; }
        public override async Task ConnectAsync(string input, CookieContainer cc)
        {
            BeforeConnect();
            var match = Regex.Match(input, "(lv\\d+)");
            if (!match.Success)
            {
                //throw 
                AfterDisconnected();
                return;
            }
            var liveId = match.Groups[1].Value;
            try
            {
                _watchDataProps = await API.GetWatchDataProps(_server, liveId, cc).ConfigureAwait(false);
            }
            catch (MembersOnlyCommunityException)
            {
                SendSystemInfo("メンバー限定コミュニティのためコメントを取得できませんでした", InfoType.Error);
                AfterDisconnected();
                return;
            }
            var url = _watchDataProps.WebSocketUrl;
            var metaTask = _metaProvider.ReceiveAsync(url, _watchDataProps.BroadcastId);

            await _tcs.Task;
            var messageTask = _messageProvider.ReceiveAsync(_messageUrl, _threadId);

            var tasks = new List<Task>();
            tasks.Add(metaTask);
            tasks.Add(messageTask);
            while (tasks.Count > 0)
            {
                var t = await Task.WhenAny(tasks);
                if (t == metaTask)
                {
                    try
                    {
                        await metaTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    tasks.Remove(metaTask);
                }
                else
                {
                    _metaProvider.Disconnect();
                    _messageProvider.Disconnect();
                    try
                    {
                        await metaTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    tasks.Remove(metaTask);
                    try
                    {
                        await messageTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    tasks.Remove(messageTask);
                }
            }
            AfterDisconnected();
            return;
        }
        string _messageUrl;
        string _threadId;
        string _roomName;
        string _ticket;
        WatchDataProps _watchDataProps;
        TaskCompletionSource<object> _tcs = new TaskCompletionSource<object>();
        public override void Disconnect()
        {
            _messageProvider?.Disconnect();
        }

        public override bool IsValidInput(string input)
        {
            return Regex.IsMatch(input, "lv\\d+");
        }
        MetaProvider _metaProvider;
        MessageProvider _messageProvider;
        public bool IsLoggedIn { get; set; }
        public NewLiveInternalProvider(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager, ILogger logger, IDataSource server)
            : base(options, siteOptions, userStoreManager, server, logger)
        {
            _server = server;
            _metaProvider = new MetaProvider();
            _metaProvider.MessageReceived += MetaProvider_MessageReceived;

            _messageProvider = new MessageProvider();
            _messageProvider.ChatReceived += MessageProvider_ChatReceived;
            _messageProvider.ThreadReceived += MessageProvider_ThreadReceived;
        }

        private void MessageProvider_ThreadReceived(object sender, IThread e)
        {
            if (!string.IsNullOrEmpty(e.Ticket))
            {
                _ticket = e.Ticket;
            }
        }

        private async void MessageProvider_ChatReceived(object sender, ReceivedChat e)
        {
            var chat = e.Message;
            var isInitialComment = e.IsInitialComment;

            var roomName = _roomName;
            var context = await CreateMessageContextAsync(chat, roomName, isInitialComment);
            RaiseMessageReceived(context);
        }
        System.Timers.Timer _keepSeatTimer = new System.Timers.Timer();
        System.Timers.Timer _elapsedTimer = new System.Timers.Timer();
        DateTime? _startedAt;

        private void MetaProvider_MessageReceived(object sender, IInternalMessage e)
        {
            switch (e)
            {
                case RoomInternalMessage room:
                    {
                        _messageUrl = room.MessageServerUrl;
                        _threadId = room.ThreadId;
                        _roomName = room.RoomName;
                        _tcs.SetResult(null);
                    }
                    break;
                case StatisticsInternalMessage stat:
                    //stat.Viewers
                    RaiseMetadataUpdated(new Metadata
                    {
                         TotalViewers = stat.Viewers.ToString(),
                          Others = $"広告P:{stat.AdPoint} ギフトP:{stat.GiftPoints}",
                    });
                    break;
                case SeatInternalMessage seat:
                    var intervalSec = seat.KeepIntervalSec;
                    _keepSeatTimer.Interval = intervalSec * 1000;
                    _keepSeatTimer.AutoReset = true;
                    _keepSeatTimer.Elapsed += _keepSeatTimer_Elapsed;
                    _keepSeatTimer.Enabled = true;
                    break;
                case ScheduleInternalMessage schedule:
                    _startedAt = DateTime.Parse(schedule.Begin);
                    _elapsedTimer.Interval = 500;
                    _elapsedTimer.Elapsed += _elapsedTimer_Elapsed;
                    _elapsedTimer.AutoReset = true;
                    _elapsedTimer.Enabled = true;
                    break;
                case DisconnectInternalMessage disconnect:
                    Disconnect();
                    break;
            }

        }

        private void _elapsedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_startedAt.HasValue)
            {
                RaiseMetadataUpdated(new Metadata
                {
                    Elapsed = SitePluginCommon.Utils.ElapsedToString(DateTime.Now - _startedAt.Value),
                });
            }
        }

        private void _keepSeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _metaProvider.SendKeepSeat();
        }

        public override Task PostCommentAsync(string comment, string mail)
        {
            //[{"ping":{"content":"rs:1"}},{"ping":{"content":"ps:5"}},{"chat":{"thread":"1651612445","vpos":356587,"mail":"184 ","ticket":"0x3d4e000","user_id":"2297426","premium":1,"content":"？","postkey":".1559405184.b-Obktn_YDybdhLR9Hf9Pa17c4g"}},{"ping":{"content":"pf:5"}},{"ping":{"content":"rf:1"}}]

            var threadId = _threadId;
            var vpos = "";
            var ticket = _ticket;
            var userId = "";
            var premium = "";
            var postkey = "";

            _messageProvider.PostComment(threadId, vpos, ticket, userId, premium, postkey, mail, comment);
            return Task.CompletedTask;
        }
        public override void SetMessage(string raw)
        {
            _messageProvider.SetMessage(raw);
        }
    }
}
