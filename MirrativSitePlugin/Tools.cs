using Mcv.PluginV2;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MirrativSitePlugin
{
    static class Tools
    {
        public static Message ParseType1Data(dynamic json)
        {
            string comment;
            if (json.IsDefined("cm"))
            {
                comment = json["cm"];
            }
            else
            {
                comment = "";
            }
            var message = new Message
            {
                Comment = comment,
                CreatedAt = (long)json["created_at"],
                Id = (json["lci"]).ToString(),
                Type = MessageType.Comment,
                UserId = json["u"],
                Username = json["ac"],
            };
            return message;
        }
        public static DateTime UnixTime2DateTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        }
        public static string ExtractLiveId(string str)
        {
            //https://www.mirrativ.com/live/sUbSZSYyTAOYJtLd0WamoQ
            var match = Regex.Match(str, "mirrativ\\.com/(?:live|broadcast)/([a-zA-Z0-9-_]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }
        public static string ExtractUserId(string str)
        {
            //https://www.mirrativ.com/user/1091674
            var match = Regex.Match(str, "mirrativ\\.com/user/(\\d+)");
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
        /// <param name="keyValueData">{"key1":"value1","key2":0,"key3":"value3"}のような文字列</param>
        /// <returns></returns>
        public static Dictionary<string, string> KeyValue2Dict(string keyValueData)
        {
            var temp = keyValueData;
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(keyValueData))
            {
                return dict;
            }
            if (keyValueData.Length < 2)
            {
                return dict;
            }
            temp = temp.Substring(1, temp.Length - 2);//囲っている{}を外す
            var kvArr = temp.Split(new[] { "," }, StringSplitOptions.None);
            foreach (var kv in kvArr)
            {
                var match = Regex.Match(kv, "\"(?<key>[^\"]+)\":\"?(?<value>[^\"]+)\"?");
                if (match.Success)
                {
                    var k = match.Groups["key"].Value;
                    var v = match.Groups["value"].Value;
                    dict.Add(k, v);
                }
            }
            return dict;
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
