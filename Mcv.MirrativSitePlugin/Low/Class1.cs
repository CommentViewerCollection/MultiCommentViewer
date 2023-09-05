using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrativSitePlugin.Low.UserProfile
{
    public partial class RootObject
    {
        [JsonProperty("ambassador_image_url")]
        public string AmbassadorImageUrl { get; set; }

        [JsonProperty("current_continuous_record")]
        public string CurrentContinuousRecord { get; set; }

        [JsonProperty("custom_thanks_message")]
        public string CustomThanksMessage { get; set; }

        [JsonProperty("registered_at")]
        public string RegisteredAt { get; set; }

        [JsonProperty("profile_image_url")]
        public Uri ProfileImageUrl { get; set; }

        [JsonProperty("header_image_url")]
        public string HeaderImageUrl { get; set; }

        [JsonProperty("badges")]
        public Badge[] Badges { get; set; }

        [JsonProperty("follower_num")]
        public string FollowerNum { get; set; }

        [JsonProperty("my_app_num")]
        public long MyAppNum { get; set; }

        [JsonProperty("links")]
        public object[] Links { get; set; }

        [JsonProperty("grade_id")]
        public long GradeId { get; set; }

        [JsonProperty("twitter_screen_name")]
        public object TwitterScreenName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("total_viewer_num")]
        public long TotalViewerNum { get; set; }

        [JsonProperty("properties")]
        public object[] Properties { get; set; }

        [JsonProperty("live_announcement")]
        public object LiveAnnouncement { get; set; }

        [JsonProperty("is_blocking")]
        public long IsBlocking { get; set; }

        [JsonProperty("paypal_username")]
        public string PaypalUsername { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("onlive")]
        public Onlive Onlive { get; set; }

        [JsonProperty("share_url")]
        public Uri ShareUrl { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("is_new")]
        public long IsNew { get; set; }

        [JsonProperty("following_num")]
        public string FollowingNum { get; set; }

        [JsonProperty("kakao_name")]
        public string KakaoName { get; set; }

        [JsonProperty("is_follower")]
        public long IsFollower { get; set; }

        [JsonProperty("max_continuous_record")]
        public string MaxContinuousRecord { get; set; }

        [JsonProperty("chat_enabled")]
        public long ChatEnabled { get; set; }

        [JsonProperty("is_continuous_streamer")]
        public long IsContinuousStreamer { get; set; }

        [JsonProperty("is_following")]
        public long IsFollowing { get; set; }

        [JsonProperty("live_request_num")]
        public string LiveRequestNum { get; set; }
    }

    public partial class Badge
    {
        [JsonProperty("image_url")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("small_image_url")]
        public Uri SmallImageUrl { get; set; }
    }

    public partial class Onlive
    {
        [JsonProperty("live_id")]
        public string LiveId { get; set; }
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
}
