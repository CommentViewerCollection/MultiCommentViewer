using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhowatchSitePlugin.Low.LiveData
{
    public partial class RootObject
    {
        [JsonProperty("live")]
        public Live Live { get; set; }

        [JsonProperty("comments")]
        public List<Comment> Comments { get; set; }

        [JsonProperty("deleted_comment_ids")]
        public List<long> DeletedCommentIds { get; set; }

        [JsonProperty("updated_at")]
        public long UpdatedAt { get; set; }

        [JsonProperty("live_sent_items")]
        public List<LiveSentItem> LiveSentItems { get; set; }

        [JsonProperty("is_item_updated")]
        public bool IsItemUpdated { get; set; }

        [JsonProperty("jwt")]
        public string Jwt { get; set; }
    }

    public partial class Comment
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("user")]
        public CommentUser User { get; set; }

        [JsonProperty("comment_type")]
        public string CommentType { get; set; }

        [JsonProperty("item_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? ItemCount { get; set; }

        [JsonProperty("anonymized")]
        public bool Anonymized { get; set; }

        [JsonProperty("pickup_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? PickupTime { get; set; }

        [JsonProperty("live_id")]
        public long LiveId { get; set; }

        [JsonProperty("not_escaped")]
        public bool NotEscaped { get; set; }

        [JsonProperty("posted_at")]
        public long PostedAt { get; set; }

        [JsonProperty("tts", NullValueHandling = NullValueHandling.Ignore)]
        public Tts Tts { get; set; }

        [JsonProperty("play_item_pattern_id", NullValueHandling = NullValueHandling.Ignore)]
        public long? PlayItemPatternId { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("escaped_message")]
        public string EscapedMessage { get; set; }

        [JsonProperty("is_silent_comment")]
        public bool IsSilentComment { get; set; }

        [JsonProperty("pattern_decoration", NullValueHandling = NullValueHandling.Ignore)]
        public string PatternDecoration { get; set; }

        [JsonProperty("ng_word_included")]
        public bool NgWordIncluded { get; set; }
    }

    public partial class Tts
    {
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class CommentUser
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("user_type")]
        public string UserType { get; set; }

        [JsonProperty("user_profile")]
        public UserProfile UserProfile { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("account_name")]
        public string AccountName { get; set; }

        [JsonProperty("user_path")]
        public string UserPath { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class UserProfile
    {
        [JsonProperty("is_date_of_birth_today")]
        public bool IsDateOfBirthToday { get; set; }
    }

    public partial class Live
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("live_type")]
        public string LiveType { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("telop")]
        public string Telop { get; set; }

        [JsonProperty("live_status")]
        public string LiveStatus { get; set; }

        [JsonProperty("user")]
        public LiveUser User { get; set; }

        [JsonProperty("client_type")]
        public string ClientType { get; set; }

        [JsonProperty("category")]
        public Category Category { get; set; }

        [JsonProperty("started_at")]
        public long StartedAt { get; set; }

        [JsonProperty("time_limit")]
        public long TimeLimit { get; set; }

        [JsonProperty("live_act_limit")]
        public long LiveActLimit { get; set; }

        [JsonProperty("extension_option_limit")]
        public long ExtensionOptionLimit { get; set; }

        [JsonProperty("total_view_count")]
        public long TotalViewCount { get; set; }

        [JsonProperty("comment_count")]
        public long CommentCount { get; set; }

        [JsonProperty("item_count")]
        public long ItemCount { get; set; }

        [JsonProperty("live_finished_image_url")]
        public string LiveFinishedImageUrl { get; set; }

        [JsonProperty("share_url")]
        public string ShareUrl { get; set; }

        [JsonProperty("latest_thumbnail_url")]
        public string LatestThumbnailUrl { get; set; }

        [JsonProperty("running_time")]
        public long RunningTime { get; set; }

        [JsonProperty("is_mute")]
        public bool IsMute { get; set; }

        [JsonProperty("is_automatic_extension")]
        public bool IsAutomaticExtension { get; set; }

        [JsonProperty("view_count")]
        public long ViewCount { get; set; }

        [JsonProperty("is_comment_disallowed")]
        public bool IsCommentDisallowed { get; set; }
    }

    public partial class Category
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("badge")]
        public string Badge { get; set; }

        [JsonProperty("is_movie_only")]
        public bool IsMovieOnly { get; set; }

        [JsonProperty("is_radio_only")]
        public bool IsRadioOnly { get; set; }

        [JsonProperty("is_collaboration_ban")]
        public bool IsCollaborationBan { get; set; }
    }

    public partial class LiveUser
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("user_type")]
        public string UserType { get; set; }

        [JsonProperty("user_profile")]
        public UserProfile UserProfile { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("account_name")]
        public string AccountName { get; set; }

        [JsonProperty("user_path")]
        public string UserPath { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("is_follow")]
        public bool IsFollow { get; set; }

        [JsonProperty("is_follow_backed")]
        public bool IsFollowBacked { get; set; }

        [JsonProperty("is_push_registered")]
        public bool IsPushRegistered { get; set; }
    }

    public partial class LiveSentItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }
    }
}
