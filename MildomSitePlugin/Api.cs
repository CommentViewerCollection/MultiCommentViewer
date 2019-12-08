using Codeplex.Data;
using Newtonsoft.Json;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MildomSitePlugin
{
    static class Api
    {
        public static async Task<Dictionary<int, string>> GetImageDictionary(IDataServer server, long room_id, CookieContainer cc)
        {
            var url = $"https://cloudac.mildom.com/nonolive/gappserv/emotion/getListV1?room_id={room_id}&__platform=web";
            var res = await server.GetAsync(url, new Dictionary<string, string>());
            var d = DynamicJson.Parse(res);
            var dict = new Dictionary<int, string>();
            if ((int)d == 0)
            {
                const string urlPrefix = "https://res.mildom.com/download/file/";
                var obj = Tools.Deserialize<Low.emotions.RootObject>(res);
                foreach (var emot in obj.Body.OfficialEmotions)
                {
                    dict.Add((int)emot.Id, urlPrefix + emot.Pic);
                }
                if (obj.Body.FansGroupEmotions != null)
                {
                    foreach (var emot in obj.Body.FansGroupEmotions)
                    {
                        dict.Add((int)emot.Id, urlPrefix + emot.Pic);
                    }
                }
            }
            return dict;
        }
    }
}
