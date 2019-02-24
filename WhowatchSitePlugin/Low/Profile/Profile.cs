using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhowatchSitePlugin.Low.Profile
{
    public class RootObject
    {
        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("background_url")]
        public string BackgroundUrl { get; set; }

        [JsonProperty("show_follow_list")]
        public long ShowFollowList { get; set; }

        [JsonProperty("show_follower_list")]
        public long ShowFollowerList { get; set; }

        [JsonProperty("live")]
        public Live Live { get; set; }

        [JsonProperty("account_name")]
        public string AccountName { get; set; }

        [JsonProperty("user_path")]
        public string UserPath { get; set; }

        [JsonProperty("follow_count")]
        public long FollowCount { get; set; }

        [JsonProperty("follower_count")]
        public long FollowerCount { get; set; }

        [JsonProperty("twitter_id")]
        public string TwitterId { get; set; }

        [JsonProperty("is_follow")]
        public bool IsFollow { get; set; }

        [JsonProperty("is_follow_backed")]
        public bool IsFollowBacked { get; set; }

        [JsonProperty("is_blocked")]
        public bool IsBlocked { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("date_of_birth")]
        public string DateOfBirth { get; set; }

        [JsonProperty("area")]
        public string Area { get; set; }

        [JsonProperty("likes")]
        public List<string> Likes { get; set; }

        [JsonProperty("live_history_count")]
        public long LiveHistoryCount { get; set; }

        [JsonProperty("is_push_registered")]
        public bool IsPushRegistered { get; set; }
    }

    public class Live
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("client_type")]
        public string ClientType { get; set; }

        [JsonProperty("started_at")]
        public long StartedAt { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("running_time")]
        public long RunningTime { get; set; }

        [JsonProperty("is_comment_disallowed")]
        public bool IsCommentDisallowed { get; set; }
    }
}
