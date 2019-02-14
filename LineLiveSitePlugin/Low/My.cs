using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineLiveSitePlugin.Low.My
{
    public partial class RootObject
    {
        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("visitedBroadcasts")]
        public List<VisitedBroadcast> VisitedBroadcasts { get; set; }

        [JsonProperty("channel")]
        public object Channel { get; set; }

        [JsonProperty("broadcast")]
        public RootObjectBroadcast Broadcast { get; set; }

        [JsonProperty("followingsCount")]
        public long FollowingsCount { get; set; }

        [JsonProperty("coin")]
        public object Coin { get; set; }

        [JsonProperty("apistatusCode")]
        public long ApistatusCode { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }
    }

    public partial class RootObjectBroadcast
    {
        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonProperty("rows")]
        public List<object> Rows { get; set; }
    }

    public partial class User
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("iconURL")]
        public Uri IconUrl { get; set; }

        [JsonProperty("twitterScreenName")]
        public object TwitterScreenName { get; set; }

        [JsonProperty("facebookScreenName")]
        public object FacebookScreenName { get; set; }

        [JsonProperty("hasLineAccount")]
        public bool HasLineAccount { get; set; }
    }

    public partial class VisitedBroadcast
    {
        [JsonProperty("visitedAt")]
        public long VisitedAt { get; set; }

        [JsonProperty("broadcast")]
        public VisitedBroadcastBroadcast Broadcast { get; set; }
    }

    public partial class VisitedBroadcastBroadcast
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("channelId")]
        public long ChannelId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("viewerCount")]
        public long ViewerCount { get; set; }

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

        [JsonProperty("limitedLoveExchangeRate")]
        public long LimitedLoveExchangeRate { get; set; }

        [JsonProperty("maxOwnedLimitedLoveCount")]
        public long MaxOwnedLimitedLoveCount { get; set; }

        [JsonProperty("chatCount")]
        public long ChatCount { get; set; }

        [JsonProperty("thumbnailURLs")]
        public ThumbnailUrLs ThumbnailUrLs { get; set; }

        [JsonProperty("autoPlayURL")]
        public Uri AutoPlayUrl { get; set; }

        [JsonProperty("vodLastsceneURL")]
        public Uri VodLastsceneUrl { get; set; }

        [JsonProperty("shareURL")]
        public Uri ShareUrl { get; set; }

        [JsonProperty("broadcastSecretToken")]
        public object BroadcastSecretToken { get; set; }

        [JsonProperty("numericAspectRatio")]
        public double NumericAspectRatio { get; set; }

        [JsonProperty("liveStatus")]
        public string LiveStatus { get; set; }

        [JsonProperty("archiveStatus")]
        public string ArchiveStatus { get; set; }

        [JsonProperty("archiveDuration")]
        public long ArchiveDuration { get; set; }

        [JsonProperty("finishedBroadcastingAt")]
        public long FinishedBroadcastingAt { get; set; }

        [JsonProperty("createdAt")]
        public long CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("apistatusCode")]
        public long ApistatusCode { get; set; }

        [JsonProperty("isBroadcastingNow")]
        public bool IsBroadcastingNow { get; set; }

        [JsonProperty("isOAFollowRequired")]
        public bool IsOaFollowRequired { get; set; }

        [JsonProperty("isArchived")]
        public bool IsArchived { get; set; }

        [JsonProperty("isBanned")]
        public bool IsBanned { get; set; }

        [JsonProperty("isRadioMode")]
        public bool IsRadioMode { get; set; }

        [JsonProperty("isCollaboratable")]
        public bool IsCollaboratable { get; set; }

        [JsonProperty("supportGauge")]
        public object SupportGauge { get; set; }

        [JsonProperty("isEventParticipant")]
        public bool IsEventParticipant { get; set; }

        [JsonProperty("hasChallenge")]
        public bool HasChallenge { get; set; }
    }

    public partial class Channel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconURL")]
        public Uri IconUrl { get; set; }

        [JsonProperty("statusMessage")]
        public string StatusMessage { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("isFollowing")]
        public object IsFollowing { get; set; }

        [JsonProperty("isOfficialCertifiedChannel")]
        public bool IsOfficialCertifiedChannel { get; set; }

        [JsonProperty("mid")]
        public object Mid { get; set; }
    }

    public partial class ThumbnailUrLs
    {
        [JsonProperty("small")]
        public Uri Small { get; set; }

        [JsonProperty("large")]
        public Uri Large { get; set; }

        [JsonProperty("commonLarge")]
        public Uri CommonLarge { get; set; }

        [JsonProperty("commonSmall")]
        public Uri CommonSmall { get; set; }

        [JsonProperty("large1x1")]
        public Uri Large1X1 { get; set; }

        [JsonProperty("small1x1")]
        public Uri Small1X1 { get; set; }

        [JsonProperty("landscape")]
        public Uri Landscape { get; set; }

        [JsonProperty("swipe")]
        public Uri Swipe { get; set; }
    }
}
