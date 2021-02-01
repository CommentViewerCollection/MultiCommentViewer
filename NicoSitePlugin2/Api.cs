using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using Org.BouncyCastle.Utilities.IO;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace NicoSitePlugin
{
    class ApiGetCommunityLivesException : Exception
    {
        public ApiGetCommunityLivesException()
        {

        }
    }
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
            var data = obj.Data[0];
            var userInfo = new UserInfo
            {
                Nickname = data.Nickname,
                UserIconUrl = data.Icons.Urls.The150X150,
            };
            return userInfo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="server"></param>
        /// <param name="cc"></param>
        /// <param name="communityId">co\d+</param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static async Task<NicoSitePlugin2.Low.CommunityLives.Live[]> GetCommunityLives(IDataSource server, CookieContainer cc, string communityId, int limit, int offset)
        {
            var url = $"https://com.nicovideo.jp/api/v1/communities/{communityId.Substring(2)}/lives.json?limit={limit}&offset={offset}";
            var res = await server.GetAsync(url, cc);
            dynamic d = JsonConvert.DeserializeObject(res);
            //{"meta":{"status":404,"error-code":"RESOURCE_NOT_FOUND","error-message":"対象のコミュニティが存在しません。"}}
            //{"meta":{"status":404,"error-code":"RESOURCE_NOT_FOUND","error-message":"リソース communities の値の形式が不正です"}}
            var status = (int)d.meta.status;
            if (status == 200)
            {
                var obj = JsonConvert.DeserializeObject<NicoSitePlugin2.Low.CommunityLives.RootObject>(res);
                return obj.Data.Lives;

            }
            else
            {
                throw new ApiGetCommunityLivesException();
            }
        }
        public static async Task<MyInfo> GetMyInfo(IDataSource server, CookieContainer cc)
        {
            var url = "https://com.nicovideo.jp/community/co1034396";
            var html = await server.GetAsync(url, cc);
            var match = Regex.Match(html, "user:\\s*({[^}]+?})");
            if (!match.Success)
            {
                throw new SpecChangedException(html);
            }
            var data = match.Groups[1].Value;
            var myInfo = new MyInfo();
            var matchUserId = Regex.Match(data, "id\\s*:\\s*(\\d+)");
            if (matchUserId.Success)
            {
                myInfo.UserId = matchUserId.Groups[1].Value;
            }
            var matchNickname = Regex.Match(data, "nickname\\s*:\\s*'([^']+)'");
            if (matchNickname.Success)
            {
                myInfo.Nickname = matchNickname.Groups[1].Value;
            }
            var matchIcon = Regex.Match(data, "iconUrl\\s*:\\s*'([^']+)'");
            if (matchIcon.Success)
            {
                myInfo.IconUrl = matchIcon.Groups[1].Value;
            }
            var matchPremium = Regex.Match(data, "isPremium\\s*:\\s*(true|false)");
            if (matchPremium.Success)
            {
                myInfo.IsPremium = matchPremium.Groups[1].Value == "true";
            }
            var matchLogin = Regex.Match(data, "isLogin\\s*:\\s*(true|false)");
            if (matchLogin.Success)
            {
                myInfo.IsLogin = matchLogin.Groups[1].Value == "true";
            }


            return myInfo;
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
            var lives = await GetCommunityLives(dataSource, cc, communityId, 1, 0);
            if (lives.Length > 0 && lives[0].Status == "ON_AIR")
            {
                return lives[0].Id;
            }
            else
            {
                return null;
            }

        }
    }
}
