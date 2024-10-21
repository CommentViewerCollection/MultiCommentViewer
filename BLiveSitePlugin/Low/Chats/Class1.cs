using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLiveSitePlugin.Low.Chats
{
    public partial class RootObject
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("quality_type")]
        public long QualityType { get; set; }

        [JsonProperty("posted_at")]
        public DateTimeOffset PostedAt { get; set; }

        [JsonProperty("stamp")]
        public Stamp Stamp { get; set; }

        [JsonProperty("yell_type")]
        public object YellType { get; set; }

        [JsonProperty("yell")]
        public Yell Yell { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("to_user")]
        public object ToUser { get; set; }

        [JsonProperty("chat_setting")]
        public ChatSetting ChatSetting { get; set; }

        [JsonProperty("is_moderating")]
        public bool IsModerating { get; set; }

        [JsonProperty("has_banned_word")]
        public bool HasBannedWord { get; set; }
    }
    public class Stamp
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("group_id")]
        public long GroupId { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
    }
    public class Yell
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("points")]
        public long Points { get; set; }

        [JsonProperty("yells")]
        public long Yells { get; set; }

        [JsonProperty("ticker_seconds")]
        public long TickerSeconds { get; set; }
    }
    public partial class ChatSetting
    {
        [JsonProperty("name_color")]
        public string NameColor { get; set; }

        [JsonProperty("is_premium_hidden")]
        public bool IsPremiumHidden { get; set; }
    }

    public partial class User
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
        public string IconImageUrl { get; set; }

        [JsonProperty("cover_image_url")]
        public string CoverImageUrl { get; set; }

        [JsonProperty("follows")]
        public long Follows { get; set; }

        [JsonProperty("followers")]
        public long Followers { get; set; }

        [JsonProperty("is_premium")]
        public bool IsPremium { get; set; }

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
    }
}
