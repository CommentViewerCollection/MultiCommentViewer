using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrativSitePlugin.Low.LiveInfo
{
    public partial class RootObject
    {
        [JsonProperty("is_private")]
        public long IsPrivate { get; set; }

        [JsonProperty("shares")]
        public Shares Shares { get; set; }

        [JsonProperty("streaming_url_hls")]
        public Uri StreamingUrlHls { get; set; }

        [JsonProperty("collab_supported")]
        public long CollabSupported { get; set; }

        [JsonProperty("sticker_enabled")]
        public long StickerEnabled { get; set; }

        [JsonProperty("is_gift_supported")]
        public long IsGiftSupported { get; set; }

        [JsonProperty("collab_has_vacancy")]
        public long CollabHasVacancy { get; set; }

        [JsonProperty("stamp_num")]
        public long StampNum { get; set; }

        [JsonProperty("streaming_key")]
        public string StreamingKey { get; set; }

        [JsonProperty("linked_live")]
        public object LinkedLive { get; set; }

        [JsonProperty("live_id")]
        public string LiveId { get; set; }

        [JsonProperty("collab_online_user_num")]
        public string CollabOnlineUserNum { get; set; }

        [JsonProperty("broadcast_key")]
        public string BroadcastKey { get; set; }

        [JsonProperty("is_mirrorable")]
        public long IsMirrorable { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("remaining_coins")]
        public long RemainingCoins { get; set; }

        [JsonProperty("total_viewer_num")]
        public long TotalViewerNum { get; set; }

        [JsonProperty("archive_url_hls")]
        public string ArchiveUrlHls { get; set; }

        [JsonProperty("sticker_category_ids")]
        public List<object> StickerCategoryIds { get; set; }

        [JsonProperty("ended_at")]
        public long EndedAt { get; set; }

        [JsonProperty("thumbnail_image_url")]
        public Uri ThumbnailImageUrl { get; set; }

        [JsonProperty("is_archive")]
        public long IsArchive { get; set; }

        [JsonProperty("online_user_num")]
        public long OnlineUserNum { get; set; }

        [JsonProperty("announcement_url")]
        public string AnnouncementUrl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("share_url")]
        public Uri ShareUrl { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("avatar")]
        public Avatar Avatar { get; set; }

        [JsonProperty("app_id")]
        public string AppId { get; set; }

        [JsonProperty("orientation")]
        public string Orientation { get; set; }

        [JsonProperty("is_muted")]
        public string IsMuted { get; set; }

        [JsonProperty("timeline")]
        public List<Timeline> Timeline { get; set; }

        [JsonProperty("app_icon_urls")]
        public List<Uri> AppIconUrls { get; set; }

        [JsonProperty("max_online_viewer_num")]
        public long MaxOnlineViewerNum { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        [JsonProperty("ad")]
        public Ad Ad { get; set; }

        [JsonProperty("is_live")]
        public long IsLive { get; set; }

        [JsonProperty("started_at")]
        public long StartedAt { get; set; }

        [JsonProperty("preview_blur_image_url")]
        public Uri PreviewBlurImageUrl { get; set; }

        [JsonProperty("is_paid_sticker_supported")]
        public long IsPaidStickerSupported { get; set; }

        [JsonProperty("image_url_without_letterbox")]
        public Uri ImageUrlWithoutLetterbox { get; set; }

        [JsonProperty("thumbnail_blur_image_url")]
        public Uri ThumbnailBlurImageUrl { get; set; }

        [JsonProperty("sticker_num")]
        public string StickerNum { get; set; }

        [JsonProperty("comment_num")]
        public long CommentNum { get; set; }

        [JsonProperty("max_collab_user_num")]
        public string MaxCollabUserNum { get; set; }

        [JsonProperty("owner")]
        public Owner Owner { get; set; }

        [JsonProperty("recommend_sticker_ids")]
        public List<object> RecommendStickerIds { get; set; }

        [JsonProperty("broadcast_port")]
        public long BroadcastPort { get; set; }

        [JsonProperty("sticker_display_type")]
        public string StickerDisplayType { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("broadcast_host")]
        public string BroadcastHost { get; set; }

        [JsonProperty("live_user_key")]
        public string LiveUserKey { get; set; }

        [JsonProperty("archive_comment_enabled")]
        public long ArchiveCommentEnabled { get; set; }

        [JsonProperty("streaming_url_edge")]
        public string StreamingUrlEdge { get; set; }

        [JsonProperty("collab_enabled")]
        public string CollabEnabled { get; set; }

        [JsonProperty("image_url")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("bcsvr_key")]
        public string BcsvrKey { get; set; }

        [JsonProperty("heartbeated_at")]
        public string HeartbeatedAt { get; set; }

        [JsonProperty("orientation_v2")]
        public string OrientationV2 { get; set; }
    }

    public partial class Ad
    {
        [JsonProperty("settings_app_install")]
        public SettingsAppInstall SettingsAppInstall { get; set; }
    }

    public partial class SettingsAppInstall
    {
        [JsonProperty("actions")]
        public List<Uri> Actions { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class Avatar
    {
        [JsonProperty("wipe_position")]
        public string WipePosition { get; set; }

        [JsonProperty("is_fullscreen")]
        public string IsFullscreen { get; set; }

        [JsonProperty("background")]
        public Background Background { get; set; }

        [JsonProperty("asset_bundle_url")]
        public Uri AssetBundleUrl { get; set; }

        [JsonProperty("camera")]
        public string Camera { get; set; }

        [JsonProperty("body")]
        public Body Body { get; set; }

        [JsonProperty("wipe_cameras")]
        public Dictionary<string, string> WipeCameras { get; set; }

        [JsonProperty("enabled")]
        public long Enabled { get; set; }
    }

    public partial class Background
    {
        [JsonProperty("icon_url")]
        public Uri IconUrl { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class Body
    {
        [JsonProperty("head")]
        public Head Head { get; set; }

        [JsonProperty("icon_url")]
        public Uri IconUrl { get; set; }

        [JsonProperty("hair_color")]
        public HairColor HairColor { get; set; }

        [JsonProperty("skin_color")]
        public string SkinColor { get; set; }

        [JsonProperty("asset_bundle_name")]
        public string AssetBundleName { get; set; }

        [JsonProperty("hat")]
        public Hat Hat { get; set; }

        [JsonProperty("clothes")]
        public Clothes Clothes { get; set; }

        [JsonProperty("eye")]
        public Eye Eye { get; set; }

        [JsonProperty("asset_bundle_prefab_name")]
        public string AssetBundlePrefabName { get; set; }

        [JsonProperty("proportion")]
        public Proportion Proportion { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("mouth")]
        public Mouth Mouth { get; set; }

        [JsonProperty("hair")]
        public Hair Hair { get; set; }

        [JsonProperty("hair_color_percentage")]
        public string HairColorPercentage { get; set; }
    }

    public partial class Clothes
    {
        [JsonProperty("color")]
        public ClothesColor Color { get; set; }

        [JsonProperty("icon_url")]
        public Uri IconUrl { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class ClothesColor
    {
        [JsonProperty("setup")]
        public Setup Setup { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class Setup
    {
        [JsonProperty("asset_bundle_prefab_name")]
        public string AssetBundlePrefabName { get; set; }

        [JsonProperty("asset_bundle_name")]
        public string AssetBundleName { get; set; }
    }

    public partial class Eye
    {
        [JsonProperty("color")]
        public EyeColor Color { get; set; }

        [JsonProperty("icon_url")]
        public Uri IconUrl { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class EyeColor
    {
        [JsonProperty("asset_bundle_prefab_postfix")]
        public string AssetBundlePrefabPostfix { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class Hair
    {
        [JsonProperty("icon_url")]
        public Uri IconUrl { get; set; }

        [JsonProperty("updated_at")]
        public long UpdatedAt { get; set; }

        [JsonProperty("asset_bundle_prefab_name")]
        public string AssetBundlePrefabName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("asset_bundle_name")]
        public string AssetBundleName { get; set; }
    }

    public partial class HairColor
    {
        [JsonProperty("gradient")]
        public List<long> Gradient { get; set; }
    }

    public partial class Hat
    {
        [JsonProperty("color")]
        public HatColor Color { get; set; }

        [JsonProperty("icon_url")]
        public Uri IconUrl { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class HatColor
    {
        [JsonProperty("asset_bundle_prefab_name")]
        public string AssetBundlePrefabName { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("asset_bundle_name")]
        public string AssetBundleName { get; set; }
    }

    public partial class Head
    {
        [JsonProperty("icon_url")]
        public Uri IconUrl { get; set; }

        [JsonProperty("updated_at")]
        public long UpdatedAt { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class Mouth
    {
        [JsonProperty("asset_bundle_prefab_postfix")]
        public string AssetBundlePrefabPostfix { get; set; }

        [JsonProperty("icon_url")]
        public Uri IconUrl { get; set; }

        [JsonProperty("updated_at")]
        public long UpdatedAt { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class Proportion
    {
        [JsonProperty("icon_url")]
        public Uri IconUrl { get; set; }

        [JsonProperty("updated_at")]
        public long UpdatedAt { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public partial class Owner
    {
        [JsonProperty("share_url")]
        public Uri ShareUrl { get; set; }

        [JsonProperty("profile_image_url")]
        public Uri ProfileImageUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("header_image_url")]
        public string HeaderImageUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("properties")]
        public List<object> Properties { get; set; }

        [JsonProperty("badges")]
        public List<object> Badges { get; set; }

        [JsonProperty("is_continuous_streamer")]
        public long IsContinuousStreamer { get; set; }

        [JsonProperty("is_new")]
        public long IsNew { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("live_request_num")]
        public string LiveRequestNum { get; set; }

        [JsonProperty("onlive")]
        public object Onlive { get; set; }
    }

    public partial class Shares
    {
        [JsonProperty("twitter")]
        public Twitter Twitter { get; set; }

        [JsonProperty("others")]
        public Others Others { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public partial class Others
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }

    public partial class Twitter
    {
        [JsonProperty("maxlength")]
        public long Maxlength { get; set; }

        [JsonProperty("card")]
        public Card Card { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("placeholder")]
        public string Placeholder { get; set; }
    }

    public partial class Card
    {
        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("image_url")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public partial class Status
    {
        [JsonProperty("msg")]
        public string Msg { get; set; }

        [JsonProperty("ok")]
        public long Ok { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("captcha_url")]
        public string CaptchaUrl { get; set; }

        [JsonProperty("error_code")]
        public long ErrorCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public partial class Timeline
    {
        [JsonProperty("app")]
        public App App { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public partial class App
    {
        [JsonProperty("is_my_app")]
        public long IsMyApp { get; set; }

        [JsonProperty("icon_url")]
        public Uri IconUrl { get; set; }

        [JsonProperty("store_url")]
        public Uri StoreUrl { get; set; }

        [JsonProperty("app_id")]
        public string AppId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("is_category")]
        public string IsCategory { get; set; }
    }
}
