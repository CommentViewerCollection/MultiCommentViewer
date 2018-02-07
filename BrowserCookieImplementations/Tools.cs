using System;

namespace ryu_s.BrowserCookie
{
    internal static class Tools
    {

        /// <summary>
        /// unix epochをDateTimeで表した定数
        /// </summary>
        private readonly static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        /// <summary>
        /// UNIX時間からDateTimeに変換するメソッド
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        public static DateTime FromUnixTime(long unixTime)
        {
            // unix epochからunixTime秒だけ経過した時刻を求める
            return UnixEpoch.AddSeconds(unixTime).ToLocalTime();
        }
    }
}
