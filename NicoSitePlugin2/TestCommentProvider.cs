using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json;
using NicoSitePlugin.Metadata;

namespace NicoSitePlugin
{
    class TestCommentProvider : CommentProviderBase, INicoCommentProvider, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IUserStoreManager _userStoreManager;
        private readonly INicoSiteOptions _siteOptions;
        private readonly IDataSource _server;
        private readonly Metadata.MetaProvider _metaProvider;
        CancellationTokenSource _disconnectCts;
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
            var nicoInput = Tools.ParseInput(input);
            if (nicoInput is InvalidInput invalidInput)
            {
                SendSystemInfo("未対応の形式のURLが入力されました", InfoType.Error);
                AfterDisconnected();
                return;
            }
            _isFirstConnection = true;
        reload:
            _isDisconnectedExpected = false;
            _disconnectCts = new CancellationTokenSource();
            try
            {
                await ConnectInternalAsync(nicoInput, browserProfile);
            }
            catch (ApiGetCommunityLivesException ex)
            {
                _isDisconnectedExpected = true;
                SendSystemInfo("コミュニティの配信状況の取得に失敗しました", InfoType.Error);
                _logger.LogException(ex, "", $"input:{input}, browser:{browserProfile.Type}");
            }
            catch (SpecChangedException ex)
            {
                _isDisconnectedExpected = true;
                SendSystemInfo("サイトの仕様変更があったためコメント取得を継続できません", InfoType.Error);
                _logger.LogException(ex, "", $"input:{input}, browser:{browserProfile.Type}");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "", $"input:{input}, browser:{browserProfile.Type}");
            }
            _dataProps = null;
            if (!_isDisconnectedExpected)
            {
                _isFirstConnection = false;
                goto reload;
            }
            var m = new NicoDisconnected("");
            var s = new DisconnectedMessageMetadata(m, _options, _siteOptions);
            var c = new NicoMessageContext(m, s, new NicoMessageMethods());
            RaiseMessageReceived(c);
            AfterDisconnected();
        }
        private CookieContainer GetCookieContainer(IBrowserProfile browserProfile)
        {
            return GetCookieContainer(browserProfile, "nicovideo.jp");
        }
        private async Task<string> GetChannelLiveId(ChannelUrl channelUrl)
        {
        check:
            var currentLiveId = await Api.GetCurrentChannelLiveId(_server, channelUrl.ChannelScreenName);
            if (currentLiveId != null)
            {
                return currentLiveId;
            }
            else
            {
                RaiseMetadataUpdated(new TestMetadata
                {
                    Title = "（次の配信が始まるまで待機中...）",
                });
                await Task.Delay(30 * 1000, _disconnectCts.Token);
                goto check;
            }
        }
        private async Task<string> GetCommunityLiveId(CommunityUrl communityUrl, CookieContainer cc)
        {
        check:
            var currentLiveId = await Api.GetCurrentCommunityLiveId(_server, communityUrl.CommunityId, cc);
            if (currentLiveId != null)
            {
                return currentLiveId;
            }
            else
            {
                RaiseMetadataUpdated(new TestMetadata
                {
                    Title = "（次の配信が始まるまで待機中...）",
                });
                await Task.Delay(30 * 1000, _disconnectCts.Token);
                goto check;
            }
        }
        CookieContainer _cc;
        public async Task ConnectInternalAsync(IInput input, IBrowserProfile browserProfile)
        {
            var cc = GetCookieContainer(browserProfile);
            _cc = cc;
            string vid;
            if (input is LivePageUrl livePageUrl)
            {
                vid = livePageUrl.LiveId;
            }
            else if (input is ChannelUrl channelUrl)
            {
                vid = await GetChannelLiveId(channelUrl);
            }
            else if (input is CommunityUrl communityUrl)
            {
                vid = await GetCommunityLiveId(communityUrl, cc);
            }
            else if(input is LiveId liveId)
            {
                vid = liveId.Raw;
            }
            else
            {
                throw new InvalidOperationException("bug");
            }
            var url = "https://live2.nicovideo.jp/watch/" + vid;


            var liveHtml = await _server.GetAsync(url, cc);
            _dataProps = ExtractDataProps(liveHtml);
            if (_dataProps == null)
            {
                throw new SpecChangedException("data-propsが無い", liveHtml);
            }
            if (_dataProps.Status == "ENDED")
            {
                SendSystemInfo("この番組は終了しました", InfoType.Notice);
                if (input is LivePageUrl)//チャンネルやコミュニティのURLを入力した場合は次の配信が始まるまで待機する
                {
                    _isDisconnectedExpected = true;
                }
                return;
            }
            _vposBaseTime = Common.UnixTimeConverter.FromUnixTime(_dataProps.VposBaseTime);
            _localTime = DateTime.Now;
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
                    _toAdd.Clear();
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
                    _metaProvider?.Disconnect();
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
                        _logger.LogException(ex);
                    }
                    _tasks.Clear();//本当はchatのTaskだけ取り除きたいけど、変数に取ってなくて無理だから全部消しちゃう
                }
            }
            return;
        }
        /// <summary>
        /// 初期コメント取得中か
        /// </summary>
        private bool _isInitialCommentsReceiving;
        protected readonly ConcurrentDictionary<string, int> _userCommentCountDict = new ConcurrentDictionary<string, int>();
        /// <summary>
        /// 意図的な切断か
        /// </summary>
        private bool _isDisconnectedExpected;
        /// <summary>
        /// 一番最初の接続か。再接続時はfalse。
        /// 再接続時は初期コメントが不要だから主にその判別に使うフラグ
        /// </summary>
        private bool _isFirstConnection;
        private static bool IsAd(Chat.ChatMessage chat)
        {
            return chat.Content.StartsWith("/nicoad ");
        }
        private static bool IsGift(Chat.ChatMessage chat)
        {
            return chat.Content.StartsWith("/gift ");
        }
        private static bool IsSpi(Chat.ChatMessage chat)
        {
            return chat.Content.StartsWith("/spi ");
        }
        private static bool IsEmotion(Chat.ChatMessage chat)
        {
            return chat.Content.StartsWith("/emotion ");
        }
        private static bool IsInfo(Chat.ChatMessage chat)
        {
            return chat.Content.StartsWith("/info ");
        }
        private static bool IsDisconnect(Chat.ChatMessage chat)
        {
            return chat.Content == "/disconnect";
        }
        /// <summary>
        /// 生IDか
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private bool IsRawUserId(string userId)
        {
            return !string.IsNullOrEmpty(userId) && Regex.IsMatch(userId, "^\\d+$");
        }
        private Task<string> GetUserName(string userId)
        {
            throw new NotImplementedException();
        }
        private const string SystemUserId = "900000000";
        private async Task ProcessChatMessageAsync(Chat.IChatMessage message)
        {
            switch (message)
            {
                case Chat.ChatMessage chat:
                    {
                        if (_isFirstConnection == false && _isInitialCommentsReceiving == true)
                        {
                            //再接続時は初期コメントを無視する
                            return;
                        }
                        var userId = chat.UserId;
                        var user = GetUser(userId);
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
                        //var comment = await Tools.CreateNicoComment(chat, user, _siteOptions, roomName, async userid => await API.GetUserInfo(_dataSource, userid), _logger);
                        INicoMessage comment;
                        INicoMessageMetadata metadata;
                        if (IsAd(chat))
                        {
                            ///nicoad {"totalAdPoint":215500,"message":"シュガーさんが1700ptニコニ広告しました","version":"1"}
                            var adJson = chat.Content.Replace("/nicoad", "");
                            dynamic d = JsonConvert.DeserializeObject(adJson);
                            if ((string)d.version != "1")
                            {
                                throw new ParseException(chat.Raw);
                            }
                            var content = (string)d.message;
                            var ad = new NicoAd(chat.Raw)
                            {
                                PostedAt = Common.UnixTimeConverter.FromUnixTime(chat.Date),
                                UserId = userId,
                                Text = content,
                            };
                            comment = ad;
                            metadata = new AdMessageMetadata(ad, _options, _siteOptions)
                            {
                                IsInitialComment = _isInitialCommentsReceiving,
                                SiteContextGuid = SiteContextGuid,
                            };
                        }
                        else if (IsGift(chat))
                        {
                            var match = Regex.Match(chat.Content, "/gift (\\S+) (\\d+|NULL) \"(\\S+)\" (\\d+) \"(\\S*)\" \"(\\S+)\"(?: (\\d+))?");
                            if (!match.Success)
                            {
                                throw new ParseException(chat.Raw);
                            }
                            var giftId = match.Groups[1].Value;
                            var userIdp = match.Groups[2].Value;//ギフトを投げた人。userId == "900000000"
                            var username = match.Groups[3].Value;
                            var point = match.Groups[4].Value;
                            var what = match.Groups[5].Value;
                            var itemName = match.Groups[6].Value;
                            var itemCount = match.Groups[7].Value;//アイテムの個数？ギフト貢献n位？
                            var text = $"{username}さんがギフト「{itemName}（{point}pt）」を贈りました";
                            var gift = new NicoGift(chat.Raw)
                            {
                                Text = text,
                                PostedAt = Common.UnixTimeConverter.FromUnixTime(chat.Date),
                                UserId = userIdp == "NULL" ? "" : userIdp,
                                NameItems = Common.MessagePartFactory.CreateMessageItems(username),
                            };
                            comment = gift;
                            metadata = new ItemMessageMetadata(gift, _options, _siteOptions)
                            {
                                IsInitialComment = _isInitialCommentsReceiving,
                                SiteContextGuid = SiteContextGuid,
                            };
                        }
                        else if (IsSpi(chat))
                        {
                            var spi = new NicoSpi(chat.Raw)
                            {
                                Text = chat.Content,
                                PostedAt = Common.UnixTimeConverter.FromUnixTime(chat.Date),
                                UserId = chat.UserId,
                            };
                            comment = spi;
                            metadata = new SpiMessageMetadata(spi, _options, _siteOptions)
                            {
                                IsInitialComment = _isInitialCommentsReceiving,
                                SiteContextGuid = SiteContextGuid,
                            };
                        }
                        else if (IsEmotion(chat))
                        {
                            var content = chat.Content.Substring("/emotion ".Length);
                            var abc = new NicoEmotion("")
                            {
                                ChatNo = chat.No,
                                Anonymity = chat.Anonymity,
                                PostedAt = Common.UnixTimeConverter.FromUnixTime(chat.Date),
                                Content = content,
                                UserId = chat.UserId,
                            };
                            comment = abc;
                            metadata = new EmotionMessageMetadata(abc, _options, _siteOptions, user, this)
                            {
                                IsInitialComment = _isInitialCommentsReceiving,
                                SiteContextGuid = SiteContextGuid,
                            };
                        }
                        else if (IsInfo(chat))
                        {
                            var match = Regex.Match(chat.Content, "^/info (?<no>\\d+) (?<content>.+)$", RegexOptions.Singleline);
                            if (!match.Success)
                            {
                                throw new ParseException(chat.Raw);
                            }
                            else
                            {
                                var no = int.Parse(match.Groups["no"].Value);
                                var content = match.Groups["content"].Value;
                                var info = new NicoInfo(chat.Raw)
                                {
                                    Text = content,
                                    PostedAt = Common.UnixTimeConverter.FromUnixTime(chat.Date),
                                    UserId = chat.UserId,
                                    No = no,
                                };
                                comment = info;
                                metadata = new InfoMessageMetadata(info, _options, _siteOptions)
                                {
                                    IsInitialComment = _isInitialCommentsReceiving,
                                    SiteContextGuid = SiteContextGuid,
                                };
                            }
                        }
                        else
                        {
                            if (IsDisconnect(chat))//NicoCommentではなく専用のクラスを作っても良いかも。
                            {
                                _chatProvider?.Disconnect();
                            }
                            string username;
                            if (IsRawUserId(chat.UserId) && chat.UserId != SystemUserId && _siteOptions.IsAutoGetUsername)
                            {
                                var userInfo = await Api.GetUserInfo(_server, _cc, chat.UserId);
                                username = userInfo.Nickname;
                                user.Name = Common.MessagePartFactory.CreateMessageItems(username);
                            }
                            else
                            {
                                username = null;
                            }
                            if (_siteOptions.IsAutoSetNickname)
                            {
                                var nick = SitePluginCommon.Utils.ExtractNickname(chat.Content);
                                if (!string.IsNullOrEmpty(nick))
                                {
                                    user.Nickname = nick;
                                }
                            }
                            var abc = new NicoComment("")
                            {
                                ChatNo = chat.No,
                                Id = chat.No.ToString(),
                                Is184 = chat.Anonymity == 1,
                                PostedAt = Common.UnixTimeConverter.FromUnixTime(chat.Date),
                                Text = chat.Content,
                                UserId = chat.UserId,
                                UserName = username,
                            };
                            comment = abc;
                            metadata = new CommentMessageMetadata(abc, _options, _siteOptions, user, this, isFirstComment)
                            {
                                IsInitialComment = _isInitialCommentsReceiving,
                                SiteContextGuid = SiteContextGuid,
                            };
                        }


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
        private async void ChatProvider_Received(object sender, Chat.IChatMessage e)
        {
            var message = e;
            try
            {
                await ProcessChatMessageAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }

        }

        readonly List<Task> _tasks = new List<Task>();
        readonly List<Task> _toAdd = new List<Task>();
        TaskCompletionSource<object> _mainLooptcs;
        private readonly Chat.ChatProvider _chatProvider;
        Metadata.Room _room;
        DataProps _dataProps;
        private bool _disposedValue;

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
                        _metaProvider?.Send(new Metadata.Pong());
                        break;
                    case Metadata.Statistics stat:
                        RaiseMetadataUpdated(new TestMetadata
                        {
                            TotalViewers = stat.Viewers.ToString(),
                            Others = $"コメント数:{stat.Comments} 広告ポイント:{stat.AdPoints} ギフトポイント:{stat.GiftPoints}",
                        });
                        break;
                    case Metadata.Disconnect disconnect:
                        SendSystemInfo($"メタデータサーバーとの接続が切断されました{Environment.NewLine}原因:{disconnect.Reason}", InfoType.Notice);
                        //Disconnect();
                        break;
                    case Metadata.ServerTime serverTime:
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }
        DateTime? _vposBaseTime;
        DateTime? _localTime;
        /// <summary>
        /// 意図的な切断
        /// </summary>
        public override void Disconnect()
        {
            _isDisconnectedExpected = true;
            _disconnectCts.Cancel();
            _metaProvider?.Disconnect();
            _chatProvider?.Disconnect();
        }

        public override async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var cc = GetCookieContainer(browserProfile);
            try
            {
                var myInfo = await Api.GetMyInfo(_server, cc);
                return await Task.FromResult(new CurrentUserInfo { Username = myInfo.Nickname, IsLoggedIn = myInfo.IsLogin });
            }
            catch (NotLoggedInException)
            {
                return await Task.FromResult(new CurrentUserInfo { Username = "(未ログイン)", IsLoggedIn = false });
            }
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

        Task INicoCommentProvider.PostCommentAsync(string comment, bool is184, string color, string size, string position)
        {
            var elapsed = DateTime.Now.AddHours(-9) - _vposBaseTime.Value;
            var ms = elapsed.TotalMilliseconds;
            var vpos = (long)Math.Round(ms / 10);
            var postComment = new PostComment(comment, vpos, is184, color, size, position);
            _metaProvider.Send(postComment);
            return Task.CompletedTask;
        }
        public TestCommentProvider(ICommentOptions options, INicoSiteOptions siteOptions, IDataSource server, ILogger logger, IUserStoreManager userStoreManager) : base(logger, options)
        {
            _logger = logger;
            _userStoreManager = userStoreManager;
            _siteOptions = siteOptions;
            _server = server;
            _metaProvider = new Metadata.MetaProvider(_logger);
            _metaProvider.Received += MetaProvider_Received;
            _chatProvider = new Chat.ChatProvider(_logger);
            _chatProvider.Received += ChatProvider_Received;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _metaProvider.Received -= MetaProvider_Received;
                    _chatProvider.Received -= ChatProvider_Received;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TestCommentProvider()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
