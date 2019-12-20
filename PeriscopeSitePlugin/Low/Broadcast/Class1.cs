using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin.Low.Broadcast
{
    public partial class RootObject
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

        [JsonProperty("has_location")]
        public bool RootObjectHasLocation { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("country_state")]
        public string CountryState { get; set; }

        [JsonProperty("iso_code")]
        public string IsoCode { get; set; }

        [JsonProperty("ip_lat")]
        public long IpLat { get; set; }

        [JsonProperty("ip_lng")]
        public long IpLng { get; set; }

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

        [JsonProperty("image_url_medium")]
        public Uri ImageUrlMedium { get; set; }

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

        [JsonProperty("is_high_latency")]
        public bool IsHighLatency { get; set; }

        [JsonProperty("n_total_watching")]
        public long NTotalWatching { get; set; }

        [JsonProperty("n_watching")]
        public long NWatching { get; set; }

        [JsonProperty("n_web_watching")]
        public long NWebWatching { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("broadcastHasUpdated")]
        public bool BroadcastHasUpdated { get; set; }

        [JsonProperty("isRunning")]
        public bool IsRunning { get; set; }

        [JsonProperty("isUnavailable")]
        public bool IsUnavailable { get; set; }

        [JsonProperty("isEnded")]
        public bool IsEnded { get; set; }

        [JsonProperty("interstitial")]
        public bool Interstitial { get; set; }

        [JsonProperty("endTime")]
        public DateTimeOffset EndTime { get; set; }

        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("formattedDuration")]
        public string FormattedDuration { get; set; }

        [JsonProperty("hasLocation")]
        public string HasLocation { get; set; }

        [JsonProperty("hasFeaturedCategory")]
        public bool HasFeaturedCategory { get; set; }

        [JsonProperty("getFeaturedColor")]
        public string GetFeaturedColor { get; set; }

        [JsonProperty("has360Video")]
        public bool Has360Video { get; set; }

        [JsonProperty("supportsProducerFraming")]
        public bool SupportsProducerFraming { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("relativeUrl")]
        public string RelativeUrl { get; set; }

        [JsonProperty("requestedUrl")]
        public Uri RequestedUrl { get; set; }

        [JsonProperty("canonicalUrl")]
        public Uri CanonicalUrl { get; set; }

        [JsonProperty("urlWithUsername")]
        public Uri UrlWithUsername { get; set; }

        [JsonProperty("relativeUrlWithUsername")]
        public string RelativeUrlWithUsername { get; set; }

        [JsonProperty("deepLinkUrl")]
        public string DeepLinkUrl { get; set; }

        [JsonProperty("inAppUrl")]
        public string InAppUrl { get; set; }

        [JsonProperty("cardPlayerUrl")]
        public Uri CardPlayerUrl { get; set; }

        [JsonProperty("localThumbnailUrl")]
        public string LocalThumbnailUrl { get; set; }

        [JsonProperty("locationDescription")]
        public string LocationDescription { get; set; }

        [JsonProperty("isWithin24Hours")]
        public bool IsWithin24Hours { get; set; }
    }

    public partial class Data
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
        public long IpLat { get; set; }

        [JsonProperty("ip_lng")]
        public long IpLng { get; set; }

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

        [JsonProperty("image_url_medium")]
        public Uri ImageUrlMedium { get; set; }

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

        [JsonProperty("is_high_latency")]
        public bool IsHighLatency { get; set; }

        [JsonProperty("n_total_watching")]
        public long NTotalWatching { get; set; }

        [JsonProperty("n_watching")]
        public long NWatching { get; set; }

        [JsonProperty("n_web_watching")]
        public long NWebWatching { get; set; }
    }
}
