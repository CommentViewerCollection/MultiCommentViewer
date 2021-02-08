using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace Common.AutoUpdate
{
    public static class Tools
    {
        private class VersionInfo
        {
            public string Name { get; set; }
            public string Version { get; set; }
            public string Url { get; set; }
        }
        public static async Task<LatestVersionInfo> GetLatestVersionInfo(string name, string userAgent)
        {
            name = name.ToLower();
            //APIが確定するまでアダプタを置いている。ここから本当のAPIを取得する。
            var permUrl = @"https://ryu-s.github.io/" + name + "_latest";

            VersionInfo json;
            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", userAgent);
                var api = await client.GetStringAsync(permUrl);
                var jsonStr = await client.GetStringAsync(api);
                json = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionInfo>(jsonStr);
            }
            return new LatestVersionInfo(json.Version, json.Url);
        }
    }
}
