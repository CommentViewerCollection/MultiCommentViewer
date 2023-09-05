using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineLiveSitePlugin.Low.ChannelInfo
{
    public partial class RootObject
    {
        [JsonProperty("apistatusCode")]
        public long ApistatusCode { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("statusMessage")]
        public string StatusMessage { get; set; }

        [JsonProperty("iconURL")]
        public string IconUrl { get; set; }

        [JsonProperty("latestBroadcastId")]
        public long LatestBroadcastId { get; set; }

        [JsonProperty("broadcastCount")]
        public long BroadcastCount { get; set; }

        [JsonProperty("followerCount")]
        public long FollowerCount { get; set; }

        [JsonProperty("loveCount")]
        public long LoveCount { get; set; }

        [JsonProperty("premiumLoveCount")]
        public long PremiumLoveCount { get; set; }

        [JsonProperty("shareURL")]
        public string ShareUrl { get; set; }

        [JsonProperty("isBroadcastingNow")]
        public bool IsBroadcastingNow { get; set; }

        [JsonProperty("isUpdated")]
        public bool IsUpdated { get; set; }

        [JsonProperty("facebookId")]
        public object FacebookId { get; set; }

        [JsonProperty("facebookURL")]
        public object FacebookUrl { get; set; }

        [JsonProperty("instagramId")]
        public object InstagramId { get; set; }

        [JsonProperty("instagramURL")]
        public object InstagramUrl { get; set; }

        [JsonProperty("twitterId")]
        public string TwitterId { get; set; }

        [JsonProperty("twitterURL")]
        public string TwitterUrl { get; set; }

        [JsonProperty("lineScheme")]
        public object LineScheme { get; set; }

        [JsonProperty("isFollowing")]
        public object IsFollowing { get; set; }

        [JsonProperty("information")]
        public string Information { get; set; }

        [JsonProperty("upcomingCount")]
        public long UpcomingCount { get; set; }

        [JsonProperty("archivedBroadcasts")]
        public ArchivedBroadcasts ArchivedBroadcasts { get; set; }

        [JsonProperty("liveBroadcasts")]
        public LiveBroadcasts LiveBroadcasts { get; set; }

        [JsonProperty("upcomings")]
        public Upcomings Upcomings { get; set; }

        [JsonProperty("createdAt")]
        public long CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonProperty("isNotificationEnabled")]
        public object IsNotificationEnabled { get; set; }

        [JsonProperty("mid")]
        public object Mid { get; set; }

        [JsonProperty("badges")]
        public List<object> Badges { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }
    }

    public partial class ArchivedBroadcasts
    {
        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonProperty("rows")]
        public List<ArchivedBroadcastsRow> Rows { get; set; }

        [JsonProperty("apistatusCode")]
        public long ApistatusCode { get; set; }
    }

    public partial class ArchivedBroadcastsRow
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
        public long? ChatCount { get; set; }

        [JsonProperty("thumbnailURLs")]
        public ThumbnailUrLs ThumbnailUrLs { get; set; }

        [JsonProperty("autoPlayURL")]
        public string AutoPlayUrl { get; set; }

        [JsonProperty("vodLastsceneURL")]
        public string VodLastsceneUrl { get; set; }

        [JsonProperty("shareURL")]
        public string ShareUrl { get; set; }

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
        public PurpleChannel Channel { get; set; }

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
    }

    public partial class PurpleChannel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconURL")]
        public string IconUrl { get; set; }

        [JsonProperty("statusMessage")]
        public string StatusMessage { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("isFollowing")]
        public object IsFollowing { get; set; }

        [JsonProperty("mid")]
        public object Mid { get; set; }
    }

    public partial class ThumbnailUrLs
    {
        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }

        [JsonProperty("commonLarge")]
        public string CommonLarge { get; set; }

        [JsonProperty("commonSmall")]
        public string CommonSmall { get; set; }

        [JsonProperty("large1x1")]
        public string Large1X1 { get; set; }

        [JsonProperty("small1x1")]
        public string Small1X1 { get; set; }

        [JsonProperty("landscape")]
        public string Landscape { get; set; }

        [JsonProperty("swipe")]
        public string Swipe { get; set; }
    }

    public partial class LiveBroadcasts
    {
        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonProperty("rows")]
        public List<LiveBroadcastsRow> Rows { get; set; }

        [JsonProperty("apistatusCode")]
        public long ApistatusCode { get; set; }
    }

    public partial class LiveBroadcastsRow
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
        public long? ChatCount { get; set; }

        [JsonProperty("thumbnailURLs")]
        public ThumbnailUrLs ThumbnailUrLs { get; set; }

        [JsonProperty("autoPlayURL")]
        public string AutoPlayUrl { get; set; }

        [JsonProperty("vodLastsceneURL")]
        public object VodLastsceneUrl { get; set; }

        [JsonProperty("shareURL")]
        public string ShareUrl { get; set; }

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
        public FluffyChannel Channel { get; set; }

        [JsonProperty("tags")]
        public List<object> Tags { get; set; }

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
    }

    public partial class FluffyChannel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("iconURL")]
        public string IconUrl { get; set; }

        [JsonProperty("statusMessage")]
        public string StatusMessage { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("isFollowing")]
        public object IsFollowing { get; set; }

        [JsonProperty("mid")]
        public object Mid { get; set; }
    }

    public partial class Upcomings
    {
        [JsonProperty("hasNextPage")]
        public bool HasNextPage { get; set; }

        [JsonProperty("rows")]
        public List<object> Rows { get; set; }

        [JsonProperty("apistatusCode")]
        public long ApistatusCode { get; set; }
    }
}
