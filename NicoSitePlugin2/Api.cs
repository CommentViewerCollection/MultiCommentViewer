using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NicoSitePlugin
{
    class ApiGetCommunityLivesException : Exception
    {
        public ApiGetCommunityLivesException()
        {

        }
    }
    class NotLoggedInException : Exception { }
    class UserInfo
    {
        public string Nickname { get; set; }
        public string UserIconUrl { get; set; }

    }
    static class Api
    {
        public static async Task<UserInfo> GetUserInfo(IDataSource server, CookieContainer cc, string userId)
        {
            var url = $"https://public.api.nicovideo.jp/v1/users.json?userIds={userId}";
            var res = await server.GetAsync(url, cc);
            var obj = JsonConvert.DeserializeObject<NicoSitePlugin2.Low.UserInfo.RootObject>(res);
            if (obj.Data.Length == 0)
            {
                throw new ArgumentException("指定されたuserIdは存在しない:" + userId);
            }
            var data = obj.Data[0];
            var userInfo = new UserInfo
            {
                Nickname = data.Nickname,
                UserIconUrl = data.Icons.Urls.The150X150,
            };
            return userInfo;
        }
        public static async Task<CommunityLiveInfo[]> GetCommunityLives(IDataSource server, CookieContainer cc, string communityId)
        {
            //以下のAPIだとON_AIRだけ取れる。
            //https://com.nicovideo.jp/api/v1/communities/{communityIdWithoutCo}/lives/onair.json
            //でも配信していないと
            //{"meta":{"status":404,"error-code":"RESOURCE_NOT_FOUND","error-message":"このコミュニティは生放送中ではありません。"}}
            //が返ってくる


            var communityIdWithoutCo = communityId.Substring(2);
            var url = $"https://com.nicovideo.jp/api/v1/communities/{communityIdWithoutCo}/lives.json";
            var res = await server.GetAsync(url, cc);
            dynamic d = JsonConvert.DeserializeObject(res);
            if (d.meta.status != 200)
            {
                throw new ApiGetCommunityLivesException();
            }
            var lives = new List<CommunityLiveInfo>();
            foreach (var liveDyn in d.data.lives)
            {
                var live = new CommunityLiveInfo
                {
                    Description = (string)liveDyn.description,
                    LiveId = (string)liveDyn.id,
                    Status = (string)liveDyn.status,
                    Title = (string)liveDyn.title,
                    UserId = (long)liveDyn.user_id,
                    WatchUrl = (string)liveDyn.watch_url,
                };
                lives.Add(live);
            }
            return lives.ToArray();
        }
        public class CommunityLiveInfo
        {
            public string LiveId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Status { get; set; }
            public long UserId { get; set; }
            public string WatchUrl { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        /// <exception cref="NotLoggedInException">未ログインの場合</exception>
        /// <exception cref="SpecChangedException"></exception>
        public static async Task<MyInfo> GetMyInfo(IDataSource server, CookieContainer cc)
        {
            var url = "https://www.nicovideo.jp/my";
            var html = await server.GetAsync(url, cc);
            var match = Regex.Match(html, "data-common-header=\"(.+)\"></div>");
            if (!match.Success)
            {
                var matchHtmlTitle = Regex.Match(html, "<title>([^<]*)</title>");
                var title = matchHtmlTitle.Groups[1].Value;
                if (matchHtmlTitle.Success && title.Contains("ログイン"))
                {
                    throw new NotLoggedInException();
                }
                else
                {
                    throw new SpecChangedException(html);
                }
            }
            var json = match.Groups[1].Value.Replace("&quot;", "\"");
            dynamic d = JsonConvert.DeserializeObject(json);
            var isLogin = (bool)d.initConfig.user.isLogin;
            var userId = (long)d.initConfig.user.id;
            var isPremium = (bool)d.initConfig.user.isPremium;
            var nickname = (string)d.initConfig.user.nickname;
            var iconUrl = (string)d.initConfig.user.iconUrl;

            return new MyInfo
            {
                IsLogin = isLogin,
                Nickname = nickname,
                IconUrl = iconUrl,
                IsPremium = isPremium,
                UserId = userId.ToString(),
            };
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
        /// 指定されたコミュニティの現在の配信のIDを取得する
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="communityId">co\d+</param>
        /// <returns>配信中であればその配信のID、そうでなければnull</returns>
        internal static async Task<string> GetCurrentCommunityLiveId(IDataSource dataSource, string communityId, CookieContainer cc)
        {
            var lives = await GetCommunityLives(dataSource, cc, communityId);
            return lives.FirstOrDefault(a => a.Status == "ON_AIR")?.LiveId;
        }
    }
}
