using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLiveSitePlugin.Low.External.Movies
{
    public class RootObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("movie_id")]
        public long MovieId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("introduction")]
        public string Introduction { get; set; }

        [JsonProperty("is_live")]
        public bool IsLive { get; set; }

        [JsonProperty("onair_status")]
        public long OnairStatus { get; set; }

        [JsonProperty("movie_type")]
        public string MovieType { get; set; }

        [JsonProperty("monetize_status")]
        public long MonetizeStatus { get; set; }

        [JsonProperty("thumbnail_url")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("l_thumbnail_url")]
        public string LThumbnailUrl { get; set; }

        [JsonProperty("s_thumbnail_url")]
        public string SThumbnailUrl { get; set; }

        [JsonProperty("l_sprite_image_url")]
        public string LSpriteImageUrl { get; set; }

        [JsonProperty("s_sprite_image_url")]
        public string SSpriteImageUrl { get; set; }

        [JsonProperty("sprite_intervals")]
        public List<long> SpriteIntervals { get; set; }

        [JsonProperty("live_views")]
        public long? LiveViews { get; set; }

        [JsonProperty("total_views")]
        public long TotalViews { get; set; }

        [JsonProperty("total_yells")]
        public long TotalYells { get; set; }

        [JsonProperty("is_mobile")]
        public bool IsMobile { get; set; }

        [JsonProperty("is_low_latency")]
        public bool IsLowLatency { get; set; }

        [JsonProperty("is_dvr")]
        public bool IsDvr { get; set; }

        [JsonProperty("enabled_ad")]
        public bool EnabledAd { get; set; }

        [JsonProperty("enabled_yell")]
        public bool EnabledYell { get; set; }

        [JsonProperty("ad")]
        public Ad Ad { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("started_at")]
        public DateTimeOffset StartedAt { get; set; }

        [JsonProperty("ended_at")]
        public DateTimeOffset EndedAt { get; set; }

        [JsonProperty("will_start_at")]
        public object WillStartAt { get; set; }

        [JsonProperty("will_end_at")]
        public object WillEndAt { get; set; }

        [JsonProperty("start_time")]
        public long StartTime { get; set; }

        [JsonProperty("play_time")]
        public long PlayTime { get; set; }

        [JsonProperty("is_banned")]
        public bool IsBanned { get; set; }

        [JsonProperty("ban_type")]
        public long BanType { get; set; }

        [JsonProperty("device_type")]
        public long DeviceType { get; set; }

        [JsonProperty("orientation")]
        public long Orientation { get; set; }

        [JsonProperty("width")]
        public object Width { get; set; }

        [JsonProperty("height")]
        public object Height { get; set; }

        [JsonProperty("connect_count")]
        public long ConnectCount { get; set; }

        [JsonProperty("play_list_id")]
        public long PlayListId { get; set; }

        [JsonProperty("league_id")]
        public object LeagueId { get; set; }

        [JsonProperty("tags")]
        public List<object> Tags { get; set; }

        [JsonProperty("media")]
        public Media Media { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }

        [JsonProperty("chat_setting")]
        public ChatSetting ChatSetting { get; set; }

        [JsonProperty("game")]
        public Game Game { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }
    }

    public partial class Ad
    {
        [JsonProperty("web_stream")]
        public string WebStream { get; set; }
    }

    public partial class Channel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("recxuser_id")]
        public long RecxuserId { get; set; }

        [JsonProperty("blive_user_id")]
        public long BLiveUserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("introduction")]
        public string Introduction { get; set; }

        [JsonProperty("icon_image_url")]
        public string IconImageUrl { get; set; }

        [JsonProperty("cover_image_url")]
        public string CoverImageUrl { get; set; }

        [JsonProperty("movies")]
        public long Movies { get; set; }

        [JsonProperty("views")]
        public long Views { get; set; }

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

        [JsonProperty("chat_rule")]
        public string ChatRule { get; set; }

        [JsonProperty("blacklist")]
        public List<Blacklist> Blacklist { get; set; }
    }

    public partial class Blacklist
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

        [JsonProperty("is_live")]
        public bool IsLive { get; set; }

        [JsonProperty("live_views")]
        public long LiveViews { get; set; }
    }

    public partial class ChatSetting
    {
        [JsonProperty("limited_continuous_chat")]
        public bool LimitedContinuousChat { get; set; }

        [JsonProperty("continuous_chat_threshold")]
        public long ContinuousChatThreshold { get; set; }

        [JsonProperty("limited_unfollower_chat")]
        public bool LimitedUnfollowerChat { get; set; }

        [JsonProperty("unfollower_chat_threshold")]
        public long UnfollowerChatThreshold { get; set; }

        [JsonProperty("limited_fresh_user_chat")]
        public bool LimitedFreshUserChat { get; set; }

        [JsonProperty("fresh_user_chat_threshold")]
        public long FreshUserChatThreshold { get; set; }

        [JsonProperty("limited_temporary_blacklist")]
        public bool LimitedTemporaryBlacklist { get; set; }

        [JsonProperty("temporary_blacklist_threshold")]
        public long TemporaryBlacklistThreshold { get; set; }

        [JsonProperty("limited_warned_user_chat")]
        public bool LimitedWarnedUserChat { get; set; }

        [JsonProperty("chat_rule")]
        public string ChatRule { get; set; }
    }

    public partial class Game
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("game_id")]
        public long GameId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("introduction")]
        public string Introduction { get; set; }

        [JsonProperty("title_image_url")]
        public string TitleImageUrl { get; set; }

        [JsonProperty("license_status")]
        public long LicenseStatus { get; set; }

        [JsonProperty("notice_message")]
        public string NoticeMessage { get; set; }

        [JsonProperty("monetize_status")]
        public long MonetizeStatus { get; set; }

        [JsonProperty("cero_rating")]
        public long CeroRating { get; set; }

        [JsonProperty("is_portrait")]
        public bool IsPortrait { get; set; }

        [JsonProperty("maker")]
        public object Maker { get; set; }

        [JsonProperty("platform_mobile")]
        public string PlatformMobile { get; set; }

        [JsonProperty("platform_pc")]
        public string PlatformPc { get; set; }

        [JsonProperty("platform_console")]
        public string PlatformConsole { get; set; }

        [JsonProperty("platform_arcade")]
        public string PlatformArcade { get; set; }
    }

    public partial class Media
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("url_dvr")]
        public string UrlDvr { get; set; }

        [JsonProperty("url_source")]
        public object UrlSource { get; set; }

        [JsonProperty("url_public")]
        public string UrlPublic { get; set; }

        [JsonProperty("url_trailer")]
        public object UrlTrailer { get; set; }
    }
}
