using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;

namespace NicoSitePlugin
{
    class TestCommentProvider : CommentProviderBase, INicoCommentProvider
    {
        private readonly ILogger _logger;
        private readonly IUserStoreManager _userStoreManager;
        private readonly INicoSiteOptions _siteOptions;
        private readonly IDataSource _server;
        private string GetVid(string input)
        {
            var match = Regex.Match(input, "(lv\\d+)");
            if (!match.Success) return null;
            return match.Groups[1].Value;
        }
        private DataProps ExtractDataProps(string livePagehtml)
        {
            var match = Regex.Match(livePagehtml, "<script [^>]+ data-props=\"([^>]+)\"></script>");
            if (!match.Success) return null;
            var pre = match.Groups[1].Value;
            var dataPropsJson = pre.Replace("&quot;", "\"");
            var dataProps = new DataProps(dataPropsJson);
            return dataProps;
        }
        public override async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            BeforeConnect();
            try
            {
                await ConnectInternalAsync(input, browserProfile);
            }
            finally
            {
                AfterDisconnected();
            }
        }
        protected override void AfterDisconnected()
        {
            _chatProvider = null;
            _dataProps = null;
            base.AfterDisconnected();
        }
        Metadata.MetaProvider _metaProvider;
        private CookieContainer GetCookieContainer(IBrowserProfile browserProfile)
        {
            return GetCookieContainer(browserProfile, "nicovideo.jp");
        }
        public async Task ConnectInternalAsync(string input, IBrowserProfile browserProfile)
        {
            var cc = GetCookieContainer(browserProfile);
            var vid = GetVid(input);
            var url = "https://live2.nicovideo.jp/watch/" + vid;
            _metaProvider = new Metadata.MetaProvider();
            _metaProvider.Received += MetaProvider_Received;
            _chatProvider = new Chat.ChatProvider();
            _chatProvider.Received += ChatProvider_Received;

            var liveHtml = await _server.GetAsync(url, cc);
            _dataProps = ExtractDataProps(liveHtml);
            if (_dataProps == null)
            {
                throw new SpecChangedException("data-propsが無い");
            }
            if (_dataProps.Status == "ENDED")
            {
                SendSystemInfo("この番組は終了しました", InfoType.Notice);
                return;
            }
            RaiseMetadataUpdated(new TestMetadata
            {
                Title = _dataProps.Title,
            });

            var metaTask = _metaProvider.ReceiveAsync(_dataProps.WebsocketUrl);
            _tasks.Add(metaTask);
            _mainLooptcs = new TaskCompletionSource<object>();
            _tasks.Add(_mainLooptcs.Task);

            while (_tasks.Count > 1)//1の場合は_mainLooptcs.Taskだからループを終了する
            {
                var t = await Task.WhenAny(_tasks);
                if (t == _mainLooptcs.Task)
                {
                    _tasks.Remove(_mainLooptcs.Task);
                    _tasks.AddRange(_toAdd);
                    _mainLooptcs = new TaskCompletionSource<object>();
                    _tasks.Add(_mainLooptcs.Task);
                }
                else if (t == metaTask)
                {
                    try
                    {
                        await metaTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    _tasks.Remove(metaTask);
                }
                else//roomTask
                {
                    try
                    {
                        await metaTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    _tasks.Remove(metaTask);
                    try
                    {
                        await t;
                    }
                    catch (Exception ex)
                    {

                    }
                    _tasks.Clear();//本当はchatのTaskだけ取り除きたいけど、変数に取ってなくて無理だから全部消しちゃう
                }
                if (_tasks.Count <= 1)//_mainLooptcsはあっても良い。
                {
                    break;
                }
            }
            _metaProvider.Received -= MetaProvider_Received;
            _chatProvider.Received -= ChatProvider_Received;
            return;
        }
        /// <summary>
        /// 初期コメント取得中か
        /// </summary>
        private bool _isInitialCommentsReceiving;

        private void ProcessChatMessage(Chat.IChatMessage message)
        {
            switch (message)
            {
                case Chat.ChatMessage chat:
                    {
                        var userId = chat.UserId;
                        var user = GetUser(userId);

                        //var comment = await Tools.CreateNicoComment(chat, user, _siteOptions, roomName, async userid => await API.GetUserInfo(_dataSource, userid), _logger);
                        var comment = new NicoComment("")
                        {
                            ChatNo = chat.No,
                            Id = chat.No.ToString(),
                            Is184 = chat.Anonymity == 1,
                            PostedAt = Common.UnixTimeConverter.FromUnixTime(chat.Date),
                            Text = chat.Content,
                            UserId = chat.UserId,
                            UserName = "",
                        };
                        bool isFirstComment = false;
                        //if (_userCommentCountDict.ContainsKey(userId))
                        //{
                        //    _userCommentCountDict[userId]++;
                        //    isFirstComment = false;
                        //}
                        //else
                        //{
                        //    _userCommentCountDict.AddOrUpdate(userId, 1, (s, n) => n);
                        //    isFirstComment = true;
                        //}
                        //message = comment;
                        var metadata = new CommentMessageMetadata(comment, _options, _siteOptions, user, this, isFirstComment)
                        {
                            IsInitialComment = _isInitialCommentsReceiving,
                            SiteContextGuid = SiteContextGuid,
                        };
                        var context = new NicoMessageContext(comment, metadata, new NicoMessageMethods());
                        RaiseMessageReceived(context);
                    }
                    break;
                case Chat.Ping ping:
                    if (ping.Content == "rs:0")
                    {
                        _isInitialCommentsReceiving = true;
                    }
                    else if (ping.Content == "rf:0")
                    {
                        _isInitialCommentsReceiving = false;
                    }
                    break;
                case Chat.UnknownMessage unknown:
                    _logger.LogException(new ParseException(unknown.Raw));
                    break;
                default:
                    break;
            }
        }
        private void ChatProvider_Received(object sender, Chat.IChatMessage e)
        {
            var message = e;
            try
            {
                ProcessChatMessage(message);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }

        }

        readonly List<Task> _tasks = new List<Task>();
        readonly List<Task> _toAdd = new List<Task>();
        TaskCompletionSource<object> _mainLooptcs;
        Chat.ChatProvider _chatProvider;
        Metadata.Room _room;
        DataProps _dataProps;
        private void MetaProvider_Received(object sender, Metadata.IMetaMessage e)
        {
            var message = e;

            try
            {
                switch (message)
                {
                    case Metadata.Room room:
                        {
                            _room = room;
                            Chat.IChatOptions chatOptions;
                            if (Metadata.Room.IsLoggedIn(room))
                            {
                                chatOptions = new Chat.ChatLoggedInOptions
                                {
                                    Thread = room.ThreadId,
                                    ThreadKey = room.YourPostKey,
                                    UserId = _dataProps.UserId,
                                };
                            }
                            else
                            {
                                chatOptions = new Chat.ChatGuestOptions
                                {
                                    Thread = room.ThreadId,
                                    UserId = "guest",
                                };
                            }
                            var t = _chatProvider.ReceiveAsync(chatOptions);
                            _toAdd.Add(t);
                            _mainLooptcs.SetResult(null);
                        }
                        break;
                    case Metadata.Ping ping:
                        _metaProvider.Send(new Metadata.Pong());
                        break;
                    case Metadata.Statistics stat:
                        RaiseMetadataUpdated(new TestMetadata
                        {
                            TotalViewers = stat.Viewers.ToString(),
                            Others = $"コメント数:{stat.Comments} 広告ポイント:{stat.AdPoints} ギフトポイント:{stat.GiftPoints}",
                        });
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }

        public override void Disconnect()
        {
            _chatProvider?.Disconnect();
        }

        public override async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var cc = GetCookieContainer(browserProfile);
            var myInfo = await Api.GetMyInfo(_server, cc);
            return await Task.FromResult(new CurrentUserInfo { Username = myInfo.Nickname, IsLoggedIn = myInfo.IsLogin });
        }
        class CurrentUserInfo : ICurrentUserInfo
        {
            public string Username { get; set; }
            public bool IsLoggedIn { get; set; }
        }
        public override IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.NicoLive, userId);
        }

        public override Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }

        public override void SetMessage(string raw)
        {
        }

        Task INicoCommentProvider.PostCommentAsync(string comment, string mail)
        {
            throw new System.NotImplementedException();
        }
        public TestCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IDataSource server, ILogger logger, IUserStoreManager userStoreManager) : base(logger, options)
        {
            _logger = logger;
            _userStoreManager = userStoreManager;
            _siteOptions = siteOptions;
            _server = server;
        }
    }
}
