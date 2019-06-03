using Newtonsoft.Json;

namespace NicoSitePlugin.Low.Thread
{
    public class RootObject
    {
        [JsonProperty("thread")]
        public Thread Thread { get; set; }
    }

    public class Thread
    {
        [JsonProperty("resultcode")]
        public int? Resultcode { get; set; }

        [JsonProperty("thread")]
        public string ThreadThread { get; set; }

        [JsonProperty("last_res")]
        public int? LastRes { get; set; }

        [JsonProperty("ticket")]
        public string Ticket { get; set; }

        [JsonProperty("revision")]
        public int? Revision { get; set; }

        [JsonProperty("server_time")]
        public long ServerTime { get; set; }
    }
}
