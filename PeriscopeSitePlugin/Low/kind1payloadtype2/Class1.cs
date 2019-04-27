using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin.Low.kind1payloadtype2
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
    }

    public partial class Sender
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("participant_index")]
        public long ParticipantIndex { get; set; }
    }
}
