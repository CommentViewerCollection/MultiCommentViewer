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

namespace NicoSitePlugin.Old
{
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
            var list = new List<RoomInfo>
            {
                //TODO:計算して他の部屋も取得する。
                new RoomInfo(ps.Ms, ps.RoomLabel)
            };
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
    public class Chat
    {
        public string Thread { get; private set; }
        public int? No { get; private set; }
        private string _vpos_str;
        public long? Vpos
        {
            get
            {
                if (long.TryParse(_vpos_str, out long vpos))
                {
                    return vpos;
                }
                else
                    return null;
            }
        }
        public string DateStr { get; private set; }
        public DateTime Date
        {
            get
            {
                return FromUnixTime(long.Parse(DateStr));
            }
        }
        private static DateTime FromUnixTime(long unix)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(unix).ToLocalTime();
        }
        public long? DateUsec
        {
            get
            {
                if (long.TryParse(_date_usec_str, out long _date_usec))
                {
                    return _date_usec;
                }
                else
                    return null;
            }
        }
        private string _date_usec_str;
        public string Mail { get; private set; }
        public string UserId { get; private set; }
        public int? Premium { get; private set; }
        public int? Anonymity { get; private set; }
        public string Locale { get; private set; }
        public int? Score { get; private set; }
        public string Text { get; private set; }
        public bool Yourpost { get; private set; }
        public string Origin { get; private set; }
        public bool IsBsp { get { return Text.StartsWith("/press show "); } }
        public string Raw { get; private set; }
        public Chat(string strChat)
        {
            Raw = strChat;
            using (var xmlReader = XmlReader.Create(new System.IO.StringReader($"<root>{strChat}</root>")))
            {
                xmlReader.ReadToFollowing("chat");
                if (int.TryParse(xmlReader.GetAttribute("no"), out int no_))
                    No = no_;
                _vpos_str = xmlReader.GetAttribute("vpos");
                Thread = xmlReader.GetAttribute("thread");
                DateStr = xmlReader.GetAttribute("date");
                _date_usec_str = xmlReader.GetAttribute("date_usec");
                Mail = xmlReader.GetAttribute("mail");
                UserId = xmlReader.GetAttribute("user_id");
                if (int.TryParse(xmlReader.GetAttribute("premium"), out int premium_))
                    Premium = premium_;
                if (int.TryParse(xmlReader.GetAttribute("anonymity"), out int anonymity_))
                    Anonymity = anonymity_;
                Locale = xmlReader.GetAttribute("locale");
                if (int.TryParse(xmlReader.GetAttribute("score"), out int score_))
                    Score = score_;
                Yourpost = !string.IsNullOrWhiteSpace(xmlReader.GetAttribute("yourpost"));
                Origin = xmlReader.GetAttribute("origin");
                //以前はHtmlConverter.Decode()をしていたけど、ここでは一切加工しないことに。
                Text = xmlReader.ReadElementContentAsString();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Thread
    {
        //<thread resultcode="0" thread="1389881167" last_res="6006" ticket="0x32b22900" revision="1" server_time="1414839575"/>
        /// <summary>
        /// 
        /// </summary>
        /// 0:問題なし
        /// 1:失敗
        /// 3:threadにversionを入れずにサーバに送ったら帰ってきた。
        public int? Resultcode { get; private set; }
        public string ThreadId { get; private set; }
        public int? LastRes { get; private set; }
        public string Ticket { get; private set; }
        public int? Revision { get; private set; }
        public long? ServerTime { get; private set; }
        public string Raw { get; private set; }
        public Thread(string strThread)
        {
            Raw = strThread;
            using (var xmlReader = XmlReader.Create(new System.IO.StringReader($"<root>{strThread}</root>")))
            {
                xmlReader.ReadToFollowing("thread");
                if (int.TryParse(xmlReader.GetAttribute("resultcode"), out int resultcode_))
                    Resultcode = resultcode_;
                ThreadId = xmlReader.GetAttribute("thread");
                if (int.TryParse(xmlReader.GetAttribute("last_res"), out int last_res_))
                    LastRes = last_res_;
                Ticket = xmlReader.GetAttribute("ticket");
                if (int.TryParse(xmlReader.GetAttribute("revision"), out int revision_))
                    Revision = revision_;
                if (long.TryParse(xmlReader.GetAttribute("server_time"), out long server_time_))
                    ServerTime = server_time_;
            }
        }
    }
}
