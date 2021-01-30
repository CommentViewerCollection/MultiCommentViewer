using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePlugin2.Low.UserInfo
{
    public partial class RootObject
    {
        [JsonProperty("data")]
        public Datum[] Data { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }

    public partial class Datum
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("hasPremiumOrStrongerRights")]
        public bool HasPremiumOrStrongerRights { get; set; }

        [JsonProperty("hasSuperPremiumOrStrongerRights")]
        public bool HasSuperPremiumOrStrongerRights { get; set; }

        [JsonProperty("icons")]
        public Icons Icons { get; set; }
    }

    public partial class Icons
    {
        [JsonProperty("urls")]
        public Urls Urls { get; set; }
    }

    public partial class Urls
    {
        [JsonProperty("150x150")]
        public string The150X150 { get; set; }

        [JsonProperty("50x50")]
        public string The50X50 { get; set; }
    }

    public partial class Meta
    {
        [JsonProperty("status")]
        public long Status { get; set; }
    }
}
