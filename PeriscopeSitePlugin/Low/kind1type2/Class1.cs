using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin.Low.kind1type2
{
    public partial class RootObject
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("ntpForBroadcasterFrame")]
        public double NtpForBroadcasterFrame { get; set; }

        [JsonProperty("ntpForLiveFrame")]
        public double NtpForLiveFrame { get; set; }

        [JsonProperty("participant_index")]
        public long ParticipantIndex { get; set; }

        [JsonProperty("remoteID")]
        public string RemoteId { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("type")]
        public long Type { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("v")]
        public long V { get; set; }
    }
}
