using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Common;
using SitePlugin;
using SitePluginCommon.AutoReconnection;

namespace MirrativSitePlugin
{
    class UnknownMessage : IMirrativMessage
    {
        public MirrativMessageType MirrativMessageType { get; } = MirrativMessageType.Unknown;
        public string Raw { get; set; }
        public SiteType SiteType { get; } = SiteType.Mirrativ;

        public event EventHandler<ValueChangedEventArgs> ValueChanged;
    }
    class MessageParser
    {
        public static Func<DateTime> GetCurrent { get; set; } = () => DateTime.Now;
        private static long GetCurrentUnixTime()
        {
            var dto = new DateTimeOffset(GetCurrent().Ticks, new TimeSpan(9, 0, 0));
            return dto.ToUnixTimeSeconds();
        }
        internal static IMirrativMessage ParseMessage(string data, Action<string, InfoType> SendInfo)
        {
            IMirrativMessage mirrativMessage;
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
                            mirrativMessage = comment;
                        }
                        break;
                    case 3://入室メッセージ
                        {
                            try
                            {
                                var message = new Message
                                {
                                    Comment = json["ac"] + "が入室しました",
                                    CreatedAt = (long)json["created_at"],
                                    Type = MessageType.BroadcastInfo,
                                    UserId = json["u"],
                                    Username = json["ac"],
                                };
                                SetLinkedLiveOwnerName(message, json);
                                var joinRoom = new MirrativJoinRoom(message, data)
                                {
                                    OnlineViewerNum = (int)json["online_viewer_num"],
                                };
                                mirrativMessage = joinRoom;

                                //MetadataUpdated?.Invoke(this, new Metadata
                                //{
                                //    CurrentViewers = (json["online_viewer_num"]).ToString(),
                                //});
                            }
                            catch (Exception ex)
                            {
                                throw new ParseException(data, ex);
                            }
                        }
                        break;
                    //case 4://Followed
                    //    {
                    //        Debug.WriteLine(data);
                    //        mirrativMessage = null;
                    //    }
                    //    break;
                    case 7:
                        Debug.WriteLine(data);
                        SendInfo(data, InfoType.Debug);
                        mirrativMessage = null;
                        break;
                    case 8:
                        {
                            var message = new MirrativDisconnected(data);
                            mirrativMessage = message;
                        }
                        break;
                    //case 16://Shared
                    //    {
                    //        Debug.WriteLine(data);
                    //        mirrativMessage = null;
                    //    }
                    //    break;
                    //case 34:
                    //    mirrativMessage = null;
                    //    break;
                    case 35:
                        {
                            //{"gift_title":"かわいいエモモスナップ(300)","photo_gift_id":"9162721","burl":"","coins":"300","gift_small_image_url":"https:\/\/cdn.mirrativ.com\/mirrorman-prod\/assets\/img\/gift\/small_64.png?v=5","u":"4353835","nameplate_enabled":"1","t":35,"avatar_user_ids":"4072373,4383477,6221780,4353835,2921078,664329","count":1,"is_photo_gift":1,"ac":"matsu【\ud83c\udfa8定期組】\ud83c\udf77\ud83c\udccf\ud83d\udc9c ","total_gift_coins":"25972","iurl":"https:\/\/cdn.mirrativ.com\/mirrorman-prod\/image\/profile_image\/5b4ceb7de739f19491efe17165c7fa2f8c065170ef2b0c1ff039e96c48c6125e_m.jpeg?1552123860","gift_id":"64","pause_duration":"0","orientations":"0","gift_large_image_url":"https:\/\/cdn.mirrativ.com\/mirrorman-prod\/assets\/img\/gift\/large_64.png?v=5","photo_gift_image_url":"https:\/\/cdn.mirrativ.com\/mirrorman-prod\/image\/photo_gift:1552124210:4353835:26477211\/5b4ceb7de739f19491efe17165c7fa2f8c065170ef2b0c1ff039e96c48c6125e_origin.png?1552124211","share_text":"@KURORO966_Blackさん,@akatukihawk3さん,@usausa_otomeさん,@0609_spitzさん,@uru_umiさん,カルルンバ\ud83c\udfa8さんとの  #エモモスナップ！ #エモモ #ミラティブ"}
                            //{"count":"8","gift_title":"小さな星","total_gift_coins":"26306","ac":"\ud83d\udc3e真顔ちゃん'-'\ud83c\udf4a\ud83c\udf4c\ud83d\udd4a\ud83d\udc36\ud83c\udf31\ud83c\udf75","burl":"","iurl":"https:\/\/cdn.mirrativ.com\/mirrorman-prod\/image\/profile_image\/fa3a29a81ece745badebc1fee44071997da131414ee7d53e2bb5228f2adf23cd_m.jpeg?1551797451","coins":"1","gift_small_image_url":"https:\/\/cdn.mirrativ.com\/mirrorman-prod\/assets\/img\/gift\/small_1.png?v=2","u":"5101297","gift_id":"1","nameplate_enabled":"1","pause_duration":"0","gift_large_image_url":"https:\/\/cdn.mirrativ.com\/mirrorman-prod\/assets\/img\/gift\/large_1.png?v=2","t":35}

                            if (json.IsDefined("photo_gift_id"))
                            {
                                var message = new Message
                                {
                                    Type = MessageType.BroadcastInfo,
                                    CreatedAt = GetCurrentUnixTime(),
                                    UserId = json["u"],
                                    Username = json["ac"],
                                    Comment = json["share_text"],
                                };
                                var item = new MirrativPhotoGift(message, data)
                                {
                                    GiftTitle = json["gift_title"],
                                    Coins = int.Parse(json["coins"]),
                                    ShareText = json["share_text"],
                                };
                                mirrativMessage = item;
                            }
                            else
                            {
                                var message = new Message
                                {
                                    Type = MessageType.BroadcastInfo,
                                    CreatedAt = GetCurrentUnixTime(),
                                    UserId = json["u"],
                                    Username = json["ac"],
                                };
                                var countRaw = json["count"];
                                var itemCount = countRaw switch
                                {
                                    string s => int.Parse(s),
                                    double n => (int)n,
                                    _ => throw new ParseException(data),
                                };
                                message.Comment = json["ac"] + "が" + json["gift_title"] + $"を{itemCount}個贈りました";
                                var item = new MirrativGift(message, data)
                                {
                                    Count = itemCount,
                                };
                                mirrativMessage = item;
                            }
                        }
                        break;
                    //case 38:
                    //    mirrativMessage = null;
                    //    break;
                    default:
                        //{"u":"1895964","ac":"キザシ","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/bdbf7a85cf950b9fb058e58f0d476d90674843ef6b4952d95db0010e64e26c35_m.jpeg?1551359805","owner_name":"トオるん@火星人(本物)","target_live_id":"bT6KzStu8H0B5-dYa7la4A","t":9}
                        //{"users":[{"u":"4715932","ac":"プーのクマさん🐱💛","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/7f56101d8c1129b9c82ae4d9d7191e64fb55ea9eac3159bfe008791927c8e4b7_m.jpeg?1546437257"},{"u":"5428825","ac":"おとうふ (無職)🐰","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/d78aa116f61804ed94f9fd43745141b5a7cac66ff5773be03d6a16d6cc160294_m.jpeg?1546346805"},{"u":"4956040","ac":"飛べない・涼・🐱💛™️😻","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/7073ad377f51ddea20ce1d97312e6d2888d2b25d820e33beb5f7e90075935aee_m.jpeg?1545913736"}],"t":38}
                        //{"users":[{"u":"4715932","ac":"プーのクマさん🐱💛","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/7f56101d8c1129b9c82ae4d9d7191e64fb55ea9eac3159bfe008791927c8e4b7_m.jpeg?1546437257"},{"u":"5428825","ac":"おとうふ (無職)🐰","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/d78aa116f61804ed94f9fd43745141b5a7cac66ff5773be03d6a16d6cc160294_m.jpeg?1546346805"},{"u":"4956040","ac":"飛べない・涼・🐱💛™️😻","burl":"","iurl":"https://cdn.mirrativ.com/mirrorman-prod/image/profile_image/7073ad377f51ddea20ce1d97312e6d2888d2b25d820e33beb5f7e90075935aee_m.jpeg?1545913736"}],"t":38}
                        //{"avatar":{"wipe_position":"0","is_fullscreen":"0","background":{"icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/backgrounds/0087_icon.png?v=4","updated_at":"1545894000","url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/backgrounds/0087.png?v=4&v=2","id":"87"},"asset_bundle_url":"https://www.mirrativ.com/assets/avatar/AssetBundlesOpenBeta/Android/","camera":"orth,1.41,0.45","body":{"head":{"icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/bodies/female/heads/0002.png?v=4","updated_at":0,"id":"2"},"icon_url":"https://www.mirrativ.com/assets/img/avatar/sex_female.png","hair_color":{"gradient":["14521944",14796465]},"skin_color":"16577775","asset_bundle_name":"body_f_0001","clothes":{"color":{"setup":{"asset_bundle_prefab_name":"setup_f_0036_01.prefab","asset_bundle_name":"setup_f_0036"},"value":"16777215"},"icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/bodies/female/clothes/setup_f_0036_01.png?v=4","id":"3601"},"eye":{"color":{"asset_bundle_prefab_postfix":"_08_01.prefab","value":"6704704"},"icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/bodies/female/eyes/0008.png?v=4","id":"8"},"asset_bundle_prefab_name":"body_f_0001_01.prefab","proportion":{"icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/bodies/female/proportions/tall.png?v=4","updated_at":0,"id":"tall"},"id":"female","mouth":{"asset_bundle_prefab_postfix":"_02.prefab","icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/bodies/female/mouths/0002.png?v=4","updated_at":0,"id":"2"},"hair":{"icon_url":"https://cdn.mirrativ.com/mirrorman-prod/assets/avatar/img/bodies/female/hairs/0001.png?v=4","updated_at":0,"asset_bundle_prefab_name":"hair_f_0001.prefab","id":"1","asset_bundle_name":"hair_f_0001"},"hair_color_percentage":"0.16666669386593413"},"wipe_cameras":{"1":"orth,1.52,0.275","0":"orth,1.41,0.45","2":"orth,1.52,0.275"},"enabled":1},"t":34}
                        Debug.WriteLine(data);
                        SendInfo(data, InfoType.Debug);
                        //throw new ParseException(data);
                        mirrativMessage = new UnknownMessage { Raw = data };
                        break;
                }
            }
            else
            {
                SendInfo(data, InfoType.Debug);
                throw new ParseException(data);
            }
            return mirrativMessage;
        }
        private static void SetLinkedLiveOwnerName(Message message, dynamic json)
        {
            if (json.IsDefined("linked_live_owner_name"))
            {
                var linkedLiveOwnerName = json["linked_live_owner_name"];
                message.Comment += $"（{linkedLiveOwnerName}さんの配信からのリンク経由）";
            }
        }
    }
    class Message
    {
        public MessageType Type { get; set; }
        public string Id { get; set; }
        public string Comment { get; set; }
        public long CreatedAt { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
    }
    class MessageProvider2 : IProvider
    {
        public IProvider Master { get; set; }
        public bool IsFinished { get; private set; }
        public Task Work { get; private set; }
        public ProviderFinishReason FinishReason { get; }

        private readonly IWebSocket _webSocket;
        private readonly ILogger _logger;
        public string BroadcastKey { get; set; }

        public event EventHandler<IMirrativMessage> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;

        public void Start()
        {
            Work = _webSocket.ReceiveAsync();
        }

        public void Stop()
        {
            _webSocket.Disconnect();
        }
        public MessageProvider2(IWebSocket webSocket, ILogger logger)
        {
            _webSocket = webSocket;
            _logger = logger;
            webSocket.Opened += WebSocket_Opened;
            webSocket.Received += WebSocket_Received;
        }
        public void SetMessage(string raw)
        {
            var arr = raw.Split(new[] { "\t" }, StringSplitOptions.None);
            if (arr.Length == 0)
                return;
            try
            {
                switch (arr[0])
                {
                    case "MSG":
                        if (arr.Length != 3)
                        {
                            throw new ParseException(raw);
                        }
                        var data = arr[2];
                        OnMessageReceived(data);
                        break;
                    case "ACK":
                        break;
                    default:
                        throw new ParseException(raw);
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
        private void WebSocket_Received(object sender, string e)
        {
            var str = e;
            SetMessage(str);
        }

        private async void WebSocket_Opened(object sender, EventArgs e)
        {
            try
            {
                await _webSocket.SendAsync("PING");
                await _webSocket.SendAsync("SUB" + '\t' + BroadcastKey);
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
            var message = MessageParser.ParseMessage(data, SendInfo);
            if (message is UnknownMessage)
            {
                _logger.LogException(new ParseException(data));
                return;
            }
            if (message != null)
            {
                if (message is IMirrativDisconnected disconnected)
                {
                    this.Stop();
                }
                MessageReceived?.Invoke(this, message);
            }
            if (message is IMirrativJoinRoom join)
            {
                MetadataUpdated?.Invoke(this, new Metadata
                {
                    CurrentViewers = join.OnlineViewerNum.ToString(),
                });
            }
        }
        private void SendInfo(string data, InfoType debug)
        {
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
