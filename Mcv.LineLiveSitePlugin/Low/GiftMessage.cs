using LineLiveSitePlugin.ParseMessage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineLiveSitePlugin.Low.GiftMessage
{
    public class RootObject
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public class Data : IGiftMessage
    {
        public string Url { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        [JsonProperty("quantity")]
        public long Quantity { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("sender")]
        public Sender Sender { get; set; }

        [JsonProperty("isNGGift")]
        public bool IsNgGift { get; set; }

        [JsonProperty("sentAt")]
        public long SentAt { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("blockedByCms")]
        public bool BlockedByCms { get; set; }
    }

    public class Sender : IUser
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
        public object HashedIconId { get; set; }

        [JsonProperty("isGuest")]
        public bool IsGuest { get; set; }

        [JsonProperty("isBlocked")]
        public bool IsBlocked { get; set; }
    }
}
