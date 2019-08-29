using SitePlugin;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MixerSitePlugin
{
    static class Tools
    {
        public static DateTime UnixTime2DateTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        }
        public static string ExtractLiveId(string str)
        {
            //https://www.Mixer.com/live/sUbSZSYyTAOYJtLd0WamoQ
            var match = Regex.Match(str, "Mixer\\.com/(?:live|broadcast)/([a-zA-Z0-9-_]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }
        public static string ExtractUserId(string str)
        {
            //https://mixer.com/Monstercat
            var match = Regex.Match(str, "mixer\\.com/([^/?:=#]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }
        public static bool IsValidLiveId(string input)
        {
            var liveId = Tools.ExtractLiveId(input);
            return !string.IsNullOrEmpty(liveId);
        }
        public static bool IsValidUserId(string input)
        {
            var userId = Tools.ExtractUserId(input);
            return !string.IsNullOrEmpty(userId);
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
    }
}
