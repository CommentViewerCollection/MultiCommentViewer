using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.Concurrent;
using System.Net.Http;
using SitePlugin;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace NicoSitePlugin.Test
{
    public class NicoCommentProvider : INicoCommentProvider
    {
        private bool _canConnect;
        public bool CanConnect
        {
            get { return _canConnect; }
            private set
            {
                _canConnect = value;
                CanConnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private bool _canDisconnect;
        public bool CanDisconnect
        {
            get { return _canDisconnect; }
            private set
            {
                _canDisconnect = value;
                CanDisconnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<List<ICommentViewModel>> CommentsReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;

        private string GetLiveId(string input)
        {
            var match = Regex.Match(input, "(lv\\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            throw new ArgumentException();
        }
        
        public async Task ConnectAsync(string input, ryu_s.BrowserCookie.IBrowserProfile browserProfile)
        {
            //TODO:新配信か旧配信かを判別する必要がある。
            //とりあえず旧配信から実装しよう。

            //コミュニティは今まで通りレベルで部屋数を決めれば良いと思う。動的に生成されて立ち見Zまで可能とかって話しもあるけど、未確認。
            //チャンネルも今まで通り先頭の6部屋+動的に2部屋ずつ
            //公式も今まで通り。

            CanConnect = false;
            CanDisconnect = true;
            _cts = new CancellationTokenSource();
            IsDisconnectOffered = false;
            try
            {

                var live_id = GetLiveId(input);
                var cookies = browserProfile.GetCookieCollection("nicovideo.jp");
                var cc = new CookieContainer();
                cc.Add(cookies);
                var response = await API.GetPlayerStatusAsync(_dataSource, live_id, cc);
                if (!response.Success)
                {
                    //Infoを送信する
                    string errorMsg;
                    switch (response.Error.Code)
                    {
                        case ErrorCode.closed:
                            errorMsg = "放送が終了しているため、コメントが取得できませんでした。";
                            break;
                        default:
                            errorMsg = "エラーが発生したため、コメントが取得できませんでした。";
                            break;
                    }
                    var info = new InfoCommentViewModel(_connectionName, _options, errorMsg);
                    CommentsReceived?.Invoke(this, new List<ICommentViewModel> { info });
                    return;
                }
                else
                {
                    var playerStatus = response.PlayerStatus;
                    //var currentRoom = new RoomInfo(playerStatus.Ms, playerStatus.RoomLabel);

                    //var rooms = Tools.GetRooms(currentRoom, playerStatus.ProviderType, _siteOptions);

                    switch (playerStatus.ProviderType)
                    {
                        case ProviderType.Channel:
                            {
                                //待ち時間を引数で受け取りたい
                                var waitTime = 0;// 3 * 60 * 1000;
                                var taskList = new List<Task>();
                                var dict = new Dictionary<RoomInfo, RoomContext>();
                                while (!_cts.IsCancellationRequested)
                                {
                                    taskList.Clear();
                                    taskList.AddRange(dict.Select(kv => kv.Value.CommentReceivingTask));
                                    var psRetriveTask = Task.Run<IPlayerStatusResponse>(async () =>
                                    {
                                        await Task.Delay(waitTime);
                                        var ps = await API.GetPlayerStatusAsync(_dataSource, live_id, cc);

                                        return ps;
                                    });
                                    taskList.Add(psRetriveTask);


                                    //TODO:どっかの部屋でエラーが起きたりした時の処理をしっかりしないと。

                                    var t = await Task.WhenAny(taskList);
                                    if(t == psRetriveTask)
                                    {
                                        var psResponse = await psRetriveTask;
                                        if (psResponse.Success)
                                        {
                                            var psN = psResponse.PlayerStatus;
                                            //取得済みの部屋との差分を取る。
                                            var kizon = dict.Select(kv => kv.Key).ToList();

                                            RoomInfo[] roomsN;
                                            //if(IsBroadcaster())
                                            //{
                                            //  
                                            //}
                                            //else
                                            //{
                                            ////リスナーの場合は、現在の部屋の情報から取れる
                                                roomsN = Tools.GetRooms(new RoomInfo(psN.Ms, psN.RoomLabel), psN.ProviderType, _siteOptions);
                                            //}

                                            //一番最初は取得済みの部屋が何も無いから、全てがnewRoom
                                            var newRooms = GetSubtract(kizon, roomsN);
                                            foreach(var room in roomsN)
                                            {
                                                if (!kizon.Contains(room))
                                                {
                                                    newRooms.Add(room);
                                                    dict.Add(room, new RoomContext {  CommentReceivingTask = DoAsync(room, _cts.Token)});
                                                }
                                            }
                                            if(newRooms.Count > 0 && HasMsList(psN))
                                            {
                                                //追加があり、配信者だからサーバにアップする。
                                                //配信者かどうかの汎用性のある判定方法を考えたい。MsListを持っているかどうかは俺のサーバから取ったデータの場合使えない。
                                                //PlayerStatusにあるOwnerIdはコミュのオーナーのものか、それとも配信者のものか。配信者のだったらそれで判定できる。
                                                if (_siteOptions.CanUploadPlayerStatus)
                                                {
                                                    //俺のサーバにアップする。
                                                    try
                                                    {
                                                        HttpContent streamContent = new StringContent(psN.Raw);
                                                        using (var client = new HttpClient())
                                                        using (var formData = new MultipartFormDataContent())
                                                        {
                                                            formData.Add(streamContent, "ps", live_id + ".xml");
                                                            var res = await client.PostAsync("http://int-main.net/playerstatus", formData);
                                                        }                                                        
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Debug.WriteLine(ex.Message);
                                                        
                                                    }
                                                }
                                                
                                            }
                                        }
                                        else
                                        {
                                            //とりあえず諦めて何もしない。
                                            //途中で追い出しがあった場合はFull何てことも考えられる。
                                        }

                                    }
                                    else
                                    {
                                        await t;
                                    }
                                    if (_cts.IsCancellationRequested)
                                        break;
                                    waitTime = 3 * 60 * 1000;
                                }
                            }
                            break;
                        case ProviderType.Community:
                            break;
                        case ProviderType.Official:
                            break;
                        default:
                            throw new NotSupportedException();
                    }


                }
            }
            finally
            {
                CanConnect = true;
                CanDisconnect = false;
            }
        }
        private bool HasMsList(IPlayerStatus ps)
        {
            //ニコ生のサーバから取ったやつのみ有効。俺のサーバからだったらこれじゃダメ。
            return ps.MsList != null && ps.MsList.Length > 0;
        }
        /// <summary>
        /// 差分を取得する
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin"></param>
        /// <param name="newList"></param>
        /// <returns></returns>
        /// <remarks>List.Exceptでもできそうだけど、要素の順番を崩したくないからとりあえず自前で実装</remarks>
        private List<T> GetSubtract<T>(List<T> origin, IEnumerable<T> newList)
        {
            var newRooms = new List<T>();
            foreach (var room in newList)
            {
                if (!origin.Contains(room))
                {
                    newRooms.Add(room);
                }
            }
            return newRooms;
        }
        class RoomContext
        {
            public StreamSocket Socket { get; set; }
            public int RetryCount { get; set; }
            public Task CommentReceivingTask { get; set; }
        }

        private async Task DoAsync(RoomInfo roomInfo, CancellationToken ct)
        {
            var res_from = 100;
            var xml = $"<thread thread=\"{roomInfo.Thread}\" version=\"20061206\" res_from=\"-{res_from}\" scores=\"1\" />\0";
            var socket = new StreamSocket(roomInfo.Addr, roomInfo.Port, 8192*16, new SplitBuffer2("\0"));
            socket.Received += (s, e) =>
            {
                var cvmList = new List<ICommentViewModel>();
                foreach (var str in e)
                {
                    if (str.StartsWith("<chat "))
                    {
                        var chat = new chat(str);
                        var cvm = new NicoCommentViewModel2(_connectionName, chat, _options, _siteOptions);
                        cvmList.Add(cvm);
                    }
                    else if (str.StartsWith("<thread "))
                    {
                        var thread = new thread(str);
                        if(thread.resultcode == 1)
                        {
                            //could not connect to the room.
                            socket.Disconnect();
                        }
                    }
                }
                CommentsReceived?.Invoke(this, cvmList);
            };
            _dict.Add(roomInfo, socket);
            await socket.ConnectAsync();
            await socket.SendAsync(xml);
            await socket.ReceiveAsync();
        }
        Dictionary<RoomInfo, StreamSocket> _dict = new Dictionary<RoomInfo, StreamSocket>();
        
        bool IsDisconnectOffered = false;
        public void Disconnect()
        {
            IsDisconnectOffered = true;
            foreach(var kv in _dict)
            {
                kv.Value.Disconnect();
            }
            if(_cts != null)
            {
                _cts.Cancel();
            }
        }

        public List<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        public Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }
        private readonly ConnectionName _connectionName;
        private readonly IOptions _options;
        private readonly NicoSiteOptions _siteOptions;
        private readonly IDataSource _dataSource;
        private CancellationTokenSource _cts;
        internal NicoCommentProvider(ConnectionName connectionName, IOptions options, NicoSiteOptions siteOptions)
        {
            _connectionName = connectionName;
            _options = options;
            _siteOptions = siteOptions;
            _dataSource = new DataSource();

            CanConnect = true;
            CanDisconnect = false;
        }
    }
    public class InfoCommentViewModel : ICommentViewModel
    {
        public string ConnectionName => _connectionName.Name;

        public IEnumerable<IMessagePart> NameItems { get; }

        public IEnumerable<IMessagePart> MessageItems { get; }

        public string UserId { get; }

        public string Info { get; set; }

        public string Id { get; set; }

        public string Nickname { get; set; }

        public bool IsInfo => true;

        public bool IsFirstComment => false;

        public IUser User { get; set; }

        public IEnumerable<IMessagePart> Thumbnail => new List<IMessagePart>();

        public FontFamily FontFamily => _options.FontFamily;

        public FontStyle FontStyle => _options.FontStyle;

        public FontWeight FontWeight => _options.FontWeight;

        public int FontSize => _options.FontSize;

        public bool IsVisible { get; set; } = true;

        public SolidColorBrush Foreground => new SolidColorBrush(_options.InfoForeColor);

        public SolidColorBrush Background => new SolidColorBrush(_options.InfoBackColor);

        public Task AfterCommentAdded()
        {
            throw new NotImplementedException();
        }
        private readonly IOptions _options;
        private readonly ConnectionName _connectionName;
        public InfoCommentViewModel(ConnectionName connectionName, IOptions options, string message)
        {
            _connectionName = connectionName;
            _options = options;
            MessageItems = new List<IMessagePart> { new MessageText { Text = message } };
        }
        #region INotifyPropertyChanged
        [NonSerialized]
        private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
        /// <summary>
        /// 
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
    class ChannelCommentProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msList"></param>
        /// <param name="arenaName">RoomLabelを設定するのにアリーナだけは情報が必要</param>
        /// <returns></returns>
        public static RoomInfo[] ConvertChannelMsList(IMs[] msList, string arenaName)
        {
            var list = new List<RoomInfo>();
            int i = 0;
            foreach (var ms in msList)
            {
                string roomLabel = i == 0 ? arenaName : $"立ち見{(char)('A' + i - 1)}列";
                list.Add(new RoomInfo(ms, roomLabel));
                i++;
            }
            return list.ToArray();
        }
        async Task<RoomInfo[]> GetInitialRooms()
        {
            //1,俺のサーバからPSの取得を試みる。
            //2,取れたらそこにms_listがあるはずだから、それを元にRoomInfo[]を作成
            //3,ニコ生サーバから取る。
            //4,ms_listがあったら配信者と見做して俺のサーバにアップ。RoomInfo[]に変換
            //5,無かったらリスナー。1で失敗した場合のみmsを元に各部屋の情報を計算して取得

            IPlayerStatus myPs = null;
            RoomInfo[] myRooms = null;
            try
            {
                var myServerRes = await API.GetPlayerStatusFromUrlAsync(_dataServer, "http://int-main.net/playerstatus/" + _live_id, _cc);
                if (myServerRes.Success)
                {
                    myPs = myServerRes.PlayerStatus;
                    myRooms = ConvertChannelMsList(myPs.MsList, myPs.DefaultCommunity);
                }
            }
            catch (Exception ex)
            {
                //_logger
            }
            var res = await API.GetPlayerStatusAsync(_dataServer, _live_id, _cc);
            if (!res.Success)
            {
                //fullとかclosedとか。席取りはコメビュで面倒見るものじゃないと思うから、Infoを出して終了。
                //TODO:Infoを送信
                throw new Exception();//TODO:席取れなかったExceptionを作る
            }
            var ps = res.PlayerStatus;
            if (ps.MsList != null)
            {
                //配信者と見做す
                _isBroadcaster = true;
                if (_siteOptions.CanUploadPlayerStatus)
                {
                    //俺のサーバにアップする。
                    await _uploadServer.PostAsync("http://int-main.net/playerstatus/", _live_id + ".xml", ps.Raw);
                }
                var rooms = ConvertChannelMsList(ps.MsList, ps.DefaultCommunity);
                return rooms;
            }

            //ここに来るのはリスナーだけ
            if (myRooms != null)
            {
                return myRooms;
            }
            var list = new List<RoomInfo>();
            //TODO:計算して他の部屋も取得する。
            list.Add(new RoomInfo(ps.Ms, ps.RoomLabel));
            return list.ToArray();
        }
        public async Task ReceiveComments()
        {

        }
        private readonly string _live_id;
        private readonly IDataSource _dataServer;
        private readonly IUploadServer _uploadServer;
        private readonly CookieContainer _cc;
        private bool _isBroadcaster;
        private readonly NicoSiteOptions _siteOptions;
        public ChannelCommentProvider(IDataSource dataServer, IUploadServer uploadServer, string live_id, CookieContainer cc, NicoSiteOptions siteOptions)
        {
            _live_id = live_id;
            _dataServer = dataServer;
            _uploadServer = uploadServer;
            _cc = cc;
            _siteOptions = siteOptions;
        }
    }

    public interface IUploadServer
    {
        Task PostAsync(string url, string filename, string data);
    }
    public class UploadServer : IUploadServer
    {
        public Task PostAsync(string url, string filename, string data)
        {
            HttpContent streamContent = new StringContent(data);
            using (var client = new HttpClient())
            using (var formData = new MultipartFormDataContent())
            {
                formData.Add(streamContent, "ps", filename);
                return client.PostAsync(url, formData);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class chat
    {
        public string thread { get; private set; }
        public int? no { get; private set; }
        private string _vpos_str;
        public long? vpos
        {
            get
            {
                long vpos;
                if (long.TryParse(_vpos_str, out vpos))
                {
                    return vpos;
                }
                else
                    return null;
            }
        }
        public string date_str { get; private set; }
        public DateTime date
        {
            get
            {
                return FromUnixTime(long.Parse(date_str));
            }
        }
        private static DateTime FromUnixTime(long unix)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(unix).ToLocalTime();
        }
        public long? date_usec
        {
            get
            {
                long _date_usec;
                if (long.TryParse(date_usec_str, out _date_usec))
                {
                    return _date_usec;
                }
                else
                    return null;
            }
        }
        private string date_usec_str;
        public string mail { get; private set; }
        public string user_id { get; private set; }
        public int? premium { get; private set; }
        public int? anonymity { get; private set; }
        public string locale { get; private set; }
        public int? score { get; private set; }
        public string text { get; private set; }
        public bool yourpost { get; private set; }
        public string origin { get; private set; }
        public bool IsBsp { get { return text.StartsWith("/press show "); } }
        public string Raw { get; private set; }
        public chat(string strChat)
        {
            Raw = strChat;
            using (var xmlReader = XmlReader.Create(new System.IO.StringReader($"<root>{strChat}</root>")))
            {
                xmlReader.ReadToFollowing("chat");
                int no_;
                if (int.TryParse(xmlReader.GetAttribute("no"), out no_))
                    no = no_;
                _vpos_str = xmlReader.GetAttribute("vpos");
                thread = xmlReader.GetAttribute("thread");
                date_str = xmlReader.GetAttribute("date");
                date_usec_str = xmlReader.GetAttribute("date_usec");
                mail = xmlReader.GetAttribute("mail");
                user_id = xmlReader.GetAttribute("user_id");
                int premium_;
                if (int.TryParse(xmlReader.GetAttribute("premium"), out premium_))
                    premium = premium_;
                int anonymity_;
                if (int.TryParse(xmlReader.GetAttribute("anonymity"), out anonymity_))
                    anonymity = anonymity_;
                locale = xmlReader.GetAttribute("locale");
                int score_;
                if (int.TryParse(xmlReader.GetAttribute("score"), out score_))
                    score = score_;
                yourpost = !string.IsNullOrWhiteSpace(xmlReader.GetAttribute("yourpost"));
                origin = xmlReader.GetAttribute("origin");
                //以前はHtmlConverter.Decode()をしていたけど、ここでは一切加工しないことに。
                text = xmlReader.ReadElementContentAsString();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class thread
    {
        //<thread resultcode="0" thread="1389881167" last_res="6006" ticket="0x32b22900" revision="1" server_time="1414839575"/>
        /// <summary>
        /// 
        /// </summary>
        /// 0:問題なし
        /// 1:失敗
        /// 3:threadにversionを入れずにサーバに送ったら帰ってきた。
        public int? resultcode { get; private set; }
        public string thread_id { get; private set; }
        public int? last_res { get; private set; }
        public string ticket { get; private set; }
        public int? revision { get; private set; }
        public long? server_time { get; private set; }
        public string Raw { get; private set; }
        public thread(string strThread)
        {
            Raw = strThread;
            using (var xmlReader = XmlReader.Create(new System.IO.StringReader($"<root>{strThread}</root>")))
            {
                xmlReader.ReadToFollowing("thread");
                int resultcode_;
                if (int.TryParse(xmlReader.GetAttribute("resultcode"), out resultcode_))
                    resultcode = resultcode_;
                thread_id = xmlReader.GetAttribute("thread");
                int last_res_;
                if (int.TryParse(xmlReader.GetAttribute("last_res"), out last_res_))
                    last_res = last_res_;
                ticket = xmlReader.GetAttribute("ticket");
                int revision_;
                if (int.TryParse(xmlReader.GetAttribute("revision"), out revision_))
                    revision = revision_;
                long server_time_;
                if (long.TryParse(xmlReader.GetAttribute("server_time"), out server_time_))
                    server_time = server_time_;
            }
        }
    }
}
