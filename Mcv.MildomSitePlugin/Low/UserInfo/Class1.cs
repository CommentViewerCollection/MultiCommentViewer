using Newtonsoft.Json;
using System;

namespace MildomSitePlugin.Low.UserInfo
{
    public class RootObject
    {
        [JsonProperty("my_id")]
        public long MyId { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("loginname")]
        public string Loginname { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("avatar")]
        public Uri Avatar { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("finance_country")]
        public string FinanceCountry { get; set; }

        [JsonProperty("user_cluster")]
        public string UserCluster { get; set; }

        [JsonProperty("power_point")]
        public long PowerPoint { get; set; }

        [JsonProperty("available_account")]
        public long AvailableAccount { get; set; }

        [JsonProperty("account")]
        public long Account { get; set; }

        [JsonProperty("account_total")]
        public long AccountTotal { get; set; }

        [JsonProperty("accessToken")]
        public Guid AccessToken { get; set; }
    }
}
