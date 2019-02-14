using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MirrativSitePlugin
{
    class FirstCommentDetector
    {
        Dictionary<string, int> _userCommentCountDict = new Dictionary<string, int>();
        public bool IsFirstComment(string userId)
        {
            bool isFirstComment;
            if (_userCommentCountDict.ContainsKey(userId))
            {
                _userCommentCountDict[userId]++;
                isFirstComment = false;
            }
            else
            {
                _userCommentCountDict.Add(userId, 1);
                isFirstComment = true;
            }
            return isFirstComment;
        }
    }
    class MetadataProvider
    {
        private readonly IDataServer _server;
        private readonly IMirrativSiteOptions _siteOptions;
        private readonly string _liveId;
        public event EventHandler<ILiveInfo> MetadataUpdated;

        public MetadataProvider(IDataServer server, IMirrativSiteOptions siteOptions, string liveId)
        {
            _server = server;
            _siteOptions = siteOptions;
            _liveId = liveId;
        }
        public async Task ReceiveAsync()
        {
            _isDisconnectRequested = false;
            _cts = new CancellationTokenSource();

            while (true)
            {
                if (_isDisconnectRequested)
                {
                    break;
                }
                var liveInfo = await Api.PollLiveAsync(_server, _liveId);
                MetadataUpdated?.Invoke(this, liveInfo);
                try
                {
                    await Task.Delay(_siteOptions.PollingIntervalSec * 60 * 1000, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
        bool _isDisconnectRequested;
        public void Disconnect()
        {
            _isDisconnectRequested = true;
            _cts?.Cancel();
        }
        CancellationTokenSource _cts;
    }
    class MirrativCommentProvider : ICommentProvider
    {
        private bool _canConnect;
        public bool CanConnect
        {
            get { return _canConnect; }
            set
            {
                if (_canConnect == value)
                    return;
                _canConnect = value;
                CanConnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool _canDisconnect;
        public bool CanDisconnect
        {
            get { return _canDisconnect; }
            set
            {
                if (_canDisconnect == value)
                    return;
                _canDisconnect = value;
                CanDisconnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<IMessageContext> MessageReceived;

        //protected virtual string GetChannelName(string input)
        //{
        //    return Tools.GetChannelName(input);
        //}
        //private string _channelName;
        private IMessageProvider _provider;
        //private CookieContainer _cc;
        protected virtual CookieContainer GetCookieContainer(IBrowserProfile browserProfile)
        {
            var cc = new CookieContainer();

            try
            {
                var cookies = browserProfile.GetCookieCollection("mirrativ.com");
                cc.Add(cookies);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            return cc;
        }
        protected virtual IMessageProvider CreateMessageProvider()
        {
            return new MessageProvider("wss://online.mirrativ.com/");
        }
        private IMetadata LiveInfo2Meta(ILiveInfo liveInfo)
        {
            return new Metadata
            {
                Title = liveInfo.Title,
                TotalViewers = liveInfo.TotalViewerNum.ToString(),
                CurrentViewers = liveInfo.OnlineUserNum.ToString(),
            };
        }
        //protected virtual async Task<IMe> GetMeAsync(IDataServer server, CookieContainer cc)
        //{
        //    IMe me = null;
        //    try
        //    {
        //        me = await API.GetMeAsync(_server, cc);
        //    }
        //    catch (NotLoggedInException) { }
        //    catch (Exception ex)
        //    {
        //        _logger.LogException(ex);
        //    }
        //    return me;
        //}
        string _broadcastKey;
        FirstCommentDetector _first;
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            CanConnect = false;
            CanDisconnect = true;
            try
            {
                _first = new FirstCommentDetector();
                var liveId = Tools.ExtractLiveId(input);

                var liveInfo = await Api.GetLiveInfo(_server, liveId);
                MetadataUpdated?.Invoke(this, LiveInfo2Meta(liveInfo));
                Connected?.Invoke(this, new ConnectedEventArgs
                {
                    IsInputStoringNeeded = false,
                    UrlToRestore = null,
                });
                var firstComments = await Api.GetLiveComments(_server, liveId);
                //var initialCvms = new List<MirrativCommentViewModel>();
                foreach (var c in firstComments)
                {
                    var userId = c.UserId;
                    var isFirstComment = _first.IsFirstComment(userId);
                    var user = GetUser(userId);

                    var context = CreateMessageContext(c, true, "");
                    MessageReceived?.Invoke(this, context);
                    //initialCvms.Add(new MirrativCommentViewModel(_options, _siteOptions, c, isFirstComment, this, user));
                }
                //InitialCommentsReceived?.Invoke(this, initialCvms.Cast<ICommentViewModel>().ToList());
                _broadcastKey = liveInfo.Broadcastkey;
                _provider = CreateMessageProvider();
                _provider.Opened += Provider_Opened;
                _provider.Received += Provider_Received; ;
                var commentTask = _provider.ReceiveAsync();
                var metaProvider = new MetadataProvider(_server, _siteOptions, liveId);
                metaProvider.MetadataUpdated += MetaProvider_MetadataUpdated;
                var metaTask = metaProvider.ReceiveAsync();
                var t = await Task.WhenAny(commentTask, metaTask);
                if (t == commentTask)
                {
                    try
                    {
                        await commentTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    metaProvider.Disconnect();
                    try
                    {
                        await metaTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                }
                else if(t == metaTask)
                {
                    try
                    {
                        await metaTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                    //MetadataProviderの内部でcatchしないような例外が投げられた。メタデータの取得は諦めたほうが良い。多分。
                }
            }
            catch(Exception ex)
            {
                _logger.LogException(ex);
            }
            finally
            {
                CanConnect = true;
                CanDisconnect = false;
            }
        }
        private void SendInfo(string message, InfoType type)
        {
            CommentReceived?.Invoke(this, new SystemInfoCommentViewModel(_options, message, type));
        }
        private void MetaProvider_MetadataUpdated(object sender, ILiveInfo e)
        {
            var liveInfo = e;
            MetadataUpdated?.Invoke(this, LiveInfo2Meta(liveInfo));
        }

        private MirrativCommentViewModel CreateCommentViewModel(Message message, bool isFirstComment, IUser user)
        {
            var cvm = new MirrativCommentViewModel(_options, _siteOptions, message, isFirstComment, this, user);
            return cvm;
        }
        private void SetLinkedLiveOwnerName(Message message, dynamic json)
        {
            if (json.IsDefined("linked_live_owner_name"))
            {
                var linkedLiveOwnerName = json["linked_live_owner_name"];
                message.Comment += $"（{linkedLiveOwnerName}さんの配信からのリンク経由）";
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="raw"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        private MirrativMessageContext CreateMessageContext(Message message, bool isInitialComment, string raw, dynamic json)
        {
            SetLinkedLiveOwnerName(message, json);
            return CreateMessageContext(message, isInitialComment, raw);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="raw"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        private MirrativMessageContext CreateMessageContext(Message message, bool isInitialComment, string raw)
        {
            var userId = message.UserId;
            var isFirst = _first.IsFirstComment(userId);
            var user = GetUser(userId);
            var comment = new MirrativComment(message, raw);
            var metadata = new MirrativMessageMetadata(comment, _options, _siteOptions, user, this, isFirst)
            {
                IsInitialComment = isInitialComment,
            };
            var methods = new MirrativMessageMethods();
            if (_siteOptions.NeedAutoSubNickname)
            {
                var messageText = message.Comment;
                var nick = ExtractNickname(messageText);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            return new MirrativMessageContext(comment, metadata, methods);
        }
        /// <summary>
        /// 文字列から@ニックネームを抽出する
        /// 文字列中に@が複数ある場合は一番最後のものを採用する
        /// 数字だけのニックネームは不可
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        protected string ExtractNickname(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            var matches = Regex.Matches(text, "(?:@|＠)(\\S+)", RegexOptions.Singleline);
            if (matches.Count > 0)
            {
                foreach (Match match in matches.Cast<Match>().Reverse())
                {
                    var val = match.Groups[1].Value;
                    if (!Regex.IsMatch(val, "^[0-9０１２３４５６７８９]+$"))
                    {
                        return val;
                    }
                }
            }
            return null;
        }
        private void OnMessageReceived(string data)
        {
            var json = Codeplex.Data.DynamicJson.Parse(data);
            if (json.IsDefined("t"))
            {
                var type = (int)json["t"];
                switch (type)
                {
                    case 1://コメント
                        {
                            Message message = Tools.ParseType1Data(json);
                            var context = CreateMessageContext(message, false, data, json);
                            MessageReceived?.Invoke(this, context);

                            //var cvm = CreateCommentViewModel(message, isFirst, user);
                            //CommentReceived?.Invoke(this, cvm);
                            //Debug.WriteLine(message.Comment);
                        }
                        break;
                    case 3://入室メッセージ
                        {
                            try
                            {
                                //2019/01/02 dictにあるキー
                                //

                                //2019/01/02 Mirrativから送られてきたデータにバグ発見
                                //おそらく"created_at":"1546434659"とするつもりだと思うんだけど、
                                //"":"created_at","1546434659":nullとなっている。
                                long? createdAtNullable = null;
                                if (json.IsDefined("created_at"))
                                {
                                    createdAtNullable = (long)json["created_at"];
                                }
                                else
                                {
                                    foreach (var key in json.GetDynamicMemberNames())
                                    {
                                        if(long.TryParse(key, out long createdAt))
                                        {
                                            createdAtNullable = createdAt;
                                            break;
                                        }
                                    }
                                }
                                var message = new Message
                                {
                                    Comment = json["ac"] + "が入室しました",
                                    CreatedAt = createdAtNullable ?? 0,
                                    Type = MessageType.BroadcastInfo,
                                    UserId = json["u"],
                                    Username = json["ac"],
                                };
                                SetLinkedLiveOwnerName(message, json);
                                var isFirst = false;
                                var user = GetUser(message.UserId);
                                //var cvm = CreateCommentViewModel(message, isFirst, user);
                                //CommentReceived?.Invoke(this, cvm);
                                var joinRoom = new MirrativJoinRoom(message, data);
                                var metadata = new MirrativMessageMetadata(joinRoom, _options, _siteOptions, user, this, isFirst);
                                var methods = new MirrativMessageMethods();
                                MessageReceived?.Invoke(this, new MirrativMessageContext(joinRoom, metadata, methods));

                                MetadataUpdated?.Invoke(this, new Metadata
                                {
                                    CurrentViewers = (json["online_viewer_num"]).ToString(),
                                });
                            }
                            catch (Exception ex)
                            {
                                throw new ParseException(data, ex);
                            }
                        }
                        break;
                    case 7:
                        Debug.WriteLine(data);
                        SendInfo(data, InfoType.Debug);
                        break;
                    case 35:
                        {
                            var message = new Message
                            {
                                Type = MessageType.BroadcastInfo,
                                UserId = json["u"],
                                Username = json["ac"],
                            };
                            var itemCount = int.Parse(json["count"]);
                            if(itemCount ==1)
                            {
                                message.Comment = json["ac"] + "が" + json["gift_title"] + "を贈りました";
                            }
                            else
                            {
                                message.Comment = json["ac"] + "が" + json["gift_title"] + $"を{itemCount}個贈りました";
                            }
                            var isFirst = false;
                            var user = GetUser(message.UserId);
                            var cvm = CreateCommentViewModel(message, isFirst, user);
                            CommentReceived?.Invoke(this, cvm);
                        }
                        break;
                    case 34:
                        break;
                    case 38:
                        break;
                    default:
                        //{"users":[{"u":"4715932","ac":"プーのクマさん🐱💛","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/7f56101d8c1129b9c82ae4d9d7191e64fb55ea9eac3159bfe008791927c8e4b7_m.jpeg?1546437257"},{"u":"5428825","ac":"おとうふ (無職)🐰","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/d78aa116f61804ed94f9fd43745141b5a7cac66ff5773be03d6a16d6cc160294_m.jpeg?1546346805"},{"u":"4956040","ac":"飛べない・涼・🐱💛™️😻","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/7073ad377f51ddea20ce1d97312e6d2888d2b25d820e33beb5f7e90075935aee_m.jpeg?1545913736"}],"t":38}
                        //{"users":[{"u":"4715932","ac":"プーのクマさん🐱💛","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/7f56101d8c1129b9c82ae4d9d7191e64fb55ea9eac3159bfe008791927c8e4b7_m.jpeg?1546437257"},{"u":"5428825","ac":"おとうふ (無職)🐰","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/d78aa116f61804ed94f9fd43745141b5a7cac66ff5773be03d6a16d6cc160294_m.jpeg?1546346805"},{"u":"4956040","ac":"飛べない・涼・🐱💛™️😻","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/7073ad377f51ddea20ce1d97312e6d2888d2b25d820e33beb5f7e90075935aee_m.jpeg?1545913736"}],"t":38}
                        //{"avatar":{"wipe_position":"0","is_fullscreen":"0","background":{"icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/backgrounds/0087_icon.png?v=4","updated_at":"1545894000","url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/backgrounds/0087.png?v=4&v=2","id":"87"},"asset_bundle_url":"https://www.mirrativ.com/assets/avatar/AssetBundlesOpenBeta/Android/","camera":"orth,1.41,0.45","body":{"head":{"icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/bodies/female/heads/0002.png?v=4","updated_at":0,"id":"2"},"icon_url":"https://www.mirrativ.com/assets/img/avatar/sex_female.png","hair_color":{"gradient":["14521944",14796465]},"skin_color":"16577775","asset_bundle_name":"body_f_0001","clothes":{"color":{"setup":{"asset_bundle_prefab_name":"setup_f_0036_01.prefab","asset_bundle_name":"setup_f_0036"},"value":"16777215"},"icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/bodies/female/clothes/setup_f_0036_01.png?v=4","id":"3601"},"eye":{"color":{"asset_bundle_prefab_postfix":"_08_01.prefab","value":"6704704"},"icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/bodies/female/eyes/0008.png?v=4","id":"8"},"asset_bundle_prefab_name":"body_f_0001_01.prefab","proportion":{"icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/bodies/female/proportions/tall.png?v=4","updated_at":0,"id":"tall"},"id":"female","mouth":{"asset_bundle_prefab_postfix":"_02.prefab","icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/bodies/female/mouths/0002.png?v=4","updated_at":0,"id":"2"},"hair":{"icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/bodies/female/hairs/0001.png?v=4","updated_at":0,"asset_bundle_prefab_name":"hair_f_0001.prefab","id":"1","asset_bundle_name":"hair_f_0001"},"hair_color_percentage":"0.16666669386593413"},"wipe_cameras":{"1":"orth,1.52,0.275","0":"orth,1.41,0.45","2":"orth,1.52,0.275"},"enabled":1},"t":34}
                        Debug.WriteLine(data);
                        SendInfo(data, InfoType.Debug);
                        throw new ParseException(data);
                }
            }
            else
            {
                SendInfo(data, InfoType.Debug);
                throw new ParseException(data);
            }
        }
        private void Provider_Received(object sender, string e)
        {
            var str = e;
            var arr = str.Split(new[] { "\t" }, StringSplitOptions.None);
            if (arr.Length == 0)
                return;

            try
            {
                switch (arr[0])
                {
                    case "MSG":
                        if (arr.Length != 3)
                        {
                            throw new ParseException(str);
                        }
                        var data = arr[2];
                        OnMessageReceived(data);
                        break;
                    case "ACK":
                        break;
                    default:
                        throw new ParseException(str);
                }
            }
            catch(ParseException ex)
            {
                _logger.LogException(ex);
            }
            catch (Exception ex)
            {
                SendInfo(str, InfoType.Debug);
                _logger.LogException(ex);
            }
            Debug.WriteLine(str);
        }
        /*
         * メッセージ例
         * t == 1
         * {"speech":"銀杏 どこですか？","d":1,"ac":"銀杏🌸","burl":"","iurl":"","cm":"どこですか？","created_at":1540124936,"u":"4534198","is_moderator":0,"lci":"1049351080","t":1}
         * 
         * t == 2
         * 
         * t == 3
         * {"online_viewer_num":44,"speech":"銀杏🌸が入室しました","d":1,"ac":"銀杏🌸","burl":"","iurl":"","created_at":1540124836,"u":"4534198","is_moderator":0,"lci":0,"t":3}
         * 
         * t == 7
         * {"created_at":1540124709,"collab_enabled":"1","sticker_enabled":"0","collab_has_vacancy":1,"orientation_v2":"6","t":7}
         * 
         * t == 8
         * {"t":8}
         * 
         * t == 9
         * {"u":"4534198","ac":"銀杏🌸","burl":"","iurl":"","owner_name":"🐶 dog🐶","target_live_id":"jfCADsTAHky3y8L85roP9Q","t":9}
         * 
         */
        private async void Provider_Opened(object sender, EventArgs e)
        {
            try
            {
                await _provider.SendAsync("PING");
                await _provider.SendAsync("SUB" + '\t' + _broadcastKey);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                SendSystemInfo(ex.Message, InfoType.Error);
            }
        }
        private void SendSystemInfo(string message, InfoType type)
        {
            CommentReceived?.Invoke(this, new SystemInfoCommentViewModel(_options, message, type));
        }
        public void Disconnect()
        {
            _provider.Disconnect();
        }
        public IUser GetUser(string userId)
        {
            return _userStore.GetUser(userId);
        }
        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        public async Task PostCommentAsync(string text)
        {
            //var s = $"PRIVMSG {_channelName} :{text}";
            await Task.FromResult<object>(null);
        }

        public async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var cc = GetCookieContainer(browserProfile);
            var currentUser = await Api.GetCurrentUserAsync(_server, cc);

            return new CurrentUserInfo
            {
                IsLoggedIn = currentUser.IsLoggedIn,
                UserId = currentUser.UserId,
                Username = currentUser.Name,
            };
        }

        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly ICommentOptions _options;
        private readonly MirrativSiteOptions _siteOptions;
        private readonly IUserStore _userStore;
        public MirrativCommentProvider(IDataServer server, ILogger logger, ICommentOptions options, MirrativSiteOptions siteOptions, IUserStore userStore)
        {
            _server = server;
            _logger = logger;
            _options = options;
            _siteOptions = siteOptions;
            _userStore = userStore;

            CanConnect = true;
            CanDisconnect = false;
        }
    }
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
