using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class UnixTimeConverter
    {
        static DateTime baseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long ToUnixTime(DateTime dateTime)
        {
            // 時刻をUTCに変換
            dateTime = dateTime.ToUniversalTime();

            // unix epochからの経過秒数を求める
            return (long)dateTime.Subtract(baseTime).TotalSeconds;
        }
        public static DateTime FromUnixTime(long unixTime)
        {
            return baseTime.AddSeconds(unixTime);
        }
    }
}
