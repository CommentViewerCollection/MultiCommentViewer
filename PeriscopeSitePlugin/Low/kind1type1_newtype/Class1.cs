using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin.Low.kind1type1_newtype
{
    public class RootObject
    {
        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("eggmojiOverride")]
        public bool EggmojiOverride { get; set; }

        [JsonProperty("ntpForBroadcasterFrame")]
        public double NtpForBroadcasterFrame { get; set; }

        [JsonProperty("ntpForLiveFrame")]
        public double NtpForLiveFrame { get; set; }

        [JsonProperty("participant_index")]
        public long ParticipantIndex { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("sender")]
        public Sender Sender { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }

        [JsonProperty("uuid")]
        public Guid Uuid { get; set; }

        [JsonProperty("v")]
        public long V { get; set; }
    }

    public class Sender
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("participant_index")]
        public long ParticipantIndex { get; set; }

        [JsonProperty("profile_image_url")]
        public Uri ProfileImageUrl { get; set; }

        [JsonProperty("twitter_id")]
        public string TwitterId { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
