using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLiveSitePlugin.Low.BlackList
{
    public class RootObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("blive_user_id")]
        public long BLiveUserId { get; set; }

        [JsonProperty("recxuser_id")]
        public long RecxuserId { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("introduction")]
        public string Introduction { get; set; }

        [JsonProperty("icon_image_url")]
        public Uri IconImageUrl { get; set; }

        [JsonProperty("l_icon_image_url")]
        public Uri LIconImageUrl { get; set; }

        [JsonProperty("cover_image_url")]
        public string CoverImageUrl { get; set; }

        [JsonProperty("follows")]
        public long Follows { get; set; }

        [JsonProperty("followers")]
        public long Followers { get; set; }

        [JsonProperty("is_premium")]
        public bool IsPremium { get; set; }

        [JsonProperty("premium_start_at")]
        public object PremiumStartAt { get; set; }

        [JsonProperty("premium_charge_type")]
        public object PremiumChargeType { get; set; }

        [JsonProperty("is_official")]
        public bool IsOfficial { get; set; }

        [JsonProperty("is_fresh")]
        public bool IsFresh { get; set; }

        [JsonProperty("is_warned")]
        public bool IsWarned { get; set; }

        [JsonProperty("is_team")]
        public bool IsTeam { get; set; }

        [JsonProperty("is_league_yell")]
        public bool IsLeagueYell { get; set; }

        [JsonProperty("is_live")]
        public bool IsLive { get; set; }

        [JsonProperty("live_views")]
        public long LiveViews { get; set; }
    }
}
