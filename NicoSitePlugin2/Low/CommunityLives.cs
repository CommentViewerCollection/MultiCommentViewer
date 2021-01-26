using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePlugin2.Low.CommunityLives
{
    public class RootObject
    {
        [JsonProperty("meta")]
        public Meta Meta { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data
    {
        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("lives")]
        public Live[] Lives { get; set; }
    }

    public class Live
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("watch_url")]
        public Uri WatchUrl { get; set; }

        [JsonProperty("features")]
        public Features Features { get; set; }

        [JsonProperty("timeshift")]
        public Timeshift Timeshift { get; set; }

        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("finished_at", NullValueHandling = NullValueHandling.Ignore)]
        public string FinishedAt { get; set; }
    }

    public class Features
    {
        [JsonProperty("is_member_only")]
        public bool IsMemberOnly { get; set; }
    }

    public class Timeshift
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("can_view", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CanView { get; set; }

        [JsonProperty("finished_at", NullValueHandling = NullValueHandling.Ignore)]
        public string FinishedAt { get; set; }
    }

    public class Meta
    {
        [JsonProperty("status")]
        public long Status { get; set; }
    }
}
