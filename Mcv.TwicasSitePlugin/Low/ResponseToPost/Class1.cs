using Newtonsoft.Json;
using System;

namespace TwicasSitePlugin.Low.ResponseToPost
{
    public partial class RootObject
    {
        [JsonProperty("error")]
        public object Error { get; set; }

        [JsonProperty("comments")]
        public Comment[] Comments { get; set; }
    }

    public partial class Comment
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("createdAt")]
        public long CreatedAt { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }
    }

    public partial class Author
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("screenName")]
        public string ScreenName { get; set; }

        [JsonProperty("profileImage")]
        public Uri ProfileImage { get; set; }

        [JsonProperty("grade")]
        public long Grade { get; set; }
    }
}
