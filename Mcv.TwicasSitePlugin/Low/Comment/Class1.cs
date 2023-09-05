using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwicasSitePlugin.Low.Comment
{
    public partial class RootObject
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("rawMessage")]
        public string RawMessage { get; set; }

        [JsonProperty("hasMention")]
        public bool HasMention { get; set; }

        [JsonProperty("createdAt")]
        public long CreatedAt { get; set; }

        [JsonProperty("specialImage")]
        public object SpecialImage { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("numComments")]
        public long NumComments { get; set; }
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
        public string ProfileImage { get; set; }

        [JsonProperty("grade")]
        public long Grade { get; set; }
    }
}
