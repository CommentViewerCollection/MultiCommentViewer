using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Codeplex.Data;
using NicoSitePlugin.Next20181012;
namespace NicoSitePlugin
{
    interface IJikkyoInfo: IXmlSocketRoomInfo
    {
        string BaseTime { get; }
        string OpenTime { get; }
        string StartTime { get; }
        string EndTime { get; }

    }
    class JikkyoInfo: IJikkyoInfo
    {
        public string XmlSocketAddr { get; set; }
        public int XmlSocketPort { get; set; }
        public string ThreadId { get; set; }
        public string Name { get; set; }
        public string BaseTime { get; set; }
        public string OpenTime { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
    interface INicoCasUserInfo
    {
        string Name { get; }
        string MessageServerUrlWss { get; }
        List<INicoCasLiveInfo> Lives { get; }

    }
    interface INicoCasLiveInfo
    {
        string Id { get; }
        string Title { get; }
        string Description { get; }
        DateTime OnAirTimeBeginAt { get; }
        DateTime OnAirTimeEndAt { get; }
        DateTime ShowTimeBeginAt { get; }
        DateTime ShowTimeEndAt { get; }
        int Viewers { get; }
        int Comments { get; }
    }
    class NicoCasLiveInfo : INicoCasLiveInfo
    {
        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public DateTime OnAirTimeBeginAt { get; }
        public DateTime OnAirTimeEndAt { get; }
        public DateTime ShowTimeBeginAt { get; }
        public DateTime ShowTimeEndAt { get; }
        public int Viewers { get; }
        public int Comments { get; }
        public NicoCasLiveInfo(Low.NicoCasUserInfo.ContentGroupItem low)
        {
            Id = low.Id;
            Title = low.Title;
            Description = low.Description;
            OnAirTimeBeginAt = low.OnAirTime.BeginAt.DateTime;
            OnAirTimeEndAt = low.OnAirTime.EndAt.DateTime;
            ShowTimeBeginAt = low.ShowTime.BeginAt.DateTime;
            ShowTimeEndAt = low.ShowTime.EndAt.DateTime;
            Viewers = (int)low.Viewers;
            Comments = (int)low.Comments;
        }
    }
    class NicoCasUserInfo : INicoCasUserInfo
    {
        public string Name { get; }
        public string MessageServerUrlWss { get; }
        public List<INicoCasLiveInfo> Lives { get; } = new List<INicoCasLiveInfo>();
        public NicoCasUserInfo(Low.NicoCasUserInfo.RootObject low)
        {
            Name = low.Data.Name;
            MessageServerUrlWss = low.Data.MessageServer.Wss;
            foreach(var content in low.Data.ContentGroups)
            {
                //content.GroupId
                //live
                //video
                if(content.GroupId == "live")
                {
                    foreach(var live in content.Items)
                    {
                        var liveInfo = new NicoCasLiveInfo(live);
                        Lives.Add(liveInfo);
                    }
                }
                else
                {
                    Debugger.Break();
                }
            }
        }
    }
    static class API
    {
        public static async Task<INicoCasUserInfo> GetNicoCasUserInfo(IDataSource dataSource, string userId)
        {
            var url = "https://api.cas.nicovideo.jp/v1/tanzakus/user/" + userId;
            var res = await dataSource.GetAsync(url);
            var low = Tools.Deserialize<Low.NicoCasUserInfo.RootObject>(res);
            var userInfo = new NicoCasUserInfo(low);
            return userInfo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="channelId">jk123の数字部分</param>
        /// <returns></returns>
        /// <exception cref="JikkyoInfoFailedException"></exception>
        public static async Task<IJikkyoInfo> GetJikkyoInfoAsync(IDataSource dataSource, int channelId)
        {
            var url = "http://jk.nicovideo.jp/api/v2/getflv?v=jk" + channelId;
            var res = await dataSource.GetAsync(url);
            var kvs = res.Split('&');
            var dict = new Dictionary<string, string>();
            foreach (var kv in kvs)
            {
                var arr = kv.Split('=');
                var key = arr[0];
                var val = arr[1];
                dict.Add(key, HttpUtility.UrlDecode(val));
            }
            if (dict.ContainsKey("error"))
            {
                throw new JikkyoInfoFailedException(res, url, dict["error"]);
            }
            var jikkyoInfo = new JikkyoInfo()
            {
                XmlSocketAddr = dict["ms"],
                XmlSocketPort = int.Parse(dict["ms_port"]),
                Name = dict["channel_name"],
                ThreadId = dict["thread_id"],
                BaseTime = dict["base_time"],
                OpenTime = dict["open_time"],
                StartTime = dict["start_time"],
                EndTime = dict["end_time"],
            };
            return jikkyoInfo;
        }
        public static async Task<IProgramInfo> GetProgramInfo(IDataSource dataSource, string liveId, CookieContainer cc)
        {
            var url = $"http://live2.nicovideo.jp/watch/{liveId}/programinfo";
            //ログイン必須
            //未ログイン時は401
            //{"meta":{"status":401,"errorCode":"UNAUTHORIZED"}}
            var res = await dataSource.GetAsync(url, cc);
            var low = Tools.Deserialize<Low.ProgramInfo.RootObject>(res);
            var programInfo = new ProgramInfo(low);
            return programInfo;
        }
        public static async Task<HeartbeatResponse> GetHeartbeatAsync(IDataSource dataSource, string liveId, CookieContainer cc)
        {
            var url = $"http://live.nicovideo.jp/api/heartbeat?v={liveId}";
            var res = await dataSource.GetAsync(url, cc);
            var serializer = new XmlSerializer(typeof(Low.Heartbeat.RootObject));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(res));
            var heartbeat = serializer.Deserialize(ms) as Low.Heartbeat.RootObject;
            if (heartbeat.Status == "ok")
            {
                return new HeartbeatResponse((IHeartbeat)heartbeat);
            }
            else
            {
                return new HeartbeatResponse((IHeartbeartFail)heartbeat);
            }
        }
        public static async Task<string> GetPostKey(IDataSource dataSource, string threadId, int blockNo, CookieContainer cc)
        {
            var url = $"http://live.nicovideo.jp/api/getpostkey?thread={threadId}&block_no={blockNo}&uselc=1&locale_flag=1&seat_flag=1&lang_flag=1";
            var res = await dataSource.GetAsync(url, cc);
            var match = Regex.Match(res, "^postkey=(.*)$");
            if (match.Success)
            {
                var postKey = match.Groups[1].Value;
                return postKey;
            }
            else
            {
                throw new GetPostKeyFailedException(res);
            }
        }
        public static async Task<IPlayerStatusResponse> GetPlayerStatusAsync(IDataSource dataSource, string live_id, CookieContainer cc)
        {
            var url = "http://live.nicovideo.jp/api/getplayerstatus?v=" + live_id;
            PlayerStatusResponseTest ret = null;
            string xml = null;
            xml = await dataSource.GetAsync(url, cc);//500とかはcatchしない方が良いだろう
            try
            {
                var serializer = new XmlSerializer(typeof(Low.GetPlayerStatus.RootObject));
                var bytes = System.Text.Encoding.UTF8.GetBytes(xml);
                Low.GetPlayerStatus.RootObject ps;
                using (var ms = new System.IO.MemoryStream(bytes))
                {
                    ps = (Low.GetPlayerStatus.RootObject)serializer.Deserialize(ms);
                }
                if (ps.Status == "ok")
                {
                    var msList = ps.Ms_list?.Ms.Select(ms => new MsTest(ms.Addr, int.Parse(ms.Port), ms.Thread)).Cast<IMs>().ToArray();
                    var playerStatus = new PlayerStatusTest
                    {
                        Raw = xml,
                        Title = ps.Stream.Title,
                        DefaultCommunity = ps.Stream.Default_community,
                        BaseTime = int.Parse(ps.Stream.Base_time),
                        Description = ps.Stream.Description,
                        EndTime = int.Parse(ps.Stream.End_time),
                        IsJoin = ps.User.Is_join == "1",
                        IsPremium = ps.User.Is_premium,
                        Ms = new MsTest(ps.Ms.Addr, int.Parse(ps.Ms.Port), ps.Ms.Thread),
                        Nickname = ps.User.Nickname,
                        OpenTime = int.Parse(ps.Stream.Open_time),
                        StartTime = int.Parse(ps.Stream.Start_time),
                        ProviderType = Tools.Convert(ps.Stream.Provider_type),
                        UserId = ps.User.User_id,
                        RoomSeetNo = int.Parse(ps.User.Room_seetno),
                        RoomLabel = ps.User.Room_label,
                        MsList = msList ?? new IMs[0],
                    };
                    ret = new PlayerStatusResponseTest(playerStatus);
                }
                else if (ps.Status == "fail")
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(xml);
                    var root = doc.DocumentElement;
                    var codeStr = root.SelectSingleNode("error/code").InnerText;
                    var code = Tools.ConvertErrorCode(codeStr);
                    ret = new PlayerStatusResponseTest(new PlayerStatusError(code));
                }
                else
                {
                    ret = new PlayerStatusResponseTest(new PlayerStatusError(ErrorCode.unknown));
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.Assert(xml != null);
                try
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(xml);
                    var root = doc.DocumentElement;
                    var codeStr = root.SelectSingleNode("error/code").InnerText;
                    var code = Tools.ConvertErrorCode(codeStr);
                    ret = new PlayerStatusResponseTest(new PlayerStatusError(code));
                }
                catch (Exception ex1)
                {
                    //UnknownResponseException
                    Debug.WriteLine(ex1.Message);
                }
            }
            catch (Exception ex)
            {
                //UnknownResponseException
                Debug.WriteLine(ex.Message);
            }
            return ret;
        }
        public static async Task<Low.CommunityInfo.RootObject> GetCommunityInfo(IDataSource dataSource, string communityId)
        {
            var url = $"http://api.ce.nicovideo.jp/api/v1/community.info?id={communityId}&__format=json";
            var str = await dataSource.GetAsync(url, null);

            //C#的に不要なエスケープを削除
            str = str.Replace("\"@key\":\"", "\"key\":\"");
            str = str.Replace("\"@status\":\"", "\"status\":\"");

            var communityInfo = Tools.Deserialize<Low.CommunityInfo.RootObject>(str);
            return communityInfo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static async Task<string> GetDisplayNameFromUserId(IDataSource dataSource, string userId)
        {
            var url = "http://seiga.nicovideo.jp/api/user/info?id=" + userId;
            var res = await dataSource.GetAsync(url);
            //<?xml version="1.0" encoding="UTF-8"?>
            //<response>
            //	<user>
            //		<id>2297426</id>
            //		<nickname>Ryu</nickname>
            //	</user>
            //</response>
            var match = Regex.Match(res, "<nickname>([^<]+)</nickname>");
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
        /// 指定されたコミュニティの現在の配信のIDを取得する
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="communityId">co\d+</param>
        /// <returns>配信中であればその配信のID、そうでなければnull</returns>
        internal static async Task<string> GetCurrentCommunityLiveId(IDataSource dataSource, string communityId,CookieContainer cc)
        {
            //TODO:自動認証じゃないコミュニティの場合、Cookieが無いと入れない
            var url = "https://com.nicovideo.jp/community/" + communityId;
            //コミュニティフォロワーではありません。 （いわゆるclosed community）の場合403が返ってくる
            var res = await dataSource.GetAsync(url, cc);
            var match = Regex.Match(res, "(<section class=\"now_live.+?</section>)", RegexOptions.Singleline);
            if (!match.Success) return null;
            var nowLiveSection = match.Groups[1].Value;
            var liveId = Tools.ExtractLiveId(nowLiveSection);
            return liveId;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="channelId">channelId/screenName</param>
        /// <returns></returns>
        internal static async Task<string> GetCurrentChannelLiveId(IDataSource dataSource, string channelId)
        {
            var url = "http://ch.nicovideo.jp/" + channelId;
            var res = await dataSource.GetAsync(url);
            var match = Regex.Match(res, "(data-live_status=\"onair\".+?</span>)", RegexOptions.Singleline);
            if (!match.Success) return null;
            var nowLiveSection = match.Groups[1].Value;
            var liveId = Tools.ExtractLiveId(nowLiveSection);
            return liveId;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="screenName"></param>
        /// <returns>ch\d+</returns>
        public static async Task<string> GetChannelIdFromScreenName(IDataSource dataSource, string screenName)
        {
            //access-ann 2547495
            var url = "http://ch.nicovideo.jp/" + screenName;
            var html = await dataSource.GetAsync(url);
            var match = Regex.Match(html, "(ch\\d+)", RegexOptions.Singleline);
            if (!match.Success) return null;
            var id = match.Groups[1].Value;
            return id;
        }

        public static async Task<IUserInfo> GetUserInfo(IDataSource dataSource, string userId)
        {
            var url = $"http://api.ce.nicovideo.jp/api/v1/user.info?user_id={userId}&__format=json";
            //正常なレスポンス
            //{"nicovideo_user_response":{"user":{"id":"2297426","nickname":"Ryu","thumbnail_url":"http:\/\/dcdn.cdn.nimg.jp\/nicoaccount\/usericon\/229\/2297426.jpg?1477771628"},"vita_option":{"user_secret":"0"},"additionals":"","@status":"ok"}}
            //存在しないuser_idを指定した時のレスポンス
            //{"nicovideo_user_response":{"error":{"code":"NOT_FOUND","description":"\u30e6\u30fc\u30b6\u30fc\u304c\u898b\u3064\u304b\u308a\u307e\u305b\u3093"},"@status":"fail"}}
            var res = await dataSource.GetAsync(url);
            res = res.Replace("\"@status\":", "\"status\":");
            try
            {
                var json = DynamicJson.Parse(res);

                if(json.nicovideo_user_response.status == "ok")
                {
                    var d_user = json.nicovideo_user_response.user;
                    var userInfo = new UserInfo
                    {
                        Name = d_user.nickname,
                        ThumbnailUrl = d_user.thumbnail_url,
                        UserId = d_user.id,
                    };
                    return userInfo;
                }
            }
            catch (Exception ex)
            {
                throw new ParseException(res, ex);
            }
            throw new ParseException(res);
        }

        public static async Task<WatchDataProps> GetWatchDataProps(IDataSource server, string liveId, CookieContainer cc)
        {
            var url = "https://live2.nicovideo.jp/watch/" + liveId;
            string res;
            try
            {
                res = await server.GetAsync(url, cc);
            }
            catch (HttpException ex)
            {
                throw new NotImplementedException();
            }
            //quot;}}}}}}"></script><script src="http
            var match = Regex.Match(res, "data-props=\"({.+?})\"></script>");
            if (!match.Success)
            {
                //コミュニティフォロワー限定番組です
                throw new NotImplementedException();
            }
            var raw = match.Groups[1].Value;
            raw = raw.Replace("&quot;", "\"");
            var obj = Tools.Deserialize<Low.WatchDataProps.RootObject>(raw);
            return new WatchDataProps(obj);
        }

        class UserInfo : IUserInfo
        {
            public string UserId { get; set; }
            public string ThumbnailUrl { get; set; }
            public string Name { get; set; }
        }
    }
    public interface IUserInfo
    {
        string UserId { get; }
        string ThumbnailUrl { get; }
        string Name { get; }
    }
    public class WatchDataProps
    {
        public string UserId { get; }
        public string AudienceToken { get; }
        public string Status { get; }
        public string Title { get; }
        public bool IsLoggedIn { get; }
        public bool IsBroadcaster { get; }
        public string AccountType { get; }
        public bool IsOperator { get; }
        public string BroadcastId { get; }
        public string WebSocketUrl { get; }
        public string Locale { get; }
        public long ServerTime { get; }
        public WatchDataProps(Low.WatchDataProps.RootObject low)
        {
            AudienceToken = low.Player.AudienceToken;
            Status = low.Program.Status;
            Title = low.Program.Title;
            IsLoggedIn = low.User.IsLoggedIn;
            IsBroadcaster = low.User.IsBroadcaster;
            AccountType = low.User.AccountType;
            IsOperator = low.User.IsOperator;
            BroadcastId = low.Program.BroadcastId;
            WebSocketUrl=low.Site.Relive.WebSocketUrl;
            Locale = low.Site.Locale;
            UserId = low.User.Id;
            ServerTime = low.Site.ServerTime;
        }
    }
}
