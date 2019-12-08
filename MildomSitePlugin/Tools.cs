using ryu_s.BrowserCookie;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MildomSitePlugin
{
    static class Tools
    {
        public static IMyUserInfo GetUserInfoFromCookie(IBrowserProfile browserProfile)
        {
            var cookies = browserProfile.GetCookieCollection("mildom.com");
            if (cookies.Exists(item => item.Name == "user"))
            {
                var cookie = cookies.Where(item => item.Name == "user").First();
                var val = cookie.Value;
                var decoded = System.Web.HttpUtility.UrlDecode(val);
                var userInfoLow = Tools.Deserialize<Low.UserInfo.RootObject>(decoded);
                return new LoggedinUserInfo(userInfoLow);
            }
            else if (cookies.Exists(item => item.Name == "gid"))
            {
                var cookie = cookies.Where(item => item.Name == "gid").First();
                var gid = cookie.Value;
                var guestName = Tools.CreateGuestName();
                return new AnonymousUserInfo(gid, guestName);
            }
            else
            {
                var gid = "";
                var guestName = Tools.CreateGuestName();
                return new AnonymousUserInfo(gid, guestName);
            }
        }
        public static string CreateGuestName()
        {
            var rnd = new Random();
            var num = rnd.Next(100000, 1000000);
            return "guest" + num;
        }
        public static DateTime UnixTime2DateTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        }
        public static bool IsValidRoomUrl(string input)
        {
            return Regex.IsMatch(input, "mildom.com/\\d+");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <exception cref="ParseException"></exception>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            T low;
            try
            {
                low = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                throw new ParseException(json, ex);
            }
            return low;
        }

        public static long? ExtractRoomId(string input)
        {
            var match = Regex.Match(input, "mildom.com/(\\d+)");
            if (match.Success)
            {
                return long.Parse(match.Groups[1].Value);
            }
            else
            {
                return null;
            }
        }
    }
}
