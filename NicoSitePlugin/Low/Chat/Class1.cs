using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePlugin.Low.Chat
{
    public class RootObject
    {
        [JsonProperty("chat")]
        public Chat Chat { get; set; }
    }

    public class Chat
    {
        [JsonProperty("thread")]
        public string Thread { get; set; }

        [JsonProperty("no")]
        public int? No { get; set; }

        [JsonProperty("vpos")]
        public long Vpos { get; set; }

        [JsonProperty("date")]
        public long Date { get; set; }

        [JsonProperty("date_usec")]
        public long DateUsec { get; set; }

        [JsonProperty("mail", NullValueHandling = NullValueHandling.Ignore)]
        public string Mail { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("premium", NullValueHandling = NullValueHandling.Ignore)]
        public int? Premium { get; set; }

        [JsonProperty("anonymity", NullValueHandling = NullValueHandling.Ignore)]
        public int? Anonymity { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("locale", NullValueHandling = NullValueHandling.Ignore)]
        public string Locale { get; set; }

        [JsonProperty("score", NullValueHandling = NullValueHandling.Ignore)]
        public int? Score { get; set; }
    }
}
