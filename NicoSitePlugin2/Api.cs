using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;

namespace NicoSitePlugin
{
    static class Api
    {
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
    }
}
