using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
namespace LineLiveSitePlugin
{
    public interface IPromptyStats
    {
        long LoveCount { get; }
        long FreeLoveCount { get; }
        long PremiumLoveCount { get; }
        long LimitedLoveCount { get; }
        long OwnedLimitedLoveCount { get; }
        long SentLimitedLoveCount { get; }
        long ViewerCount { get; }
        long ChatCount { get; }
        //"LIVE"
        string LiveStatus { get; }
        //LiveStartedAtは常にnull
        int ApiStatusCode { get; }
        object PinnedMessage { get; }
        int Status { get; }
    }
    internal class PromptyStats : IPromptyStats
    {
        public long LoveCount { get; set; }
        public long FreeLoveCount { get; set; }
        public long PremiumLoveCount { get; set; }
        public long LimitedLoveCount { get; set; }
        public long OwnedLimitedLoveCount { get; set; }
        public long SentLimitedLoveCount { get; set; }
        public long ViewerCount { get; set; }
        public long ChatCount { get; set; }
        public string LiveStatus { get; set; }
        public int ApiStatusCode { get; set; }
        public object PinnedMessage { get; set; }
        public int Status { get; set; }
        public PromptyStats(Low.PromptyStats.RootObject low)
        {
            LoveCount = low.LoveCount;
            FreeLoveCount = low.FreeLoveCount;
            PremiumLoveCount = low.PremiumLoveCount;
            LimitedLoveCount = low.LimitedLoveCount;
            OwnedLimitedLoveCount = low.OwnedLimitedLoveCount;
            SentLimitedLoveCount = low.SentLimitedLoveCount;
            ViewerCount = low.ViewerCount;
            ChatCount = low.ChatCount.HasValue ? low.ChatCount.Value : 0;
            LiveStatus = low.LiveStatus;
            ApiStatusCode = low.ApistatusCode;
            PinnedMessage = low.PinnedMessage;
            Status = low.Status;
        }
    }
    internal static class Api
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="channelId"></param>
        /// <param name="liveId"></param>
        /// <exception cref="ParseException"></exception>
        /// <returns></returns>
        public static async Task<IPromptyStats> GetPromptyStats(IDataServer server, string channelId, string liveId)
        {
            var url = $"https://live-burst-api.line-apps.com/burst/app/channel/{channelId}/broadcast/{liveId}/promptly_stats";
            var res = await server.GetAsync(url);
            var low = Tools.Deserialize<Low.PromptyStats.RootObject>(res);
            return new PromptyStats(low);
        }
        public static async Task<IPromptyStats> GetPromptyStatsV4(IDataServer server, string channelId, string liveId)
        {
            var url = $"https://live-burst-api.line-apps.com/burst/web/v4.0/channel/{channelId}/broadcast/{liveId}/promptly_stats";
            var res = await server.GetAsync(url);
            var low = Tools.Deserialize<Low.PromptyStats.RootObject>(res);
            return new PromptyStats(low);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        public static async Task<long[]> GetBlockList(IDataServer server, List<Cookie> cookies)
        {
            string accessToken = null;
            foreach (var cookie in cookies)
            {
                if (cookie.Name == "linelive")
                {
                    accessToken = cookie.Value;
                }
            }
            if (string.IsNullOrEmpty(accessToken))
            {
                //未ログイン。どうせ情報を取得できないから諦める。
                return null;
            }
            var url = "https://live-api.line-apps.com/app/setting/blocklist/bulk";
            var headers = new Dictionary<string, string>()
            {
                {"X-CastService-WebClient-AccessToken", accessToken },
            };
            var cc = Tools.CreateCookieContainer(cookies);
            var res = await server.GetAsync(url, headers, cc);
            //{"blockedUserIds":[1983766],"apistatusCode":200,"status":200}
            //{"blockedUserIds":[316787,1983766],"apistatusCode":200,"status":200}
            var d = Codeplex.Data.DynamicJson.Parse(res);
            if (d.status == 200)
            {
                return (long[])d.blockedUserIds;
            }
            else
            {
                return null;
            }
        }
        public static async Task<(ILiveInfo, string raw)> GetLiveInfo(IDataServer server, string channelId, string liveId)
        {
            var url = $"https://live-api.line-apps.com/app/v2/channel/{channelId}/broadcast/{liveId}";
            var s = await server.GetAsync(url);
            var liveInfoLow = Tools.Deserialize<LineLiveSitePlugin.Low.LiveInfo.RootObject>(s);
            var liveInfo = Tools.Parse(liveInfoLow);
            return (liveInfo, s);
        }
        public static async Task<(ILiveInfo, string raw)> GetLiveInfoV4(IDataServer server, string channelId, string liveId)//, CookieContainer cc)
        {
            var url = $"https://live-api.line-apps.com/web/v4.0/channel/{channelId}/broadcast/{liveId}";
            var res = await server.GetAsync(url);//, cc);
            //{"status":4030005,"errorMessage":"LIVE Gateway Failed","apistatusCode":4030005}
            dynamic d = Tools.Deserialize(res);
            var liveInfo = new LiveInfo
            {
                ChatUrl = (string)d.chat.url,
                LiveStatus = (string)d.item.liveStatus,
                Title = (string)d.item.title,
            };
            return (liveInfo, res);
        }
        public static async Task<(LineLiveSitePlugin.Low.ChannelInfo.RootObject, string raw)> GetChannelInfo(IDataServer server, string channelId)
        {
            var url = $"https://live-api.line-apps.com/app/channel/{channelId}";
            var s = await server.GetAsync(url);
            var channelInfo = Tools.Deserialize<LineLiveSitePlugin.Low.ChannelInfo.RootObject>(s);
            return (channelInfo, s);
        }
        public static async Task<LineLive.Api.Loves> GetLovesV4(IDataServer server)
        {
            //https://live-api.line-apps.com/web/v4.0/billing/gift/loves?storeType=WEB&channelId={channelId}&broadcastId={broadcastId}
            var url = "https://live-api.line-apps.com/web/v4.0/billing/gift/loves";
            var headers = new Dictionary<string, string>
            {
                {"Accept-Language","ja" },
            };
            var s = await server.GetAsync(url,headers,new CookieContainer());
            var loves = LineLive.Api.Loves.Parse(s);
            return loves;
        }
        public static async Task<Me> GetMyAsync(IDataServer server, List<Cookie> cookies)
        {
            //Cookie: _ga=GA1.2.1887758210.1492703493; _trmccid=8791bd77daaaeab8; _ldbrbid=tr_dc24b04cfc4630fac6c9301351e483618c2d164d9e7449be37fb5890502714b5; ldsuid=y2iOYFvTNEm4X4S1GYs6Ag==; _trmcuser={"id":""}; _ga=GA1.3.1887758210.1492703493; linelive=c7cc59e8f126353cb23192827520afe5f25e8a893efb791d06d70b69a6f70e15; _trmcdisabled2=-1; _trmcsession={"id":"3f8174e641d2b20b","path":"/","query":"","params":{},"time":1541067304256}; _gid=GA1.3.1706626909.1541067304; __try__=1541067317211; _gat=1
            string accessToken = null;
            foreach (var cookie in cookies)
            {
                if (cookie.Name == "linelive")
                {
                    accessToken = cookie.Value;
                }
            }
            if (string.IsNullOrEmpty(accessToken))
            {
                //未ログイン。どうせ情報を取得できないから諦める。
                return null;
            }
            var url = "https://live-api.line-apps.com/app/my";
            var headers = new Dictionary<string, string>()
            {
                {"X-CastService-WebClient-AccessToken", accessToken },
            };
            var cc = Tools.CreateCookieContainer(cookies);
            var res = await server.GetAsync(url, headers, cc);
            var low = JsonConvert.DeserializeObject<Low.My.RootObject>(res);
            var me = new Me
            {
                DisplayName = low.User.DisplayName,
                UserId = low.User.Id.ToString(),
            };
            return me;
        }
    }
    class Me
    {
        public string DisplayName { get; set; }
        public string UserId { get; set; }
    }
}
