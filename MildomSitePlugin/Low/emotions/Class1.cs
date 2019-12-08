using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MildomSitePlugin.Low.emotions
{
    public partial class RootObject
    {
        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("body")]
        public Body Body { get; set; }

        [JsonProperty("setting_version")]
        public long SettingVersion { get; set; }

        [JsonProperty("ts_ms")]
        public long TsMs { get; set; }
    }

    public partial class Body
    {
        [JsonProperty("official_emotions")]
        public OfficialEmotion[] OfficialEmotions { get; set; }

        [JsonProperty("fans_group_emotions")]
        public FansGroupEmotion[] FansGroupEmotions { get; set; }

        [JsonProperty("default_version")]
        public long DefaultVersion { get; set; }
    }

    public partial class FansGroupEmotion
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("pic")]
        public string Pic { get; set; }

        [JsonProperty("pic_type")]
        public string PicType { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("lock")]
        public long Lock { get; set; }

        [JsonProperty("illegal")]
        public long Illegal { get; set; }
    }

    public partial class OfficialEmotion
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("pic")]
        public string Pic { get; set; }

        [JsonProperty("pic_type")]
        public string PicType { get; set; }
    }
}
