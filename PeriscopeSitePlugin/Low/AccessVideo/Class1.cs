using Newtonsoft.Json;
using System;

namespace PeriscopeSitePlugin.Low.AccessVideo
{
    public partial class RootObject
    {
        [JsonProperty("session")]
        public string Session { get; set; }

        [JsonProperty("hls_url")]
        public Uri HlsUrl { get; set; }

        [JsonProperty("lhls_url")]
        public Uri LhlsUrl { get; set; }

        [JsonProperty("lhlsweb_url")]
        public Uri LhlswebUrl { get; set; }

        [JsonProperty("https_hls_url")]
        public Uri HttpsHlsUrl { get; set; }

        [JsonProperty("hls_is_encrypted")]
        public bool HlsIsEncrypted { get; set; }

        [JsonProperty("lhls_is_encrypted")]
        public bool LhlsIsEncrypted { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("default_playback_buffer_length")]
        public long DefaultPlaybackBufferLength { get; set; }

        [JsonProperty("min_playback_buffer_length")]
        public long MinPlaybackBufferLength { get; set; }

        [JsonProperty("max_playback_buffer_length")]
        public long MaxPlaybackBufferLength { get; set; }

        [JsonProperty("chat_token")]
        public string ChatToken { get; set; }

        [JsonProperty("life_cycle_token")]
        public string LifeCycleToken { get; set; }

        [JsonProperty("broadcast")]
        public Broadcast Broadcast { get; set; }

        [JsonProperty("share_url")]
        public Uri ShareUrl { get; set; }

        [JsonProperty("autoplay_view_threshold")]
        public long AutoplayViewThreshold { get; set; }
    }

    public partial class Broadcast
    {
        [JsonProperty("class_name")]
        public string ClassName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_display_name")]
        public string UserDisplayName { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("twitter_id")]
        public string TwitterId { get; set; }

        [JsonProperty("twitter_username")]
        public string TwitterUsername { get; set; }

        [JsonProperty("profile_image_url")]
        public Uri ProfileImageUrl { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("is_locked")]
        public bool IsLocked { get; set; }

        [JsonProperty("friend_chat")]
        public bool FriendChat { get; set; }

        [JsonProperty("private_chat")]
        public bool PrivateChat { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("version")]
        public long Version { get; set; }

        [JsonProperty("start")]
        public DateTimeOffset Start { get; set; }

        [JsonProperty("ping")]
        public DateTimeOffset Ping { get; set; }

        [JsonProperty("has_moderation")]
        public bool HasModeration { get; set; }

        [JsonProperty("moderator_channel")]
        public string ModeratorChannel { get; set; }

        [JsonProperty("has_moderators")]
        public bool HasModerators { get; set; }

        [JsonProperty("enabled_sparkles")]
        public bool EnabledSparkles { get; set; }

        [JsonProperty("has_location")]
        public bool HasLocation { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("country_state")]
        public string CountryState { get; set; }

        [JsonProperty("iso_code")]
        public string IsoCode { get; set; }

        [JsonProperty("ip_lat")]
        public double IpLat { get; set; }

        [JsonProperty("ip_lng")]
        public double IpLng { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("camera_rotation")]
        public long CameraRotation { get; set; }

        [JsonProperty("image_url")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("image_url_small")]
        public Uri ImageUrlSmall { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("broadcast_source")]
        public string BroadcastSource { get; set; }

        [JsonProperty("available_for_replay")]
        public bool AvailableForReplay { get; set; }

        [JsonProperty("expiration")]
        public long Expiration { get; set; }

        [JsonProperty("tweet_id")]
        public string TweetId { get; set; }

        [JsonProperty("media_key")]
        public string MediaKey { get; set; }
    }
}
