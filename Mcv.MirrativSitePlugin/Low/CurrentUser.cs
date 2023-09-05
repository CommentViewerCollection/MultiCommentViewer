using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrativSitePlugin.Low.CurrentUser
{
    public partial class RootObject
    {
        [JsonProperty("share_url")]
        public Uri ShareUrl { get; set; }

        [JsonProperty("current_continuous_record")]
        public long CurrentContinuousRecord { get; set; }

        [JsonProperty("custom_thanks_message")]
        public string CustomThanksMessage { get; set; }

        [JsonProperty("registered_at")]
        public long RegisteredAt { get; set; }

        [JsonProperty("profile_image_url")]
        public Uri ProfileImageUrl { get; set; }

        [JsonProperty("header_image_url")]
        public string HeaderImageUrl { get; set; }

        [JsonProperty("badges")]
        public List<object> Badges { get; set; }

        [JsonProperty("follower_num")]
        public long FollowerNum { get; set; }

        [JsonProperty("is_new")]
        public long IsNew { get; set; }

        [JsonProperty("following_num")]
        public long FollowingNum { get; set; }

        [JsonProperty("max_continuous_record")]
        public long MaxContinuousRecord { get; set; }

        [JsonProperty("links")]
        public List<object> Links { get; set; }

        [JsonProperty("grade_id")]
        public long GradeId { get; set; }

        [JsonProperty("twitter_screen_name")]
        public string TwitterScreenName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("properties")]
        public List<object> Properties { get; set; }

        [JsonProperty("total_viewer_num")]
        public long TotalViewerNum { get; set; }

        [JsonProperty("is_continuous_streamer")]
        public long IsContinuousStreamer { get; set; }

        [JsonProperty("live_announcement")]
        public object LiveAnnouncement { get; set; }

        [JsonProperty("paypal_username")]
        public string PaypalUsername { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("live_request_num")]
        public long LiveRequestNum { get; set; }

        [JsonProperty("onlive")]
        public object Onlive { get; set; }
    }
}
