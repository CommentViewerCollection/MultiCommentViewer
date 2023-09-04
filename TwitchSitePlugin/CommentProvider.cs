﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Windows.Threading;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Mcv.PluginV2;

namespace TwitchSitePlugin
{
    class Metadata : IMetadata
    {
        public string? Title { get; set; }
        public string? Elapsed { get; set; }
        public string? CurrentViewers { get; set; }
        public string? Active { get; set; }
        public string? TotalViewers { get; set; }
        public bool? IsLive { get; set; }
        public string? Others { get; set; }
    }
    class UserState
    {
        public Dictionary<string, string> Tags { get; }
        public List<string> Params { get; }
        public UserState(Dictionary<string, string> tags, List<string> @params)
        {
            Tags = tags;
            Params = @params;
        }
        public void UpdateTags(Dictionary<string, string> newTags)
        {
            foreach (var key in newTags.Keys)
            {
                if (Tags.ContainsKey(key))
                {
                    Tags[key] = newTags[key];
                }
                else
                {
                    Tags.Add(key, newTags[key]);
                }
            }
        }
    }
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
    class TwitchCommentProvider : ICommentProvider
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
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<IMessageContext> MessageReceived;

        protected virtual string GetChannelName(string input)
        {
            return Tools.GetChannelName(input);
        }
        private string _channelName;
        private IMessageProvider _provider;
        //private CookieContainer _cc;
        protected virtual CookieContainer GetCookieContainer(List<Cookie> cookies)
        {
            var cc = new CookieContainer();

            try
            {
                foreach (var cookie in cookies)
                {
                    cc.Add(cookie);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            return cc;
        }
        protected virtual IMessageProvider CreateMessageProvider()
        {
            return new MessageProvider2();
        }
        protected virtual IMetadataProvider CreateMetadataProvider(string channelName)
        {
            return new MetadataProvider(_server, _siteOptions, channelName);
        }
        public async Task Init()
        {
            _commentCounter = new CommentCounter();
        }
        CommentCounter _commentCounter;
        public async Task ConnectAsync(string input, List<Cookie> cookies)
        {
            CanConnect = false;
            CanDisconnect = true;

            try
            {
                _channelName = GetChannelName(input);
            }
            catch (Exception ex)
            {
                SendSystemInfo($"入力したURLからチャンネル名を取得できませんでした", InfoType.Error);
                _logger.LogException(ex, "", $"input={input}");
            }
            try
            {
                //cookieの例
                //language=ja; _ga=GA1.2.1920479743.1518016680; unique_id=eTRVJSWTkPRuT1uLoWBbg5aQ5CaM4Ljo; __utmz=165406266.1520768529.6.4.utmcsr=twitch.tv|utmccn=(referral)|utmcmd=referral|utmcct=/kv501k/dashboard; __gads=ID=ef2e7365c12367c1:T=1523611207:S=ALNI_MbbhnPy2IqfXsKZVSa-OeOBuKGsPg; Unique_ID=2c6b62cbb53a4a7493a3129ef8b38002; mp_809576468572134f909dffa6bd0dcfcf_mixpanel=%7B%22distinct_id%22%3A%20%2216170d840fb315-054303055d0714-d35346d-1fa400-16170d840fc9f0%22%2C%22%24initial_referrer%22%3A%20%22https%3A%2F%2Fwww.twitch.tv%2Fsettings%2Fprofile%22%2C%22%24initial_referring_domain%22%3A%20%22www.twitch.tv%22%7D; __utma=165406266.1920479743.1518016680.1524682833.1526145797.8; Unique_ID_v2=2c6b62cbb53a4a7493a3129ef8b38002; session_unique_id=ArrQHYWS55SLOCTQKdvFME0Nc57Qn1vH; _gid=GA1.2.511219589.1540912015; persistent=270919349%3A%3Arptndai1hlfiz2x658fjs56i63kq9v; sudo=eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyNzA5MTkzNDkiLCJhdWQiOlsic3VkbyJdLCJleHAiOjE1NDA5MjAwMjAsImlhdCI6MTU0MDkxNjQyMH0=.pY5YvJrcvb6o5j67qsNXrT9YBZO8e2NO-qictdIp15fC6efmbd8ivys-tNA6Zot7AwckDd8JsmKqwAQPEBJAiA==; bits_sudo=eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyNzA5MTkzNDkiLCJhdWQiOlsic3VkbyIsImJpdHMiXSwiZXhwIjoxNTQxNTIxMjIwLCJpYXQiOjE1NDA5MTY0MjB9.k7pm0x6PvcvL8B9R4d-VYFz4nrL_fR-4ozWWO7o4boSIKHY3uxOzW3FseLTEr0CZ7T_oeXLyHE_6pw1bWaj3Qg==; login=ryu8107; name=ryu8107; last_login=2018-10-30T16:20:20Z; api_token=14a0b0a43fc8d41a279adf0865317921; device_cookie=eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJodHRwczovL3Bhc3Nwb3J0LnR3aXRjaC50diIsInN1YiI6InJ5dTgxMDciLCJhdWQiOiJicnV0ZS1mb3JjZS1wcm90ZWN0aW9uIiwiZXhwIjoxNTU2NDY4NDIwLCJpYXQiOjE1NDA5MTY0MjAsIm5vbmNlIjoiTzBtWXJBYkVXU1ZULXJ5XzRIOXd3a3BlQTBPdjNSOUtMVmI5enRaNlZQbz0ifQ%3D%3D.uzFfEDu2yN17Dswjt8nEDaXlaqg4ZZUETOwql-x-WiboJw0hgNYgWFGbN_1FWJsO5OswcKxFB5dbaMfuYhNFIA%3D%3D; twilight-user={%22authToken%22:%22qlxy239ugu4we0rzvs5u9fcpcuvjj4%22%2C%22displayName%22:%22RYU8107%22%2C%22id%22:%22270919349%22%2C%22login%22:%22ryu8107%22%2C%22roles%22:{%22isStaff%22:false}%2C%22version%22:2}; auth-token=qlxy239ugu4we0rzvs5u9fcpcuvjj4; server_session_id=19358718db714130bdefc3b898bcccf9                
                foreach (var cookie in cookies)
                {
                    if (cookie.Name == "auth-token")
                    {
                        _oauthToken = cookie.Value;
                    }
                    else if (cookie.Name == "login")
                    {
                        _name = cookie.Value;
                    }
                }

                await Init();

                _provider = CreateMessageProvider();
                _provider.Opened += Provider_Opened;
                _provider.Received += Provider_Received;
                var messageProviderTask = _provider.ReceiveAsync();

                var metaProvider = CreateMetadataProvider(_channelName);
                metaProvider.MetadataUpdated += MetaProvider_MetadataUpdated;
                var metaProviderTask = metaProvider.ReceiveAsync();
                var tasks = new List<Task>();
                tasks.Add(messageProviderTask);
                tasks.Add(metaProviderTask);


                while (tasks.Count > 0)
                {
                    var t = await Task.WhenAny(tasks);
                    if (t == messageProviderTask)
                    {
                        try
                        {
                            await messageProviderTask;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogException(ex);
                        }
                        tasks.Remove(messageProviderTask);
                        try
                        {
                            metaProvider.Disconnect();
                            await metaProviderTask;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogException(ex);
                        }
                        tasks.Remove(metaProviderTask);
                        break;
                    }
                    else
                    {
                        try
                        {
                            await metaProviderTask;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogException(ex);
                        }
                        tasks.Remove(metaProviderTask);
                    }
                }
            }
            finally
            {
                CanConnect = true;
                CanDisconnect = false;
            }
        }
        DateTime? _startedAt;
        System.Timers.Timer _elapsedTimer;
        private void MetaProvider_MetadataUpdated(object sender, Stream e)
        {
            var stream = e;
            Debug.Assert(stream != null);
            if (!_startedAt.HasValue)
            {
                _startedAt = stream.StartedAt;
                _elapsedTimer.Enabled = true;
            }
            var metadata = new Metadata
            {
                Title = stream.Title,
                CurrentViewers = stream.ViewerCount.ToString(),
            };
            MetadataUpdated?.Invoke(this, metadata);
        }

        private readonly ConcurrentDictionary<string, int> _userCommentCountDict = new ConcurrentDictionary<string, int>();
        protected virtual ICommentData ParsePrivMsg(Result result)
        {
            return Tools.ParsePrivMsg(result);
        }
        string _oauthToken;
        string _name;
        UserState _userState;
        private async void Provider_Received(object sender, string e)
        {
            var raw = e;
            await ProcessMessage(raw);
        }

        private async Task ProcessMessage(string raw)
        {
            Debug.WriteLine(raw);
            var result = Tools.Parse(raw);
            try
            {
                switch (result.Command)
                {
                    case "CLEARCHAT":
                        //"@ban-duration=10;room-id=37402112;target-msg-id=4830aaeb-1610-47b1-911e-9da2637816c5;target-user-id=87037096;tmi-sent-ts=1567069654595 :tmi.twitch.tv CLEARCHAT #shroud :derzackenausderkrone"
                        break;
                    case "CLEARMSG":
                        //"@login=kale9222;room-id=;target-msg-id=759454c4-d09f-4fed-a8d6-3a20335995ec;tmi-sent-ts=1567075260054 :tmi.twitch.tv CLEARMSG #shroud :stop playing this game man :D"
                        break;
                    case "PING":
                        await _provider.SendAsync("PONG");
                        break;
                    case "GLOBALUSERSTATE":
                        _userState = new UserState(result.Tags, result.Params);
                        break;
                    case "HOSTTARGET":
                        //:tmi.twitch.tv HOSTTARGET #evo6 :evo 4922
                        break;
                    case "USERSTATE":
                        _userState.UpdateTags(result.Tags);
                        break;
                    case "USERNOTICE":
                        //"@badge-info=subscriber/11;badges=subscriber/6,bits/100;color=#FF00FF;display-name=Kosnes;emotes=205480:0-10;flags=;id=b0dbd1a7-86fe-4f54-9d4b-1cdd47a49628;login=kosnes;mod=0;msg-id=resub;msg-param-cumulative-months=11;msg-param-months=0;msg-param-should-share-streak=1;msg-param-streak-months=11;msg-param-sub-plan-name=Channel\\sSubscription\\s(meclipse);msg-param-sub-plan=Prime;room-id=37402112;subscriber=1;system-msg=Kosnes\\ssubscribed\\swith\\sTwitch\\sPrime.\\sThey've\\ssubscribed\\sfor\\s11\\smonths,\\scurrently\\son\\sa\\s11\\smonth\\sstreak!;tmi-sent-ts=1567069704460;user-id=42814323;user-type= :tmi.twitch.tv USERNOTICE #shroud :shroud4Head"
                        break;
                    case "ROOMSTATE":
                        //"@emote-only=0;followers-only=10;r9k=0;rituals=0;room-id=37402112;slow=5;subs-only=0 :tmi.twitch.tv ROOMSTATE #shroud"
                        break;
                    case "PRIVMSG":
                        {
                            //useridが含まれていないPRIVMSGを確認。ホスティングされたことを伝える運営コメント
                            //:jtv!jtv@jtv.tmi.twitch.tv PRIVMSG 3lis_game :GamesFan34260 is now hosting you.

                            OnMessageReceived(result);
                            //var cvm = new TwitchCommentViewModel(_options, _siteOptions, commentData, isFirstComment, this, user);
                            //CommentReceived?.Invoke(this, cvm);
                        }
                        break;
                    case "NOTICE":
                        //@msg-id=msg_channel_suspended :tmi.twitch.tv NOTICE #videos :This channel has been suspended.
                        //@msg-id=msg_requires_verified_phone_number :tmi.twitch.tv NOTICE #ksonsouchou :A verified phone number is required to chat in this channel. Please visit https://www.twitch.tv/settings/security to verify your phone number.
                        OnNoticeReceived(result);
                        break;
                    case "CAP":
                        break;
                    case "JOIN":
                        //":kv501k!kv501k@kv501k.tmi.twitch.tv JOIN #shroud"
                        break;
                    case "001":
                        //":tmi.twitch.tv 001 kv501k :Welcome, GLHF!"
                        break;
                    case "002":
                        //":tmi.twitch.tv 001 kv501k :Welcome, GLHF!"
                        break;
                    case "003":
                        //":tmi.twitch.tv 003 kv501k :This server is rather new"
                        break;
                    case "004":
                        //":tmi.twitch.tv 004 kv501k :-"
                        break;
                    case "353":
                        //":kv501k.tmi.twitch.tv 353 kv501k = #shroud :kv501k"
                        break;
                    case "366":
                        //":kv501k.tmi.twitch.tv 366 kv501k #shroud :End of /NAMES list"
                        break;
                    case "372":
                        //":tmi.twitch.tv 372 kv501k :You are in a maze of twisty passages, all alike."
                        break;
                    case "375":
                        //":tmi.twitch.tv 375 kv501k :-"
                        break;
                    case "376":
                        //":tmi.twitch.tv 376 kv501k :>"
                        break;
                    default:
                        Debug.WriteLine($"Twitch unknown command={result.Command}");
                        SendSystemInfo(result.Raw, InfoType.Debug);
                        throw new ParseException(result.Raw);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "", $"raw={result.Raw}");
            }
        }

        private void OnMessageReceived(Result result)
        {
            var commentData = ParsePrivMsg(result);
            var userId = commentData.UserId;
            var displayName = commentData.DisplayName;
            var isFirstComment = _commentCounter.UpdateCount(userId);
            string? newNickname = null;
            if (_siteOptions.NeedAutoSubNickname)
            {
                var nick = Utils.ExtractNickname(commentData.Message, _siteOptions.NeedAutoSubNicknameStr);
                if (!string.IsNullOrEmpty(nick))
                {
                    newNickname = nick;
                }
            }
            var message = new TwitchComment(result.Raw)
            {
                CommentItems = Tools.GetMessageItems(result),
                Id = commentData.Id,
                UserName = commentData.Username,
                PostTime = commentData.SentAt.ToString("HH:mm:ss"),
                UserId = commentData.UserId,
                IsDisplayNameSame = commentData.Username == commentData.DisplayName,
                DisplayName = commentData.DisplayName,
            };
            var messageContext = new TwitchMessageContext(message, userId, newNickname);
            MessageReceived?.Invoke(this, messageContext);
        }
        private void OnNoticeReceived(Result result)
        {
            var message = result.Params[1];
            var notice = new TwitchNotice(result.Raw)
            {
                Message = message,
            };
            var messageContext = new TwitchMessageContext(notice, null, null);
            MessageReceived?.Invoke(this, messageContext);
        }

        protected virtual string GetRandomGuestUsername()
        {
            return Tools.GetRandomGuestUsername();
        }
        private async void Provider_Opened(object sender, EventArgs e)
        {
            try
            {
                if (IsLoggedIn())
                {
                    var name = _name;
                    var oauthToken = _oauthToken;
                    await _provider.SendAsync("CAP REQ :twitch.tv/tags twitch.tv/commands");
                    await _provider.SendAsync($"PASS oauth:{oauthToken}");
                    await _provider.SendAsync($"NICK {name}");
                    await _provider.SendAsync($"USER {name} 8 * :{name}");
                }
                else
                {
                    var name = GetRandomGuestUsername();
                    await _provider.SendAsync("CAP REQ :twitch.tv/tags twitch.tv/commands");
                    await _provider.SendAsync($"PASS SCHMOOPIIE");
                    await _provider.SendAsync($"NICK {name}");
                    await _provider.SendAsync($"USER {name} 8 * :{name}");
                }
                await _provider.SendAsync($"JOIN #" + _channelName);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                SendSystemInfo(ex.Message, InfoType.Error);
            }
        }
        protected virtual bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(_name) && !string.IsNullOrEmpty(_oauthToken);
        }

        public void Disconnect()
        {
            _provider.Disconnect();
        }
        public async Task PostCommentAsync(string text)
        {
            var s = $"PRIVMSG #{_channelName} :{text}";
            await _provider.SendAsync(s);

            //自分が投稿したコメントはサーバから送られてこないため自分で作る必要がある
            var message = Tools.CreatePrivMsg(_userState, _name, _channelName, text, GetCurrentDateTime());
            var result = Tools.Parse(message);
            OnMessageReceived(result);
        }
        public Guid SiteContextGuid { get; set; }
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly TwitchSiteOptions _siteOptions;
        public TwitchCommentProvider(IDataServer server, ILogger logger, TwitchSiteOptions siteOptions)
        {
            _server = server;
            _logger = logger;
            _siteOptions = siteOptions;

            CanConnect = true;
            CanDisconnect = false;

            _elapsedTimer = new System.Timers.Timer();
            _elapsedTimer.Interval = 500;
            _elapsedTimer.Elapsed += ElapsedTimer_Elapsed;
        }

        protected virtual DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
        private void ElapsedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_startedAt.HasValue) return;

            var elapsed = GetCurrentDateTime() - _startedAt.Value;
            var metadata = new Metadata
            {
                Elapsed = Utils.ElapsedToString(elapsed),
            };
            MetadataUpdated?.Invoke(this, metadata);
        }

        private void SendSystemInfo(string message, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                Text = message,
                SiteType = SiteType.Twitch,
                Type = type,
            });
            MessageReceived?.Invoke(this, context);
        }

        public Task<ICurrentUserInfo> GetCurrentUserInfo(List<Cookie> cookies)
        {
            var info = new CurrentUserInfo();
            string name = null;
            string displayName = null;
            foreach (var cookie in cookies)
            {
                switch (cookie.Name)
                {
                    case "login":
                        name = cookie.Value;
                        break;
                    case "twilight-user":
                        {
                            //"{\"authToken\":\"rkpavalsbvBovec0qj2l5r5q0mnlm4\",\"displayName\":\"abckk\",\"id\":\"124821926\",\"login\":\"ob112\",\"roles\":{\"isStaff\":false},\"version\":2}"
                            var decoded = System.Web.HttpUtility.UrlDecode(cookie.Value);
                            var match = Regex.Match(decoded, "\"displayName\":\"([^\"]+)\"");
                            if (match.Success)
                            {
                                displayName = match.Groups[1].Value;
                            }
                        }
                        break;

                }
            }
            info.Username = displayName ?? name;
            info.IsLoggedIn = !string.IsNullOrEmpty(name);
            return Task.FromResult<ICurrentUserInfo>(info);
        }
        bool _isInitialized;
        public async void SetMessage(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return;
            }
            if (!_isInitialized)
            {
                _isInitialized = true;
                await Init();
            }
            await ProcessMessage(raw);
        }
    }
    class CommentData : ICommentData
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Message { get; set; }
        public string Emotes { get; set; }
        public string Id { get; set; }
        public DateTime SentAt { get; internal set; }
    }
    class CommentCounter
    {
        private readonly ConcurrentDictionary<string, int> _countDict = new ConcurrentDictionary<string, int>();
        public bool UpdateCount(string id)
        {
            bool isFirst;
            if (_countDict.ContainsKey(id))
            {
                _countDict[id]++;
                isFirst = false;
            }
            else
            {
                _countDict.AddOrUpdate(id, 1, (s, n) => n);
                isFirst = true;
            }
            return isFirst;
        }
    }
}
