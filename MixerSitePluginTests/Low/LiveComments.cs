using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MirrativSitePlugin.Low.LiveComments
{
    public partial class RootObject
    {
        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("comments")]
        public List<Comment> Comments { get; set; }
    }

    public partial class Comment
    {
        [JsonProperty("badge_image_url")]
        public string BadgeImageUrl { get; set; }

        [JsonProperty("profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("is_continuous_streamer")]
        public long IsContinuousStreamer { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("comment")]
        public string CommentComment { get; set; }

        [JsonProperty("is_moderator")]
        public long IsModerator { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }
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
