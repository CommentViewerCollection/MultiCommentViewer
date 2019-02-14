using LineLiveSitePlugin.ParseMessage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineLiveSitePlugin.Low.SystemMessage
{
    public partial class RootObject
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("extraData")]
        public ExtraData ExtraData { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public partial class ExtraData
    {
        [JsonProperty("user")]
        public User User { get; set; }
    }

    public partial class User:IUser
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("hashedId")]
        public string HashedId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("iconObsHash")]
        public string IconObsHash { get; set; }

        [JsonProperty("owner")]
        public bool Owner { get; set; }

        [JsonProperty("isBlocked")]
        public bool IsBlocked { get; set; }
    }

}
