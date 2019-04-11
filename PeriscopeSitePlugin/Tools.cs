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
        public static string ExtractLiveId(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            //https://www.pscp.tv/w/1ypJdvRwXQVKW?channel=fave-musician
            //https://www.pscp.tv/w/1ypKdvbyAmpJW?channel=featured-broadcasts
            var match = Regex.Match(input, "pscp\\.tv/w/([0-9a-zA-Z-_]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
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
