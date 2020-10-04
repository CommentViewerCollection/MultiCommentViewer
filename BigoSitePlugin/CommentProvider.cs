using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using SitePlugin;
using ryu_s.BrowserCookie;
using Common;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Codeplex.Data;
using System.Web;
using System.Collections.Concurrent;
using System.Linq;
using SitePluginCommon;
using System.Net.Http;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace BigoSitePlugin
{
    interface IInternalMessage
    {
        int Num { get; }
        string Raw { get; }
    }
    class UnknownMessage : IInternalMessage
    {
        public int Num => -1;
        public string Raw { get; set; }
    }
    class Num0 : IInternalMessage
    {
        public int Num => 0;
        public string Raw { get; set; }
    }
    class Num1 : IInternalMessage
    {
        public int Num => 1;
        public string Name { get; set; }
        public string Message { get; set; }
        public string K { get; set; }
        public long Date { get; set; }
        public string Raw { get; set; }
    }
    class Num5 : IInternalMessage
    {
        public int Num => 5;
        public int CurrentViewers { get; set; }
        public string Raw { get; set; }
    }
    class Num8 : IInternalMessage
    {
        public int Num => 8;
        public string Raw { get; set; }
    }
    class Num9 : IInternalMessage
    {
        public int Num => 9;
        public string Raw { get; set; }
    }
    class Num10 : IInternalMessage
    {
        public int Num => 10;
        public string Raw { get; set; }
    }
    class Num13 : IInternalMessage
    {
        public int Num => 13;
        public string Raw { get; set; }
    }
    class MessageParser
    {
        protected virtual long GetDate()
        {
            return 0;
        }
        public IInternalMessage Parse(string raw)
        {
            var d = DynamicJson.Parse(raw);
            if (!d.IsDefined("c"))
            {
                return new UnknownMessage
                {
                    Raw = raw,
                };
            }

            var num = (int)d.c;
            IInternalMessage ret;
            switch (num)
            {
                case 0://end
                    {
                        //{\"c\":0,\"data\":{\"totalTime\":3231}}
                        var data = d.data;
                        ret = new Num0
                        {
                            Raw = raw,
                        };
                    }
                    break;
                case 1:
                    {
                        var data = d.data;
                        string k;
                        if (data.IsDefined("k"))
                        {
                            k = data.k;
                        }
                        else
                        {
                            k = null;
                        }
                        long date;
                        if (data.IsDefined("d"))
                        {
                            var preDate = data.d;
                            if (preDate is string s)
                            {
                                date = long.Parse(s);
                            }
                            else
                            {
                                date = (long)data.d;
                            }
                        }
                        else
                        {
                            date = GetDate();
                        }
                        var message = data.m;
                        var name = data.n;

                        ret = new Num1
                        {
                            K = k,
                            Message = message,
                            Name = name,
                            Date = date,
                            Raw = raw,
                        };
                    }
                    break;
                case 5:
                    {
                        var data = d.data;
                        ret = new Num5
                        {
                            CurrentViewers = int.Parse(data.m),
                            Raw = raw,
                        };
                    }
                    break;
                case 8:
                    {
                        //{"c":8,"data":{"a":"0","b":"0","c":"1","k":"","m":"2463","n":"Ezanee🎸ギターのやつ🦋","t":"","u":""},"grade":22}
                        //このメッセージの直後にc:13が来る。c:13の方が情報量が多いからこのメッセージは無視してもいいかも。
                        ret = new Num8
                        {
                            Raw = raw,
                        };
                    }
                    break;
                case 9:
                    {
                        //{"c":9,"data":{"b":0,"n":"渚","nu":0,"a":0},"grade":1}
                        //コメント欄には"sent ♡"と表示される
                        ret = new Num9
                        {
                            Raw = raw,
                        };
                    }
                    break;
                case 10:
                    {
                        //{"c":10,"data":{"b":0,"nu":0,"a":0,"m":"つー"},"grade":2}
                        //コメント欄には"became a Fan.Won't miss hte next LIVE"と表示される
                        ret = new Num10
                        {
                            Raw = raw,
                        };
                    }
                    break;
                case 13:
                    {
                        //{"c":13,"push":0,"id":2463,"from":446903006,"cnt":1,"ticket":543581,"times":22,"n":"Ezanee🎸ギターのやつ🦋","url":"http://esx.bigo.sg/live/3s1/22DVBt.jpg"}
                        //{"c":13,"push":0,"id":2463,"from":446903006,"cnt":1,"ticket":543582,"times":23,"n":"Ezanee🎸ギターのやつ🦋","url":"http://esx.bigo.sg/live/3s1/22DVBt.jpg"}
                        ret = new Num13
                        {
                            Raw = raw,
                        };
                    }
                    break;
                default:
                    ret = new UnknownMessage
                    {
                        Raw = raw,
                    };
                    break;
            }
            return ret;
        }
    }
    class MessageProvider
    {
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

        private async void Websocket_Opened(object sender, EventArgs e)
        {
            try
            {
                await _websocket.SendAsync("websocket");
            }
            catch (Exception ex)
            {

            }
        }

        private void Websocket_Received(object sender, string e)
        {
            var raw = e;
            Debug.WriteLine($"Bigo received:{raw}");
            var d = DynamicJson.Parse(raw);
            foreach (var dc in d)
            {
                var each = dc.ToString();
                var message = _parser.Parse(each);
                Received?.Invoke(this, message);
            }
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
        private readonly ICommentOptions _options;
        private readonly BigoSiteOptions _siteOptions;
        private readonly ILogger _logger;
        private readonly IUserStoreManager _userStoreManager;

        private void SendInfo(string comment, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                Text = comment,
                SiteType = SiteType.Bigo,
                Type = type,
            }, _options);
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

        protected virtual CookieContainer CreateCookieContainer(IBrowserProfile browserProfile)
        {
            var cc = new CookieContainer();//まずCookieContainerのインスタンスを作っておく。仮にCookieの取得で失敗しても/live_chatで"YSC"と"VISITOR_INFO1_LIVE"が取得できる。これらは/service_ajaxでメタデータを取得する際に必須。
            try
            {
                var cookies = browserProfile.GetCookieCollection("www.bigo.tv");
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
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            if (string.IsNullOrEmpty(input))
            {
                return;
            }
            BeforeConnect();
            MetadataUpdated?.Invoke(this, new Metadata
            {
                Active = "-",
                CurrentViewers = "-",
                Elapsed = "-",
                Title = "-",
                TotalViewers = "-",
            });

            _cc = CreateCookieContainer(browserProfile);
            var bigoId = GetBigoId(input);
            var livePageHtml = await _server.GetAsync("http://www.bigo.tv/" + bigoId, _cc);
            var title = await GetTitleAsync(bigoId, _cc);
            if (!string.IsNullOrEmpty(title))
            {
                MetadataUpdated?.Invoke(this, new Metadata
                {
                    Title = title,
                });
            }
            var wsInfo = GetWebsocketInfo(livePageHtml);
            messageProvider = new MessageProvider(new Websocket
            {
                EnableAutoSendPing = true,
                AutoSendPingInterval = 1000,
                NoDelay = true,
            }, new MessageParser());
            messageProvider.Received += MessageProvider_Received;
            try
            {
            reload:
                await messageProvider.ReceiveAsync(wsInfo.websocketUrl);
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
                messageProvider.Received -= MessageProvider_Received;
                messageProvider = null;
                AfterConnect();
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
        private void MessageProvider_Received(object sender, IInternalMessage e)
        {
            switch (e)
            {
                case Num0 num0:
                    _disconnectedExpected = true;
                    break;
                case Num1 num1:
                    {
                        var siteMessage = new BigoComment(num1);
                        var user = GetUser(num1.Name);
                        var isFirstComment = false;
                        var metadata = new BigoMessageMetadata(siteMessage, _options, _siteOptions, user, this, isFirstComment);
                        MessageReceived?.Invoke(this, new BigoMessageContext(siteMessage, metadata, new BigoMessageMethods()));
                    }
                    break;
                case Num5 num5:
                    {
                        MetadataUpdated?.Invoke(this, new Metadata
                        {
                            CurrentViewers = num5.CurrentViewers.ToString(),
                        });
                    }
                    break;
                case UnknownMessage unknown:
                    {
                    }
                    break;
                default:
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
            var match = Regex.Match(input, "bigo\\.tv/(\\d+)");
            if (!match.Success) return null;
            return match.Groups[1].Value;
        }
        public void Disconnect()
        {
            _disconnectedExpected = true;
            messageProvider?.Disconnect();
        }
        public IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.Bigo, userId);
        }

        async Task ICommentProvider.PostCommentAsync(string text)
        {
            var b = await PostCommentAsync(text);
        }
        public async Task<bool> PostCommentAsync(string text)
        {
            return false;
        }

        public async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var currentUserInfo = new CurrentUserInfo();
            var cc = CreateCookieContainer(browserProfile);
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
        public CommentProvider(ICommentOptions options, IBigoServer server, BigoSiteOptions siteOptions, ILogger logger, IUserStoreManager userStoreManager)
        {
            _options = options;
            _siteOptions = siteOptions;
            _logger = logger;
            _userStoreManager = userStoreManager;
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
