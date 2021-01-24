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
            var html = await server.GetAsync(url,cc);
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

    }
}
