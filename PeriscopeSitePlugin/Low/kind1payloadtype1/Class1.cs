using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin.Low.kind1payloadtype1
{
    public partial class RootObject
    {
        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("lang")]
        public string Lang { get; set; }

        [JsonProperty("sender")]
        public Sender Sender { get; set; }

        [JsonProperty("timestamp")]
        public double Timestamp { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }

    public partial class Sender
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [JsonProperty("participant_index")]
        public long ParticipantIndex { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("twitter_id")]
        public string TwitterId { get; set; }

        [JsonProperty("lang")]
        public string[] Lang { get; set; }

        [JsonProperty("superfan")]
        public bool Superfan { get; set; }
    }
}
