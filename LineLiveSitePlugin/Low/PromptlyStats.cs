using Newtonsoft.Json;
using System;

namespace LineLiveSitePlugin.Low.PromptyStats
{
    public class RootObject
    {
        [JsonProperty("loveCount")]
        public long LoveCount { get; set; }

        [JsonProperty("freeLoveCount")]
        public long FreeLoveCount { get; set; }

        [JsonProperty("premiumLoveCount")]
        public long PremiumLoveCount { get; set; }

        [JsonProperty("limitedLoveCount")]
        public long LimitedLoveCount { get; set; }

        [JsonProperty("ownedLimitedLoveCount")]
        public long OwnedLimitedLoveCount { get; set; }

        [JsonProperty("sentLimitedLoveCount")]
        public long SentLimitedLoveCount { get; set; }

        [JsonProperty("viewerCount")]
        public long ViewerCount { get; set; }

        [JsonProperty("chatCount")]
        public long ChatCount { get; set; }

        [JsonProperty("liveStatus")]
        public string LiveStatus { get; set; }

        [JsonProperty("liveStartedAt")]
        public object LiveStartedAt { get; set; }

        [JsonProperty("currentViewerCount")]
        public object CurrentViewerCount { get; set; }

        [JsonProperty("apistatusCode")]
        public int ApistatusCode { get; set; }

        [JsonProperty("isCollaboratable")]
        public bool IsCollaboratable { get; set; }

        [JsonProperty("isCollaborating")]
        public bool IsCollaborating { get; set; }

        [JsonProperty("canRequestCollaboration")]
        public bool CanRequestCollaboration { get; set; }

        [JsonProperty("pinnedMessage")]
        public object PinnedMessage { get; set; }

        [JsonProperty("badges")]
        public Badge[] Badges { get; set; }

        [JsonProperty("giftRanking")]
        public GiftRanking[] GiftRanking { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
    }

    public partial class Badge
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("destinationUrl")]
        public Uri DestinationUrl { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }

        [JsonProperty("iconUrl")]
        public Uri IconUrl { get; set; }
    }

    public partial class GiftRanking
    {
        [JsonProperty("rank")]
        public long Rank { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("hashedId")]
        public string HashedId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("iconUrl")]
        public Uri IconUrl { get; set; }

        [JsonProperty("isBlocked")]
        public bool IsBlocked { get; set; }

        [JsonProperty("isGuest")]
        public bool IsGuest { get; set; }
    }
}
