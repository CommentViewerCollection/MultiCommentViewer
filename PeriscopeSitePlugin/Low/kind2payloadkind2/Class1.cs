using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin.Low.kind2payloadkind2
{
    public class RootObject
    {
        [JsonProperty("kind")]
        public long Kind { get; set; }

        [JsonProperty("sender")]
        public Sender Sender { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }

    public class Sender
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

        [JsonProperty("new_user")]
        public bool NewUser { get; set; }

        [JsonProperty("lang")]
        public string[] Lang { get; set; }
    }
}
