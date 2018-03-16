using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePlugin.Next.Low.ProgramInfo
{
    public class ProgramInfo
    {
        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("isMemberOnly")]
        public bool IsMemberOnly { get; set; }

        [JsonProperty("vposBaseAt")]
        public long VposBaseAt { get; set; }

        [JsonProperty("beginAt")]
        public long BeginAt { get; set; }

        [JsonProperty("endAt")]
        public long EndAt { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("categories")]
        public List<string> Categories { get; set; }

        [JsonProperty("rooms")]
        public List<Room> Rooms { get; set; }

        [JsonProperty("socialGroup")]
        public SocialGroup SocialGroup { get; set; }
    }

    public class Room
    {
        [JsonProperty("webSocketUri")]
        public string WebSocketUri { get; set; }

        [JsonProperty("xmlSocketUri")]
        public string XmlSocketUri { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("threadId")]
        public string ThreadId { get; set; }
    }

    public class SocialGroup
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Meta
    {
        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }
    }
}
