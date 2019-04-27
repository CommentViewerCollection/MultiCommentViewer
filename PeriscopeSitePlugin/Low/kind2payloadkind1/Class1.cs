using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin.Low.kind2payloadkind1
{
    public partial class RootObject
    {
        [JsonProperty("kind")]
        public long Kind { get; set; }

        [JsonProperty("sender")]
        public Sender Sender { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
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
    }
}
