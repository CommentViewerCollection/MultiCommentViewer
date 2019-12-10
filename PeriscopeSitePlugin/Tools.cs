using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Codeplex.Data;

namespace PeriscopeSitePlugin
{
    internal static class Tools
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public static string ExtractHostnameFromEndpoint(string endpoint)
        {
            //https://proxsee.pscp.tv/api/v2/accessChatやhttps://proxsee.pscp.tv/api/v2/accessChatPublic　のレスポンス中に含まれるendpointからホスト名を抜き出す
            //"https://prod-chatman-ancillary-us-east-1.pscp.tv"
            var uri = new Uri(endpoint);
            return uri.Host;
        }
        public static (string channelName, string liveId) ExtractChannelNameAndLiveId(string input)
        {
            //2019/09/01
            //PeriscopeのURLは2種類ある。
            //Type1
            //https://www.periscope.tv/w/(LIVE_ID)
            //Type2
            //https://www.periscope.tv/(CHANNEL_NAME)/(LIVE_ID)
            //
            //Type1にはチャンネル名が付加される場合もある
            //https://www.periscope.tv/w/(LIVE_ID)?channel=(CHANNEL_NAME)
            //
            //また、ドメイン名はperiscope.tvとpscp.tvがあり、常に可換。

            if (string.IsNullOrWhiteSpace(input)) return (null, null);

            var list = new List<Func<string, (string, string)>>
            {
                ExtractChannelNameAndLiveIdFromUrlType1,
                ExtractChannelNameAndLiveIdFromUrlType2,
                ExtractChannelNameFromUrlType2,
            };
            foreach (var func in list)
            {
                var (channel, liveid) = func(input);
                if (!string.IsNullOrEmpty(channel) || !string.IsNullOrEmpty(liveid))
                {
                    return (channel, liveid);
                }
            }
            return (null, null);

        }
        private static (string channelName, string liveId) ExtractChannelNameAndLiveIdFromUrlType1(string input)
        {
            //https://www.periscope.tv/w/1jMJgvLrMEjGL
            //https://www.periscope.tv/w/1jMJgvLrMEjGL?channel=abc
            var match = Regex.Match(input, "(?:periscope|pscp)\\.tv/w/(?<liveid>[0-9a-zA-Z-_]+)(\\?channel=(?<channel>[^/]+))?");
            if (match.Success)
            {
                var liveid = match.Groups["liveid"].Value;
                var channel = match.Groups["channel"].Value;
                if (string.IsNullOrEmpty(channel))
                {
                    channel = null;
                }
                return (channel, liveid);
            }
            else
            {
                return (null, null);
            }
        }
        private static (string channelName, string liveId) ExtractChannelNameAndLiveIdFromUrlType2(string input)
        {
            //https://www.periscope.tv/Lovelylndeed/1jMJgvLrMEjGL
            var match = Regex.Match(input, "(?:periscope|pscp)\\.tv/(?<channel>[0-9a-zA-Z-_]+)/(?<liveid>[0-9a-zA-Z-_]+)");
            if (match.Success)
            {
                var liveid = match.Groups["liveid"].Value;
                var channel = match.Groups["channel"].Value;
                return (channel, liveid);
            }
            else
            {
                return (null, null);
            }
        }
        private static (string channelName, string liveId) ExtractChannelNameFromUrlType2(string input)
        {
            //https://www.periscope.tv/Lovelylndeed
            var match = Regex.Match(input, "(?:periscope|pscp)\\.tv/(?<channel>[0-9a-zA-Z-_]+)");
            if (match.Success)
            {
                var channel = match.Groups["channel"].Value;
                return (channel, null);
            }
            else
            {
                return (null, null);
            }
        }

        internal static IUrl GetUrl(string input)
        {
            var (channelid, liveid) = ExtractChannelNameAndLiveId(input);
            if (!string.IsNullOrEmpty(liveid))
            {
                return new LivePageUrl(input);
            }
            else if (!string.IsNullOrEmpty(channelid))
            {
                return new ChannelUrl(input);
            }
            else
            {
                return new UnknownUrl(input);
            }
        }
        public static string ExtractChannelPageJson(string channelPageHtml)
        {
            var match = Regex.Match(channelPageHtml, " data-store=\"({.+})\">");
            if (match.Success)
            {
                var raw = match.Groups[1].Value;
                return raw.Replace("&quot;", "\"");
            }
            else
            {
                return null;
            }
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
