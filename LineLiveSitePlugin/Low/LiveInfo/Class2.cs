using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineLiveSitePlugin.Low.LiveInfo
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class RootObject
    {
        [JsonProperty("liveHLSURLs")]
        public Dictionary<string, string> LiveHlsurLs { get; set; }

        [JsonProperty("archivedHLSURLs")]
        public Dictionary<string, object> ArchivedHlsurLs { get; set; }

        [JsonProperty("ad")]
        public object Ad { get; set; }

        [JsonProperty("pinnedMessage")]
        public object PinnedMessage { get; set; }

        [JsonProperty("badges")]
        public List<object> Badges { get; set; }

        [JsonProperty("supportGauge")]
        public object SupportGauge { get; set; }

        [JsonProperty("apistatusCode")]
        public long ApistatusCode { get; set; }

        [JsonProperty("item")]
        public Item Item { get; set; }

        [JsonProperty("chat")]
        public Chat Chat { get; set; }

        [JsonProperty("isFollowing")]
        public object IsFollowing { get; set; }

        [JsonProperty("isOAFollowRequired")]
        public bool IsOaFollowRequired { get; set; }

        [JsonProperty("isChannelBlocked")]
        public bool? IsChannelBlocked { get; set; }

        [JsonProperty("lsaPath")]
        public string LsaPath { get; set; }

        [JsonProperty("archiveBroadcastEndAt")]
        public object ArchiveBroadcastEndAt { get; set; }

        [JsonProperty("currentServerEpoch")]
        public long CurrentServerEpoch { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("isUseGift")]
        public bool IsUseGift { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("hotCasts")]
        public List<object> HotCasts { get; set; }

        [JsonProperty("hotCastsWithSection")]
        public HotCastsWithSection HotCastsWithSection { get; set; }

        [JsonProperty("isRadioMode")]
        public bool IsRadioMode { get; set; }

        [JsonProperty("currentViewerCount")]
        public object CurrentViewerCount { get; set; }

        [JsonProperty("isCollaborating")]
        public bool IsCollaborating { get; set; }

        [JsonProperty("isCollaboratable")]
        public bool IsCollaboratable { get; set; }

        [JsonProperty("canRequestCollaboration")]
        public bool CanRequestCollaboration { get; set; }

        [JsonProperty("isTrivia")]
        public bool IsTrivia { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }
    }

    public partial class Chat
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("archiveURL")]
        public object ArchiveUrl { get; set; }

        [JsonProperty("ownerMessageURL")]
        public string OwnerMessageUrl { get; set; }
    }

    public partial class HotCastsWithSection
    {
        [JsonProperty("broadcastingBroadcasts")]
        public List<object> BroadcastingBroadcasts { get; set; }

        [JsonProperty("categoryPopularBroadcasts")]
        public List<object> CategoryPopularBroadcasts { get; set; }

        [JsonProperty("homeHotBroadcasts")]
        public List<object> HomeHotBroadcasts { get; set; }
    }

    public partial class Item
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

        [JsonProperty("chatCount")]
        public long ChatCount { get; set; }

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
    }

    public partial class Channel
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
        public string Mid { get; set; }
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

    public partial class User
    {
        [JsonProperty("twitterScreenName")]
        public object TwitterScreenName { get; set; }

        [JsonProperty("facebookUserName")]
        public object FacebookUserName { get; set; }

        [JsonProperty("hasLineAccount")]
        public bool HasLineAccount { get; set; }
    }
}
