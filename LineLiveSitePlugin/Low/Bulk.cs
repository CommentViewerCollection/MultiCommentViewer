using LineLiveSitePlugin.ParseMessage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineLiveSitePlugin.Low.Bulk
{
    public partial class RootObject
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public RootObjectData Data { get; set; }
    }

    public partial class RootObjectData
    {
        [JsonProperty("payloads")]
        public List<object> Payloads { get; set; }
    }
}
