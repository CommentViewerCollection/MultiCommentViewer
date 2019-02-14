using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WhowatchSitePlugin.MessageParsing.Shout
{
    public class Payload
    {
        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("comment")]
        public Comment Comment { get; set; }
    }
    public partial class Comment
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("reply_to_user_id")]
        public long ReplyToUserId { get; set; }

        [JsonProperty("posted_at")]
        public long PostedAt { get; set; }

        [JsonProperty("not_escaped")]
        public bool NotEscaped { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("live_id")]
        public long LiveId { get; set; }

        [JsonProperty("is_silent_comment")]
        public bool IsSilentComment { get; set; }

        [JsonProperty("is_reply_to_me")]
        public bool IsReplyToMe { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("escaped_message")]
        public string EscapedMessage { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("comment_type")]
        public string CommentType { get; set; }

        [JsonProperty("anonymized")]
        public bool Anonymized { get; set; }
    }

    public partial class User
    {
        [JsonProperty("user_profile")]
        public UserProfile UserProfile { get; set; }

        [JsonProperty("user_path")]
        public string UserPath { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("icon_url")]
        public Uri IconUrl { get; set; }

        [JsonProperty("account_name")]
        public string AccountName { get; set; }
    }

    public partial class UserProfile
    {
        [JsonProperty("is_date_of_birth_today")]
        public bool IsDateOfBirthToday { get; set; }
    }
}