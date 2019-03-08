using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Common;
using SitePlugin;
namespace MirrativSitePlugin
{
    class Message
    {
        public MessageType Type { get; set; }
        public string Id { get; set; }
        public string Comment { get; set; }
        public long CreatedAt { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
    }
    public class MessageProvider
    {
        private readonly IWebSocket _webSocket;
        private readonly ILogger _logger;
        private readonly string _broadcastKey;

        public event EventHandler<IMirrativMessage> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;

        public Task ReceiveAsync()
        {
            return _webSocket.ReceiveAsync();
        }
        public void Disconnect()
        {
            _webSocket.Disconnect();
        }
        public MessageProvider(IWebSocket webSocket, ILogger logger, string broadcastKey)
        {
            _webSocket = webSocket;
            _logger = logger;
            _broadcastKey = broadcastKey;
            webSocket.Opened += WebSocket_Opened;
            webSocket.Received += WebSocket_Received;
        }

        private void WebSocket_Received(object sender, string e)
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
            catch (ParseException ex)
            {
                _logger.LogException(ex);
            }
            catch (Exception ex)
            {
                //SendInfo(str, InfoType.Debug);
                _logger.LogException(ex);
            }
        }

        private async void WebSocket_Opened(object sender, EventArgs e)
        {
            try
            {
                await _webSocket.SendAsync("PING");
                await _webSocket.SendAsync("SUB" + '\t' + _broadcastKey);
                MessageReceived?.Invoke(this, new MirrativConnected(""));
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                //SendSystemInfo(ex.Message, InfoType.Error);
            }
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
                            var comment = new MirrativComment(message, data);
                            MessageReceived?.Invoke(this, comment);
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
                                        if (long.TryParse(key, out long createdAt))
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
                                var joinRoom = new MirrativJoinRoom(message, data);
                                MessageReceived?.Invoke(this, joinRoom);

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
                    case 8:
                        {
                            var message = new MirrativDisconnected(data);
                            MessageReceived?.Invoke(this, message);
                        }
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
                            if (itemCount == 1)
                            {
                                message.Comment = json["ac"] + "が" + json["gift_title"] + "を贈りました";
                            }
                            else
                            {
                                message.Comment = json["ac"] + "が" + json["gift_title"] + $"を{itemCount}個贈りました";
                            }
                            var item = new MirrativItem(message, data);
                            MessageReceived?.Invoke(this, item);
                        }
                        break;
                    case 34:
                        break;
                    case 38:
                        break;
                    default:
                        //{"u":"1895964","ac":"キザシ","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/bdbf7a85cf950b9fb058e58f0d476d90674843ef6b4952d95db0010e64e26c35_m.jpeg?1551359805","owner_name":"トオるん@火星人(本物)","target_live_id":"bT6KzStu8H0B5-dYa7la4A","t":9}
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

        private void SendInfo(string data, InfoType debug)
        {
        }

        private void SetLinkedLiveOwnerName(Message message, dynamic json)
        {
            if (json.IsDefined("linked_live_owner_name"))
            {
                var linkedLiveOwnerName = json["linked_live_owner_name"];
                message.Comment += $"（{linkedLiveOwnerName}さんの配信からのリンク経由）";
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>接続毎にインスタンスを作る</remarks>
    public class WebSocket : IWebSocket
    {
        public event EventHandler Opened;

        public event EventHandler<string> Received;
        WebSocket4Net.WebSocket _ws;
        TaskCompletionSource<object> _tcs;
        private readonly string _url;

        public Task ReceiveAsync()
        {
            _tcs = new TaskCompletionSource<object>();
            var cookies = new List<KeyValuePair<string, string>>();
            _ws = new WebSocket4Net.WebSocket(_url, "", cookies);
            _ws.MessageReceived += _ws_MessageReceived;
            //_ws.NoDelay = true;
            _ws.Opened += _ws_Opened;
            _ws.Error += _ws_Error;
            _ws.Closed += _ws_Closed;
            _ws.Open();
            return _tcs.Task;
        }

        private void _ws_Closed(object sender, EventArgs e)
        {
            _tcs.TrySetResult(null);
        }

        private void _ws_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            _tcs.TrySetException(e.Exception);
        }

        private void _ws_Opened(object sender, EventArgs e)
        {
            Opened?.Invoke(this, e);
        }

        public async Task SendAsync(string s)
        {
            Debug.WriteLine("send: " + s);
            await Task.Yield();
            _ws.Send(s);// + "\r\n");
        }

        private void _ws_MessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
        {
            Received?.Invoke(this, e.Message);
        }

        public void Disconnect()
        {
            _ws?.Close();
            _ws = null;
        }
        public WebSocket(string url)
        {
            _url = url;
        }
    }
}
