using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LineLiveSitePlugin.ParseMessage;

namespace LineLiveSitePlugin.Low.Love
{
    public partial class RootObject
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public partial class Data:ILove
    {
        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("sender")]
        public Sender Sender { get; set; }

        [JsonProperty("roomId")]
        public string RoomId { get; set; }

        [JsonProperty("sentAt")]
        public long SentAt { get; set; }
    }

    public partial class Sender:IUser
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("hashedId")]
        public string HashedId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }

        [JsonProperty("hashedIconId")]
        public string HashedIconId { get; set; }

        [JsonProperty("isGuest")]
        public bool IsGuest { get; set; }

        [JsonProperty("isBlocked")]
        public bool IsBlocked { get; set; }
    }
}
