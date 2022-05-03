using System;

namespace Mcv.PluginV2;

public static class UnixTimeConverter
{
    static readonly DateTime _baseTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long ToUnixTime(DateTime dateTime)
    {
        // 時刻をUTCに変換
        dateTime = dateTime.ToUniversalTime();

        // unix epochからの経過秒数を求める
        return (long)dateTime.Subtract(_baseTime).TotalSeconds;
    }
    public static DateTime FromUnixTime(long unixTime)
    {
        return _baseTime.AddSeconds(unixTime);
    }
}
