using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MildomSitePlugin.Low.gift_find
{
    public class RootObject
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("body")]
        public Body Body { get; set; }
        [JsonProperty("setting_version")]
        public int SettingVersion { get; set; }
        [JsonProperty("ts_ms")]
        public long TsMs { get; set; }
    }
    public class Body
    {
        [JsonProperty("totalRows")]
        public int TotalRows { get; set; }
        [JsonProperty("gift_version")]
        public int GiftVersion { get; set; }
        [JsonProperty("models")]
        public GiftItem[] Models { get; set; }
        [JsonProperty("pack")]
        public GiftItem[] Pack { get; set; }

    }
    public class GiftItem
    {
        [JsonProperty("gift_id")]
        public long GiftId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("price")]
        public int Price { get; set; }
        [JsonProperty("pic")]
        public string Pic { get; set; }


    }
}
