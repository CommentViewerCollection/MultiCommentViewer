using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using Mcv.PluginV2;

namespace BigoSitePlugin
{
    interface IInternalMessage
    {
    }
    class NormalText : IInternalMessage
    {
        public string Name { get; set; }
        public string Message { get; set; }

    }
    class NormalGiftText : IInternalMessage
    {
        public string A { get; set; }
        public string B { get; set; }
        public string C { get; set; }
        public string K { get; set; }
        public string M { get; set; }
        public string N { get; set; }
        public string T { get; set; }
        public string U { get; set; }
    }
    class LightMyHeartText : IInternalMessage
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public long Timestamp { get; set; }
        public string ItemId { get; set; }
    }
    class Login : IInternalMessage
    {

    }
    class EnterRoomSuccess : IInternalMessage
    {

    }
    class MessageProvider
    {
        public event EventHandler Opened;
        public event EventHandler<IInternalMessage> Received;
        private readonly IWebsocket _websocket;
        private readonly MessageParser _parser;
        public Task ReceiveAsync(string url)
        {
            return _websocket.ReceiveAsync(url);
        }
        public void Disconnect()
        {
            _websocket?.Disconnect();
        }

        public MessageProvider(IWebsocket websocket, MessageParser parser)
        {
            _websocket = websocket;
            _parser = parser;
            websocket.Received += Websocket_Received;
            websocket.Opened += Websocket_Opened;
        }

        private void Websocket_Opened(object sender, EventArgs e)
        {
            Opened?.Invoke(this, e);

        }
        private void Websocket_Received(object sender, string e)
        {
            var raw = e;
            Debug.WriteLine($"Bigo received:{raw}");
            IInternalMessage internalMessage;
            try
            {
                internalMessage = _parser.Parse(raw);
            }
            catch (Exception ex)
            {
                return;
            }
            Received?.Invoke(this, internalMessage);

            //var d = DynamicJson.Parse(raw);
            //foreach (var dc in d)
            //{
            //    var each = dc.ToString();
            //    var message = _parser.Parse(each);
            //    Received?.Invoke(this, message);
            //}
        }

        public Task SendAsync(string b)
        {
            return _websocket.SendAsync(b);
        }
    }
    class WebSocketLink
    {
        public string UidToken { get; set; }
        public string DeviceId { get; set; }
        public string UserId { get; set; }
    }
    class InternalStudioInfo
    {
        public string RoomId { get; set; }
        public string SiteId { get; set; }
        public string GameTitle { get; set; }
        public string RoomTopic { get; set; }
        public string Nickname { get; set; }
    }
    static class Tools
    {
        /// <summary>
        /// "2001|23"を与えると512279が返ってくる謎のやつ
        /// </summary>
        /// <param name="value">"2001|23"</param>
        /// <returns></returns>
        public static string GetMessagePrefix(string value)
        {
            var arr = value.Split('|');
            if (arr.Length != 2)
            {
                throw new NotImplementedException();
            }
            var n = int.Parse(arr[0]);
            var m = int.Parse(arr[1]);
            return ((n << 8) + m).ToString();
        }
        public static long GetCurrentUnixTimeMillseconds()
        {
            return GetCurrentUnixTimeMillseconds(DateTime.Now);
        }
        public static long GetCurrentUnixTimeMillseconds(DateTime now)
        {
            var utcNow = DateTime.SpecifyKind(now, DateTimeKind.Utc);
            DateTimeOffset offset = utcNow;
            return offset.ToUnixTimeMilliseconds();
        }
    }
    class CommentProvider : ICommentProvider
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
        public event EventHandler LoggedInStateChanged;
        public bool IsLoggedIn
        {
            get
            {
                return true;
            }
        }
        public event EventHandler<IMessageContext> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        public event EventHandler<ConnectedEventArgs> Connected;

        CookieContainer _cc;
        private readonly BigoSiteOptions _siteOptions;
        private readonly ILogger _logger;

        private void SendInfo(string comment, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                Text = comment,
                SiteType = SiteType.Bigo,
                Type = type,
            });
            MessageReceived?.Invoke(this, context);
        }
        private void BeforeConnect()
        {
            _disconnectedExpected = false;
            CanConnect = false;
            CanDisconnect = true;
        }
        private void AfterConnect()
        {
            //_chatProvider = null;
            CanConnect = true;
            CanDisconnect = false;
            SendInfo("切断しました", InfoType.Notice);
        }

        protected virtual CookieContainer CreateCookieContainer(List<Cookie> cookies)
        {
            var cc = new CookieContainer();//まずCookieContainerのインスタンスを作っておく。仮にCookieの取得で失敗しても/live_chatで"YSC"と"VISITOR_INFO1_LIVE"が取得できる。これらは/service_ajaxでメタデータを取得する際に必須。
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
        MessageProvider messageProvider;
        public async Task ConnectAsync(string input, List<Cookie> cookies)
        {
            BeforeConnect();
            try
            {
                await ConnectInternalAsync(input, cookies);
            }
            finally
            {
                AfterConnect();
            }
        }
        private async Task ConnectInternalAsync(string input, List<Cookie> cookies)
        {
            if (string.IsNullOrEmpty(input))
            {
                SendInfo("URLを入力してください", InfoType.Error);
                return;
            }

            MetadataUpdated?.Invoke(this, new Metadata
            {
                Active = "-",
                CurrentViewers = "-",
                Elapsed = "-",
                Title = "-",
                TotalViewers = "-",
            });

            _cc = CreateCookieContainer(cookies);
            var bigoId = GetBigoId(input);
            _webSocketLink = await Api.GetWebSocketLink(_server, _cc);
            _internalStudioInfo = await Api.GetInternalStudioInfo(bigoId, _server, _cc);
            var gifts = await Api.GetOnlineGift(DateTime.Now, _server, _cc);
            _giftDict = gifts.ToDictionary(kv => kv.Typeid);
            var title = await GetTitleAsync(bigoId, _cc);
            if (!string.IsNullOrEmpty(title))
            {
                MetadataUpdated?.Invoke(this, new Metadata
                {
                    Title = title,
                });
            }
            var websocketUrl = "wss://wss.bigolive.tv/bigo/web";
            messageProvider = new MessageProvider(new Websocket
            {
                EnableAutoSendPing = true,
                AutoSendPingInterval = 1000,
                NoDelay = true,
            }, new MessageParser());
            messageProvider.Opened += MessageProvider_Opened;
            messageProvider.Received += MessageProvider_Received;
            try
            {
reload:
                await messageProvider.ReceiveAsync(websocketUrl);
                if (!_disconnectedExpected)
                {
                    Debug.WriteLine("BIGO reload!");
                    goto reload;
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                messageProvider.Opened -= MessageProvider_Opened;
                messageProvider.Received -= MessageProvider_Received;
                messageProvider = null;

            }
        }
        WebSocketLink _webSocketLink;
        InternalStudioInfo _internalStudioInfo;
        Dictionary<string, Gift> _giftDict;
        private async void MessageProvider_Opened(object sender, EventArgs e)
        {
            try
            {
                var prefix = Tools.GetMessagePrefix("2001|23");
                var s = $"{prefix}{{\"uid\":\"{_webSocketLink.UserId}\",\"cookie\":\"{_webSocketLink.UidToken}\",\"secret\":\"0\",\"userName\":\"0\",\"deviceId\":\"{_webSocketLink.DeviceId}\",\"userFlag\":\"0\",\"status\":\"0\",\"password\":\"0\",\"sdkVersion\":\"0\",\"displayType\":\"0\",\"pbVersion\":\"0\",\"lang\":\"cn\",\"loginLevel\":\"0\",\"clientVersionCode\":\"0\",\"clientType\":\"8\",\"clientOsVer\":\"0\",\"netConf\":{{\"clientIp\":\"0\",\"proxySwitch\":\"0\",\"proxyTimestamp\":\"0\",\"mcc\":\"0\",\"mnc\":\"0\",\"countryCode\":\"CN\"}}}}";
                await messageProvider.SendAsync(s);
            }
            catch (Exception ex)
            {

            }
        }

        private async Task<string> GetTitleAsync(string bigoId, CookieContainer cc)
        {
            var url = $"http://www.bigo.tv/OUserCenter/getUserInfoStudio?bigoId={bigoId}";
            var headers = new Dictionary<string, string>
            {
                {"Referer", $"http://www.bigo.tv/{bigoId}" }
            };
            var res = await _server.GetAsync(url, headers, cc);
            //200
            //{"code":0,"msg":"success","data":{"bean":544074,"countryCode":"JP","videoType":1,"roomTopic":"#\u30d9\u30eb\u5f53\u305f\u308a\u67a0 \u5b89\u5b9a\u3059\u3063\u3074\u3093\u2620\ufe0f","nickname":"\u4e43\u611b\ud83e\udd8b\u30cd\u30af\u30b9\u30c610\u4f4d\u611f\u8b1d"}}
            //error(Refererを渡さなかったら返ってきた)
            //"{\"code\":1001,\"msg\":\"not allow to visit\",\"data\":[]}"
            var unescaped = Regex.Unescape(res);
            var match = Regex.Match(unescaped, "\"roomTopic\":\"([^\"]+)\"");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 意図的な切断であるか。falseの場合は自動的に再接続する。
        /// </summary>
        bool _disconnectedExpected;
        private async void MessageProvider_Received(object sender, IInternalMessage e)
        {
            var internalMessage = e;
            switch (internalMessage)
            {
                case Login _:
                    {
                        //以下のようなメッセージを送信する
                        //1304{"seqId":"1609173325654","roomId":"6825840935873906928","reserver":"0","clientVersion":"0","clientType":"3","version":"0","deviceid":"e6cb67b748c649069bdc88b701154f10","secretKey":"0","other":[]}
                        var time = Tools.GetCurrentUnixTimeMillseconds();
                        var roomId = _internalStudioInfo.RoomId;
                        var a = Tools.GetMessagePrefix("5|24");
                        var b = $"{a}{{\"seqId\":\"{time}\",\"roomId\":\"{roomId}\",\"reserver\":\"0\",\"clientVersion\":\"0\",\"clientType\":\"3\",\"version\":\"0\",\"deviceid\":\"{_webSocketLink.DeviceId}\",\"secretKey\":\"0\",\"other\":[]}}";
                        await messageProvider.SendAsync(b);
                    }
                    break;
                case EnterRoomSuccess _:
                    {
                        //以下のようなメッセージを送信する
                        //10776{"uid":"423865522","seqId":"1609173325748","roomid":"6825840935873906928","contribution":"0","enterTimestamp":"0","number":"0","ident":"0","userGrade":"0","version":"0","lastUserBeanGrade":"0","lastUserId":"0","others":[]}
                        var time = Tools.GetCurrentUnixTimeMillseconds();
                        var roomId = _internalStudioInfo.RoomId;
                        var uid = _webSocketLink.UserId;
                        var a = Tools.GetMessagePrefix("42|24");
                        var b = $"{a}{{\"uid\":\"{uid}\",\"seqId\":\"{time}\",\"roomid\":\"{roomId}\",\"contribution\":\"0\",\"enterTimestamp\":\"0\",\"number\":\"0\",\"ident\":\"0\",\"userGrade\":\"0\",\"version\":\"0\",\"lastUserBeanGrade\":\"0\",\"lastUserId\":\"0\",\"others\":[]}}";
                        await messageProvider.SendAsync(b);
                    }
                    break;
                case NormalText normalText:
                    {
                        var m = new BigoComment
                        {
                            Id = "",
                            Message = normalText.Message,
                            Name = normalText.Name,
                            PostedAt = DateTime.Now,
                            UserId = null,
                        };
                        var context = new BigoMessageContext(m);
                        MessageReceived?.Invoke(this, context);
                    }
                    break;
                case LightMyHeartText heartText:
                    {
                        if (!_giftDict.TryGetValue(heartText.ItemId, out var heart))
                        {
                            break;
                        }
                        var m = new BigoGift
                        {
                            Username = heartText.Username,
                            GiftName = heart.Name,
                            GiftCount = 1,
                            GiftImgUrl = heart.ImgUrl,
                        };
                        var context = new BigoMessageContext(m);
                        MessageReceived?.Invoke(this, context);
                    }
                    break;
                case NormalGiftText giftText:
                    {
                        if (!_giftDict.TryGetValue(giftText.M, out var gift))
                        {
                            break;
                        }
                        Debug.WriteLine($"item={gift.Name} × {giftText.C} by {giftText.N}");
                        var m = new BigoGift
                        {
                            Username = giftText.N,
                            GiftName = gift.Name,
                            GiftCount = int.Parse(giftText.C),
                            GiftImgUrl = gift.ImgUrl,
                        };
                        var context = new BigoMessageContext(m);
                        MessageReceived?.Invoke(this, context);
                    }
                    break;
            }
        }

        private (string websocketUrl, string bigoId) GetWebsocketInfo(string livePageHtml)
        {
            string wsUrl;
            var match1 = Regex.Match(livePageHtml, "wsUrl: \"([^\"]*)\",");
            if (match1.Success)
            {
                wsUrl = match1.Groups[1].Value;
            }
            else
            {
                wsUrl = null;
            }
            string bigoId;
            var match2 = Regex.Match(livePageHtml, "bigoId: \"([^\"]*)\",");
            if (match2.Success)
            {
                bigoId = match2.Groups[1].Value;
            }
            else
            {
                bigoId = null;
            }
            return (wsUrl, bigoId);
        }
        private string GetBigoId(string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var match = Regex.Match(input, "bigo\\.tv/ja/(\\d+)");
            if (!match.Success) return null;
            return match.Groups[1].Value;
        }
        public void Disconnect()
        {
            _disconnectedExpected = true;
            messageProvider?.Disconnect();
        }

        async Task ICommentProvider.PostCommentAsync(string text)
        {
            var b = await PostCommentAsync(text);
        }
        public async Task<bool> PostCommentAsync(string text)
        {
            return false;
        }

        public async Task<ICurrentUserInfo> GetCurrentUserInfo(List<Cookie> cookies)
        {
            var currentUserInfo = new CurrentUserInfo();
            var cc = CreateCookieContainer(cookies);
            var url = "https://www.youtube.com/embed";
            var html = await _server.GetAsync(url, cc);
            //"user_display_name":"Ryu"
            var match = Regex.Match(html, "\"user_display_name\":\"([^\"]+)\"");
            if (match.Success)
            {
                var name = match.Groups[1].Value;
                currentUserInfo.Username = name;
                currentUserInfo.IsLoggedIn = true;
            }
            else
            {
                currentUserInfo.IsLoggedIn = false;
            }
            return currentUserInfo;
        }

        public void SetMessage(string raw)
        {

        }

        public Guid SiteContextGuid { get; set; }
        IBigoServer _server;
        public CommentProvider(IBigoServer server, BigoSiteOptions siteOptions, ILogger logger)
        {
            _siteOptions = siteOptions;
            _logger = logger;
            _server = server;

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
