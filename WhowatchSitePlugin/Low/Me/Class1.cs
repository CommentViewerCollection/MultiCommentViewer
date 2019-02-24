using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhowatchSitePlugin.Low.Me
{
    public class RootObject : IMe
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("user_type")]
        public string UserType { get; set; }

        [JsonProperty("user_code")]
        public string UserCode { get; set; }

        [JsonProperty("account_register_status")]
        public string AccountRegisterStatus { get; set; }

        [JsonProperty("whowatch_point")]
        public long WhowatchPoint { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("account_name")]
        public string AccountName { get; set; }

        [JsonProperty("user_path")]
        public string UserPath { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("is_email_registered")]
        public bool IsEmailRegistered { get; set; }

        [JsonProperty("is_twitter_connected")]
        public bool IsTwitterConnected { get; set; }

        [JsonProperty("is_facebook_connected")]
        public bool IsFacebookConnected { get; set; }

        [JsonProperty("is_related_account_auto_blocked")]
        public bool IsRelatedAccountAutoBlocked { get; set; }
    }
}
