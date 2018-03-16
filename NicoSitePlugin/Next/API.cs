using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace NicoSitePlugin.Next
{
    class Room : IXmlWsRoomInfo
    {
        public Room(Low.ProgramInfo.Room low)
        {
            if (low == null) throw new ArgumentNullException(nameof(low));
            var match = Regex.Match(low.XmlSocketUri, "^[^:/]+://([^:]+):(\\d+)$");
            if (!match.Success) throw new ParseException(low.XmlSocketUri);
            XmlSocketAddr = match.Groups[1].Value;
            XmlSocketPort = int.Parse(match.Groups[2].Value);
            WebSocketUri = low.WebSocketUri;
            Name = low.Name;
            Id = low.Id;
            ThreadId = low.ThreadId;
        }

        public string XmlSocketAddr { get; set; }

        public int XmlSocketPort { get; set; }

        public string WebSocketUri { get; set; }

        public string Name { get; set; }

        public long Id { get; set; }

        public string ThreadId { get; set; }

        string IRoomInfo.Addr => XmlSocketAddr;

        int IRoomInfo.Port => XmlSocketPort;

        string IRoomInfo.Thread => ThreadId;
    }
    class ProgramInfo : IProgramInfo
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public bool IsMemberOnly { get; private set; }
        public long VposBaseAt { get; private set; }
        public long BeginAt { get; private set; }
        public long EndAt { get; private set; }
        public string Status { get; private set; }
        public List<string> Categories { get; private set; } = new List<string>();
        public List<Room> Rooms { get; private set; } = new List<Room>();
        public ProviderType Type { get; private set; }
        public string ProviderId { get; private set; }
        public string ProviderName { get; private set; }
        public ProgramInfo(Low.ProgramInfo.ProgramInfo low)
        {
            Title = low.Data.Title;
            Description = low.Data.Description;
            IsMemberOnly = low.Data.IsMemberOnly;
            VposBaseAt = low.Data.VposBaseAt;
            BeginAt = low.Data.BeginAt;
            EndAt = low.Data.EndAt;
            Status = low.Data.Status;
            foreach (var category in low.Data.Categories)
            {
                Categories.Add(category);
            }
            foreach (var r in low.Data.Rooms)
            {
                Rooms.Add(new Room(r));
            }
        }
    }
    static class API
    {
        public static async Task<IProgramInfo> GetProgramInfo(IDataSource dataSource, string liveId, CookieContainer cc)
        {
            var url = $"http://live2.nicovideo.jp/watch/{liveId}/programinfo";
            var res = await dataSource.Get(url, cc);
            var low = Tools.Deserialize<Low.ProgramInfo.ProgramInfo>(res);
            var programInfo = new ProgramInfo(low);
            return programInfo;
        }
        public static async Task<HeartbeatResponse> GetHeartbeatAsync(IDataSource dataSource, string liveId, CookieContainer cc)
        {
            var url = $"http://live.nicovideo.jp/api/heartbeat?v={liveId}";
            var res = await dataSource.Get(url, cc);
            var serializer = new XmlSerializer(typeof(Heartbeat));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(res));
            var heartbeat = serializer.Deserialize(ms) as Heartbeat;
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
            var res = await dataSource.Get(url, cc);
            var match = Regex.Match(res, "^postkey=(.*)$");
            if (match.Success)
            {
                var postKey = match.Groups[1].Value;
                return postKey;
            }
            else
            {
                throw new Test.GetPostKeyFailedException(res);
            }
        }
        public static async Task<IPlayerStatusResponse> GetPlayerStatusFromUrlAsync(IDataSource dataSource, string url, CookieContainer cc)
        {
            PlayerStatusResponseTest ret = null;
            string xml = null;
            xml = await dataSource.Get(url, cc);//500とかはcatchしない方が良いだろう
            try
            {
                var serializer = new XmlSerializer(typeof(Low.GetPlayerStatus.Getplayerstatus));
                var bytes = System.Text.Encoding.UTF8.GetBytes(xml);
                Low.GetPlayerStatus.Getplayerstatus ps;
                using (var ms = new System.IO.MemoryStream(bytes))
                {
                    ps = (Low.GetPlayerStatus.Getplayerstatus)serializer.Deserialize(ms);
                }
                if (ps.Status == "ok")
                {
                    var msList = ps.Ms_list?.Ms.Select(ms => new MsTest(ms.Addr, ms.Thread, int.Parse(ms.Port))).Cast<IMs>().ToArray();
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
                        Ms = new MsTest(ps.Ms.Addr, ps.Ms.Thread, int.Parse(ps.Ms.Port)),
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
        public static Task<IPlayerStatusResponse> GetPlayerStatusAsync(IDataSource dataSource, string live_id, CookieContainer cc)
        {
            var url = "http://live.nicovideo.jp/api/getplayerstatus?v=" + live_id;
            return GetPlayerStatusFromUrlAsync(dataSource, url, cc);
        }
        public static async Task<Low.CommunityInfo.RootObject> GetCommunityInfo(IDataSource dataSource, string communityId)
        {
            var url = $"http://api.ce.nicovideo.jp/api/v1/community.info?id={communityId}&__format=json";
            var str = await dataSource.Get(url, null);

            //C#的に不要なエスケープを削除
            str = str.Replace("\"@key\":\"", "\"key\":\"");
            str = str.Replace("\"@status\":\"", "\"status\":\"");

            var communityInfo = Tools.Deserialize<Low.CommunityInfo.RootObject>(str);
            return communityInfo;
        }
    }
}
