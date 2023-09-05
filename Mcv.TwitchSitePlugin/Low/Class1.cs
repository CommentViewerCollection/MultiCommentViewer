using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchSitePlugin.Low.Streams
{
    public class RootObject
    {
        [JsonProperty("data")]
        public Datum[] Data { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }

    public class Datum
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("game_id")]
        public string GameId { get; set; }

        [JsonProperty("community_ids")]
        public Guid[] CommunityIds { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("viewer_count")]
        public long ViewerCount { get; set; }

        [JsonProperty("started_at")]
        public DateTimeOffset StartedAt { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("tag_ids")]
        public Guid[] TagIds { get; set; }
    }

    public class Pagination
    {
        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }
}
