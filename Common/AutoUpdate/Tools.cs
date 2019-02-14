using System;
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
        public static async Task<LatestVersionInfo> GetLatestVersionInfo(string name)
        {
            name = name.ToLower();
            //APIが確定するまでアダプタを置いている。ここから本当のAPIを取得する。
            var permUrl = @"https://ryu-s.github.io/" + name + "_latest";

            var wc = new WebClient();
            var api = await wc.DownloadStringTaskAsync(permUrl);

            var jsonStr = await wc.DownloadStringTaskAsync(api);
            var json = Newtonsoft.Json.JsonConvert.DeserializeObject<VersionInfo>(jsonStr);

            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = asm.GetName().Version;
            return new LatestVersionInfo(json.Version, json.Url);
        }
    }
}
