namespace NicoSitePlugin.Next.Low.CommunityInfo
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Globalization;

    public partial class RootObject
    {
        [JsonProperty("nicovideo_community_response")]
        public NicovideoCommunityResponse NicovideoCommunityResponse { get; set; }
    }

    public partial class NicovideoCommunityResponse
    {
        [JsonProperty("community")]
        public Community Community { get; set; }

        [JsonProperty("@status")]
        public string Status { get; set; }
    }

    public partial class Community
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("channel_id")]
        public string ChannelId { get; set; }

        [JsonProperty("public")]
        public string Public { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("official")]
        public string Official { get; set; }

        [JsonProperty("option_flag")]
        public string OptionFlag { get; set; }

        [JsonProperty("hidden")]
        public string Hidden { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("create_time")]
        public System.DateTimeOffset CreateTime { get; set; }

        [JsonProperty("global_id")]
        public string GlobalId { get; set; }

        [JsonProperty("user_max")]
        public string UserMax { get; set; }

        [JsonProperty("user_count")]
        public string UserCount { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("option")]
        public Option Option { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("thumbnail_small")]
        public string ThumbnailSmall { get; set; }

        [JsonProperty("option_flag_details")]
        public OptionFlagDetails OptionFlagDetails { get; set; }

        [JsonProperty("top_url")]
        public string TopUrl { get; set; }

        [JsonProperty("@key")]
        public string Key { get; set; }
    }

    public partial class Option
    {
        [JsonProperty("adult_flag")]
        public string AdultFlag { get; set; }

        [JsonProperty("allow_display_vast")]
        public string AllowDisplayVast { get; set; }
    }

    public partial class OptionFlagDetails
    {
        [JsonProperty("community_priv_user_auth")]
        public string CommunityPrivUserAuth { get; set; }

        [JsonProperty("community_icon_upload")]
        public string CommunityIconUpload { get; set; }
    }
}
