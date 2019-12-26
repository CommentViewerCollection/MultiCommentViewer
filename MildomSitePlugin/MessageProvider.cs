using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Codeplex.Data;
using Common;
using Newtonsoft.Json.Linq;
using SitePlugin;
using SitePluginCommon.AutoReconnection;

namespace MildomSitePlugin
{
    interface IInternalMessage
    {
        string Raw { get; }
    }
    class UnknownMessage : IInternalMessage
    {
        public string Raw { get; set; }
    }
    class UnImplementedMessage : IInternalMessage
    {
        public string Raw { get; set; }
    }
    class OnChatMessage : IInternalMessage
    {
        public IEnumerable<IMessagePart> MessageItems { get; set; }
        public long UserId { get; internal set; }
        public string UserName { get; internal set; }
        public string UserImg { get; internal set; }
        public DateTime PostedAt { get; internal set; }
        public string Raw { get; set; }
    }
    class OnBroadcast : IInternalMessage
    {
        public string Raw { get; set; }
    }
    internal class OnLiveEnd : IInternalMessage
    {
        public OnLiveEnd(string raw)
        {
            Raw = raw;
        }
        public string Raw { get; }
    }
    internal class OnAddMessage : IInternalMessage
    {
        public string Message { get; set; }
        public int Level { get; set; }
        public long UserId { get; set; }
        public string UserImg { get; set; }
        public string UserName { get; set; }
        public DateTime PostedAt { get; internal set; }
        public string Raw { get; set; }
    }
    class MessageParser
    {
        public static DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
        public static IInternalMessage Parse(string raw, Dictionary<int, string> imageDict)
        {
            IInternalMessage internalMessage;
            var d = DynamicJson.Parse(raw);
            switch (d.cmd)
            {
                case "enterRoom":
                    //{"admin": 0, "cmd": "enterRoom", "fobiddenGlobal": 0, "forbidden": 0, "reqId": 1, "rst": 0, "type": 2, "userCount": 239}
                    internalMessage = new UnImplementedMessage();
                    break;
                case "onChat":
                    //{"area": 2000, "cmd": "onChat", "fansBgPic": null, "fansGroupType": null, "fansLevel": null, "fansName": null, "level": 7, "medals": null, "msg": "うめえぇぇえ", "reqId": 0, "roomAdmin": 0, "roomId": 10038336, "toId": 10038336, "toName": "Nephrite【ネフライト】", "type": 3, "userId": 10088625, "userImg": "https://vpic.mildom.com/download/file/jp/mildom/nnphotos/10088625/5F0AB42E-8BF4-4A3A-9E70-FC6A9A49AAF0.jpg", "userName": "FSｰSavage"}
                    var messageItems = new List<IMessagePart>();
                    var arr = Regex.Split(d.msg, "\\[/(\\d+)\\]");
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (i % 2 == 0)
                        {
                            if (string.IsNullOrEmpty(arr[i]))
                            {
                                continue;
                            }
                            messageItems.Add(Common.MessagePartFactory.CreateMessageText(arr[i]));
                        }
                        else
                        {
                            if (int.TryParse(arr[i], out int n))
                            {
                                if (imageDict.TryGetValue(n, out var emotUrl))
                                {
                                    messageItems.Add(new Common.MessageImage
                                    {
                                        Alt = "",
                                        Height = 40,
                                        Width = 40,
                                        Url = emotUrl,
                                        X = null,
                                        Y = null,
                                    });
                                }
                            }
                        }
                    }
                    internalMessage = new OnChatMessage
                    {
                        MessageItems = messageItems,
                        UserId = (long)d.userId,
                        UserName = d.userName,
                        UserImg = d.userImg,
                        PostedAt = GetCurrentDateTime(),
                        Raw = raw,
                    };
                    break;
                case "onAdd":
                    //{"area": 1000, "avatarDecortaion": 0, "cmd": "onAdd", "enterroomEffect": 0, "level": 18, "loveCountSum": 0, "medals": null, "nobleLevel": 0, "reqId": 0, "roomId": 10038336, "rst": 0, "type": 3, "userCount": 239, "userId": 10088217, "userImg": "https://lh3.googleusercontent.com/a-/AAuE7mC4Jiq49Foq6-k-TmPrkeim6cc1Rq197AC7SSM7=s120", "userName": "ぼつすけ"}
                    var username = d.userName;
                    var message = $"{username}さんが入室しました";
                    internalMessage = new OnAddMessage
                    {
                        Message = message,
                        Level = (int)d.level,
                        UserId = (long)d.userId,
                        UserName = username,
                        UserImg = d.userImg,
                        PostedAt = GetCurrentDateTime(),
                        Raw = raw,
                    };
                    break;
                case "onBroadcast":
                    //{"area": 2000, "clickColor": "#F8AC07", "clickLink": "https://event.mildom.com/activity/view?series_id=11&week=2", "clickText": "こちらをクリック！", "cmd": "onBroadcast", "msg": "配信ランキングに挑戦！${click.text}", "msgColor": "#3C8BF9", "reqId": 0, "roomId": 10038336, "rst": 0, "type": 3, "userName": "guest809480"}
                    //{"area": 2000, "clickColor": "#F6C145", "clickLink": "https://wia.mildom.com/views/rule/message.html", "clickText": "クリックして詳細を表示", "cmd": "onBroadcast", "msg": "注意:ポルノ、暴力、喫煙などの不適切なコンテンツは厳しく禁止されています。${click.text}", "msgColor": "#68B9FF", "reqId": 0, "roomId": 10037397, "rst": 0, "type": 3, "userName": "kv510k"}
                    internalMessage = new OnBroadcast
                    {
                        Raw = raw,
                    };
                    break;
                case "onGift":
                    //{"area": 2000, "avatarDecortaion": 0, "category": 1, "cmd": "onGift", "comboEffect": 0, "count": 1, "countSum": 1, "fansBgPic": null, "fansGroupType": null, "fansLevel": null, "fansName": null, "giftCoin": 106187, "giftId": 1086, "guestOrder": null, "level": 21, "medals": null, "nobleLevel": 0, "reqId": 0, "roomId": 10038336, "toAvatarDecortaion": 0, "toId": 10038336, "toLevel": 25, "toName": "Nephrite【ネフライト】", "toUserImg": "http://pbs.twimg.com/profile_images/1127227613058039809/xhm1svMM.png", "type": 3, "userId": 10073272, "userImg": "https://profile.line-scdn.net/0hARjarbzhHn1XGDHTaddhKmtdEBAgNhg1LypXE3QQExopfVkib3lVGSJMR0RyKlEvY3ZZHScdRh9_", "userName": "Yumiko♥"}
                    internalMessage = new UnImplementedMessage();
                    break;
                case "runCmdNotify":
                    //{"cmd": "runCmdNotify", "runBody": {"host_id": 10038336, "room_id": 10038336, "user_id": 10008249, "user_level": 31, "user_name": "odoritora / Riddle"}, "runCmd": "on_host_followed", "type": 3}
                    internalMessage = new UnImplementedMessage();
                    break;
                case "onLove":
                    //{"area": 2000, "cmd": "onLove", "count": 2, "countSum": 11, "fansBgPic": null, "fansGroupType": null, "fansLevel": null, "fansName": null, "level": 46, "loveId": 4, "medals": [], "msg": "taps", "reqId": 0, "roomId": 10038336, "toId": 0, "toName": "Nephrite【ネフライト】", "type": 3, "userId": 10005716, "userImg": "https://lh3.googleusercontent.com/a-/AAuE7mDLSMHK8SDSzPp4b8GGKiPzml7J0xND7p6s7uw_=s120", "userName": "hashi070429"}
                    internalMessage = new UnImplementedMessage();
                    break;
                case "onUserCount":
                    //{"cmd": "onUserCount", "roomId": 10000157, "type": 3, "userCount": 179}
                    internalMessage = new UnImplementedMessage();
                    break;
                case "onLiveEnd":
                    //{"cmd": "onLiveEnd", "roomId": 10038336, "type": 3}
                    internalMessage = new OnLiveEnd(raw);
                    break;
                default:
                    internalMessage = new UnknownMessage();
                    break;
            }
            return internalMessage;
        }
    }



    class MessageProvider : IProvider
    {
        public IProvider Master { get; set; }
        public bool IsFinished { get; private set; }
        public Task Work { get; private set; }
        public ProviderFinishReason FinishReason { get; }

        private readonly IWebSocket _webSocket;
        private readonly ILogger _logger;
        public IMyUserInfo MyInfo { get; set; }
        public string RoomId { get; set; }

        public event EventHandler<IInternalMessage> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;

        public void Start()
        {
            Work = _webSocket.ReceiveAsync();
        }

        public void Stop()
        {
            _webSocket.Disconnect();
        }
        public MessageProvider(IWebSocket webSocket, ILogger logger, Dictionary<int, string> imageDict)
        {
            _webSocket = webSocket;
            _logger = logger;
            _imgDict = imageDict;
            webSocket.Opened += WebSocket_Opened;
            webSocket.Received += WebSocket_Received;
        }
        Dictionary<int, string> _imgDict;
        private void WebSocket_Received(object sender, string e)
        {
            var raw = e;
            var internalMessage = MessageParser.Parse(raw, _imgDict);
            MessageReceived?.Invoke(this, internalMessage);
        }
        private string Create()
        {
            var userInfo = MyInfo;
            if (userInfo is LoggedinUserInfo loggedin)
            {
                var userId = loggedin.UserId;
                var level = 1;
                var userName = loggedin.Loginname;
                var userImg = "";
                var accessToken = loggedin.AccessToken;
                var guestId = loggedin.Gid;
                var nonopara = "";
                var roomId = RoomId;
                //{"userId":10104058,"level":1,"medals":[],"userName":"kv510k","userImg":"http://pbs.twimg.com/profile_images/1014534803364827139/vuSCBJ15.jpg","accessToken":"e1b8213d-93cd-4b10-ae55-5384794a2ec5","guestId":"pc-gp-cb7af220-7301-4072-88b8-314cac78f26c","nonopara":"fr=web`sfr=pc`devi=Windows 10 64-bit`la=ja`gid=pc-gp-cb7af220-7301-4072-88b8-314cac78f26c`na=Japan`loc=Japan|Kanagawa`clu=aws_japan`wh=1920*1080`rtm=2019-11-28T06:27:27.664Z`ua=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36`uid=10104058`loginname=kv510k`level=1`aid=10001683`live_type=2`live_subtype=2`game_key=Overwatch`game_type=pc`host_official_type=official_game`isHomePage=false","roomId":10001683,"cmd":"enterRoom","reConnect":0,"nobleLevel":0,"avatarDecortaion":0,"enterroomEffect":0,"nobleClose":0,"nobleSeatClose":0,"reqId":1}
                var s = $"{{\"userId\":{userId},\"level\":{level},\"medals\":[],\"userName\":\"{userName}\",\"userImg\":\"{userImg}\",\"accessToken\":\"{accessToken}\",\"guestId\":\"{guestId}\",\"nonopara\":\"{nonopara}\",\"roomId\":{roomId},\"cmd\":\"enterRoom\",\"reConnect\":0,\"nobleLevel\":0,\"avatarDecortaion\":0,\"enterroomEffect\":0,\"nobleClose\":0,\"nobleSeatClose\":0,\"reqId\":1}}";
                return s;
            }
            else
            {
                var anonymous = userInfo as AnonymousUserInfo;
                if (anonymous == null) return null;
                var userId = 0;//常に0
                var level = 1;
                var userName = anonymous.GuestName;
                var guestId = anonymous.GuestId;
                var nonopara = "";
                var roomId = RoomId;
                //{"userId":0,"level":1,"userName":"guest819226","guestId":"pc-gp-fca75ce9-323b-41ab-862b-48639a528092","nonopara":"fr=web`sfr=pc`devi=Windows 10 64-bit`la=ja`gid=pc-gp-fca75ce9-323b-41ab-862b-48639a528092`na=Japan`loc=Japan|Kanagawa`clu=aws_japan`wh=1920*1080`rtm=2019-11-27T16:38:25.331Z`ua=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36`aid=10001084`live_type=2`live_subtype=2`game_key=Tom_Clancys_Rainbow_Six_Siege`game_type=pc`host_official_type=official_game`isHomePage=false","roomId":10001084,"cmd":"enterRoom","reConnect":0,"nobleLevel":0,"avatarDecortaion":0,"enterroomEffect":0,"nobleClose":0,"nobleSeatClose":0,"reqId":1}
                var s = $"{{\"userId\":{userId},\"level\":{level},\"userName\":\"{userName}\",\"guestId\":\"{guestId}\",\"nonopara\":\"{nonopara}\",\"roomId\":{roomId},\"cmd\":\"enterRoom\",\"reConnect\":0,\"nobleLevel\":0,\"avatarDecortaion\":0,\"enterroomEffect\":0,\"nobleClose\":0,\"nobleSeatClose\":0,\"reqId\":1}}";
                return s;
            }
        }
        private async void WebSocket_Opened(object sender, EventArgs e)
        {
            try
            {
                var msg = Create();
                await _webSocket.SendAsync(msg);
                //MessageReceived?.Invoke(this, new MildomConnected(""));
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                //SendSystemInfo(ex.Message, InfoType.Error);
            }
        }
        private void SendInfo(string data, InfoType debug)
        {
        }
    }
}
