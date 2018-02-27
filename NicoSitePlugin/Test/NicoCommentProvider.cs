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
