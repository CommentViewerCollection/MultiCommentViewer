using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

namespace BigoSitePlugin
{
    static class Api
    {
        public static async Task<WebSocketLink> GetWebSocketLink(IBigoServer server, CookieContainer cc)
        {
            var url = "https://www.bigo.tv/studio/getWebSocketLink";
            var res = await server.GetAsync(url);
            dynamic d = JsonConvert.DeserializeObject(res);
            if (!d.ContainsKey("code") || d.code != 0)
            {
                throw new NotImplementedException();
            }
            var uidToken = ((string)d.data.uidToken).Replace("###VER2", "");
            var deviceId = (string)d.data.deviceId;
            var userId = (string)d.data.userId;
            var ret = new WebSocketLink
            {
                DeviceId = deviceId,
                UidToken = uidToken,
                UserId = userId,
            };
            return ret;
        }
        public static async Task<InternalStudioInfo> GetInternalStudioInfo(string bigoId, IBigoServer server, CookieContainer cc)
        {
            var url = "https://www.bigo.tv/studio/getInternalStudioInfo";
            var data = new Dictionary<string, string>
            {
                {"siteId", bigoId }
            };
            var res = await server.PostAsync(url, data, cc);
            dynamic d = JsonConvert.DeserializeObject(res);
            if (!d.ContainsKey("code") || d.code != 0)
            {
                throw new NotImplementedException();
            }
            var roomid = (string)d.data.roomId;
            var info = new InternalStudioInfo
            {
                RoomId = roomid,
            };
            return info;
        }
        public static async Task<Gift[]> GetOnlineGift(DateTime now, IBigoServer server, CookieContainer cc)
        {
            var time = Tools.GetCurrentUnixTimeMillseconds(now);
            var random = "7317262808793148";
            var url = $"https://activity.bigo.tv/live/giftconfig/getOnlineGifts?jsoncallback=id_{time}_{random}";
            var res = await server.GetAsync(url);
            var match = System.Text.RegularExpressions.Regex.Match(res, "^id_\\d+_\\d+\\((.+)\\)$");
            if (!match.Success)
            {
                throw new SpecChangedException(res);
            }
            var json = match.Groups[1].Value;
            var gifts = JsonConvert.DeserializeObject<Gift[]>(json);
            return gifts;
        }
    }
    public partial class Gift
    {
        [JsonProperty("typeid")]
        public string Typeid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vm_typeid")]
        public string VmTypeid { get; set; }

        [JsonProperty("vm_exchange_rate")]
        public string VmExchangeRate { get; set; }

        [JsonProperty("groupid")]
        public string Groupid { get; set; }

        [JsonProperty("img_url")]
        public string ImgUrl { get; set; }

        [JsonProperty("area")]
        public string Area { get; set; }

        [JsonProperty("charm_value")]
        public string CharmValue { get; set; }

        [JsonProperty("sort_key")]
        public string SortKey { get; set; }
    }
}
