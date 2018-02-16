using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
namespace NicoSitePlugin.Old
{
    public static class API
    {
        public static async Task<IPlayerStatusResponse> GetPlayerStatusFromUrlAsync(IDataSource dataSource, string url, CookieContainer cc)
        {
            PlayerStatusResponseTest ret = null;            
            string xml = null;
            xml = await dataSource.Get(url, cc);//500とかはcatchしない方が良いだろう
            try
            {
                var serializer = new XmlSerializer(typeof(Low.Getplayerstatus));
                var bytes = System.Text.Encoding.UTF8.GetBytes(xml);
                Low.Getplayerstatus ps;
                using (var ms = new System.IO.MemoryStream(bytes))
                {
                    ps = (Low.Getplayerstatus)serializer.Deserialize(ms);
                }
                if (ps.Status == "ok")
                {
                    var msList = ps.Ms_list?.Ms.Select(ms => new MsTest(ms.Addr, ms.Thread, int.Parse(ms.Port))).Cast<IMs>().ToArray();
                    var playerStatus = new PlayerStatusTest
                    {
                        Raw = xml,
                        Title = ps.Stream.Title,
                        DefaultCommunity=ps.Stream.Default_community,
                        BaseTime = long.Parse(ps.Stream.Base_time),
                        Description = ps.Stream.Description,
                        EndTime = long.Parse(ps.Stream.End_time),
                        IsJoin = ps.User.Is_join == "1",
                        Ms = new MsTest(ps.Ms.Addr, ps.Ms.Thread, int.Parse(ps.Ms.Port)),
                        Nickname = ps.User.Nickname,
                        OpenTime = long.Parse(ps.Stream.Open_time),
                        StartTime = long.Parse(ps.Stream.Start_time),
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
#if DEBUG
            Debug.Assert(Regex.IsMatch(live_id, "lv\\d+"));
#endif
            var url = "http://live.nicovideo.jp/api/getplayerstatus?v=" + live_id;
            return GetPlayerStatusFromUrlAsync(dataSource, url, cc);
        }
        public static async Task<Low.CommunityInfo> GetCommunityInfo(IDataSource dataSource, string communityId)
        {
            var url = $"http://api.ce.nicovideo.jp/api/v1/community.info?id={communityId}&__format=json";
            var str = await dataSource.Get(url, null);

            //C#的に不要なエスケープを削除
            str = str.Replace("\"@key\":\"", "\"key\":\"");
            str = str.Replace("\"@status\":\"", "\"status\":\"");

            var communityInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Low.CommunityInfo>(str);
            return communityInfo;
        }
    }
}
