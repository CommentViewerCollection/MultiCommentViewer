using Codeplex.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MixchSitePlugin
{
    class Me
    {
        public string DisplayName { get; set; }
        public string UserId { get; set; }
    }
    static class API
    {
        public static async Task<Me> GetMeAsync(IDataSource server, CookieContainer cc)
        {
            var me = new Me();
            var url = "https://mixch.tv/mypage";
            var res = await server.GetAsync(url, cc);
            var match0 = Regex.Match(res, "<p class=\"name\">\\s*([^<\\s]*)?\\s*</p>\\s*<p class=\"id\">");
            if (match0.Success)
            {
                var displayName = match0.Groups[1].Value;
                me.DisplayName = displayName;
            }
            var match1 = Regex.Match(res, "<p class=\"id\">\\s*ID\\s*:\\s*([0-9]+)");
            if (match1.Success)
            {
                me.UserId = match1.Groups[1].Value;
            }
            return me;
        }
    }
}
