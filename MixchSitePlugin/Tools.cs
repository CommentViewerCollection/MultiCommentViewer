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
    static class Tools
    {
        public static async Task<string> GetLiveId(IDataSource dataSource, string input)
        {
            //LIVE_ID
            //https://mixch.tv/u/LIVE_ID/live

            string id;
            var match = Regex.Match(input, $"{MixchSiteContext.MixchDomainRegex()}/u/(?<id>[0-9]+)/live");
            if (match.Success)
            {
                id = match.Groups[1].Value;
            }
            else
            {
                throw new InvalidInputException();
            }

            // TODO: 配信中かどうかチェックが必要かも
            return id;
        }
        public static bool IsValidUrl(string input)
        {
            var b = Regex.IsMatch(input, $"{MixchSiteContext.MixchDomainRegex()}/u/([0-9]+)/live");
            return b;
        }
        public static string ElapsedToString(int elapsed)
        {
            var t = TimeSpan.FromSeconds(elapsed);
            return t.Hours == 0 ? t.ToString("mm\\:ss") : t.ToString("h\\:mm\\:ss");
        }
    }
}
