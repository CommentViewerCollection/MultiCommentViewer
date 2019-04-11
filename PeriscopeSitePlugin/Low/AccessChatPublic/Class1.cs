using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin.Low.AccessChatPublic
{
    public partial class RootObject
    {
        [JsonProperty("subscriber")]
        public string Subscriber { get; set; }

        [JsonProperty("publisher")]
        public string Publisher { get; set; }

        [JsonProperty("auth_token")]
        public string AuthToken { get; set; }

        [JsonProperty("signer_key")]
        public string SignerKey { get; set; }

        [JsonProperty("signer_token")]
        public string SignerToken { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("should_verify_signature")]
        public bool ShouldVerifySignature { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }

        [JsonProperty("replay_access_token")]
        public string ReplayAccessToken { get; set; }

        [JsonProperty("replay_endpoint")]
        public string ReplayEndpoint { get; set; }

        [JsonProperty("room_id")]
        public string RoomId { get; set; }

        [JsonProperty("participant_index")]
        public long ParticipantIndex { get; set; }

        [JsonProperty("read_only")]
        public bool ReadOnly { get; set; }

        [JsonProperty("should_log")]
        public bool ShouldLog { get; set; }

        [JsonProperty("chan_perms")]
        public ChanPerms ChanPerms { get; set; }
    }

    public partial class ChanPerms
    {
        [JsonProperty("pb")]
        public long Pb { get; set; }

        [JsonProperty("cm")]
        public long Cm { get; set; }
    }
}
