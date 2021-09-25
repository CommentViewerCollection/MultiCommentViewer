using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Newtonsoft.Json;
using SitePlugin;

namespace MixchSitePlugin
{
    class LiveUrlInfo
    {
        public string LiveId { get; set; }
        public string Environment { get; set; }
    }
    static class Tools
    {
        private const string regexLiveUrl = "([a-z]*)\\.?mixch\\.tv/u/(?<id>[0-9]+)/live";
        public static async Task<LiveUrlInfo> GetLiveId(IDataSource dataSource, string input)
        {
            // LIVE_ID
            // https://mixch.tv/u/LIVE_ID/live

            var liveUrlInfo = new LiveUrlInfo();
            var match = Regex.Match(input, regexLiveUrl);
            if (match.Success)
            {
                liveUrlInfo.Environment = match.Groups[1].Value;
                if (liveUrlInfo.Environment == "")
                {
                    liveUrlInfo.Environment = "torte";
                }
                liveUrlInfo.LiveId = match.Groups[2].Value;
            }
            else
            {
                throw new InvalidInputException();
            }
            return liveUrlInfo;
        }
        public static bool IsValidUrl(string input)
        {
            return Regex.IsMatch(input, regexLiveUrl);
        }
        public static string ElapsedToString(int elapsed)
        {
            var t = TimeSpan.FromSeconds(elapsed);
            return t.Hours == 0 ? t.ToString("mm\\:ss") : t.ToString("h\\:mm\\:ss");
        }
    }
}
