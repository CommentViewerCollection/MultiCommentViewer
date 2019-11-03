using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixerSitePlugin.Low.CurrentUser
{
    public class RootObject
    {
        [JsonProperty("avatarUrl")]
        public object AvatarUrl { get; set; }

        [JsonProperty("bio")]
        public object Bio { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("deletedAt")]
        public object DeletedAt { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("experience")]
        public long Experience { get; set; }

        [JsonProperty("frontendVersion")]
        public object FrontendVersion { get; set; }

        [JsonProperty("groups")]
        public Group[] Groups { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("preferences")]
        public Preferences Preferences { get; set; }

        [JsonProperty("primaryTeam")]
        public object PrimaryTeam { get; set; }

        [JsonProperty("social")]
        public Social Social { get; set; }

        [JsonProperty("sparks")]
        public long Sparks { get; set; }

        [JsonProperty("storeSettings")]
        public StoreSettings StoreSettings { get; set; }

        [JsonProperty("twoFactor")]
        public TwoFactor TwoFactor { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }
    }

    public partial class Channel
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("audience")]
        public string Audience { get; set; }

        [JsonProperty("badgeId")]
        public object BadgeId { get; set; }

        [JsonProperty("bannerUrl")]
        public object BannerUrl { get; set; }

        [JsonProperty("costreamId")]
        public object CostreamId { get; set; }

        [JsonProperty("coverId")]
        public object CoverId { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("deletedAt")]
        public object DeletedAt { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("featured")]
        public bool Featured { get; set; }

        [JsonProperty("featureLevel")]
        public long FeatureLevel { get; set; }

        [JsonProperty("ftl")]
        public long Ftl { get; set; }

        [JsonProperty("hasTranscodes")]
        public bool HasTranscodes { get; set; }

        [JsonProperty("hasVod")]
        public bool HasVod { get; set; }

        [JsonProperty("hosteeId")]
        public object HosteeId { get; set; }

        [JsonProperty("interactive")]
        public bool Interactive { get; set; }

        [JsonProperty("interactiveGameId")]
        public object InteractiveGameId { get; set; }

        [JsonProperty("languageId")]
        public object LanguageId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("numFollowers")]
        public long NumFollowers { get; set; }

        [JsonProperty("online")]
        public bool Online { get; set; }

        [JsonProperty("partnered")]
        public bool Partnered { get; set; }

        [JsonProperty("suspended")]
        public bool Suspended { get; set; }

        [JsonProperty("thumbnailId")]
        public object ThumbnailId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("transcodingProfileId")]
        public long TranscodingProfileId { get; set; }

        [JsonProperty("typeId")]
        public object TypeId { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("viewersCurrent")]
        public long ViewersCurrent { get; set; }

        [JsonProperty("viewersTotal")]
        public long ViewersTotal { get; set; }

        [JsonProperty("vodsEnabled")]
        public bool VodsEnabled { get; set; }
    }

    public partial class Group
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Preferences
    {
        [JsonProperty("chat:sounds:play")]
        public string ChatSoundsPlay { get; set; }

        [JsonProperty("chat:sounds:html5")]
        public bool ChatSoundsHtml5 { get; set; }

        [JsonProperty("chat:timestamps")]
        public bool ChatTimestamps { get; set; }

        [JsonProperty("chat:whispers")]
        public bool ChatWhispers { get; set; }

        [JsonProperty("chat:chromakey")]
        public bool ChatChromakey { get; set; }

        [JsonProperty("chat:lurkmode")]
        public bool ChatLurkmode { get; set; }

        [JsonProperty("channel:notifications")]
        public ChannelNotifications ChannelNotifications { get; set; }

        [JsonProperty("channel:mature:allowed")]
        public bool ChannelMatureAllowed { get; set; }

        [JsonProperty("channel:player")]
        public ChannelPlayer ChannelPlayer { get; set; }

        [JsonProperty("chat:tagging")]
        public bool ChatTagging { get; set; }

        [JsonProperty("chat:colors")]
        public bool ChatColors { get; set; }

        [JsonProperty("chat:sounds:volume")]
        public long ChatSoundsVolume { get; set; }

        [JsonProperty("chat:ignoredUsers")]
        public object[] ChatIgnoredUsers { get; set; }

        [JsonProperty("channel:chatfilter:threshold")]
        public long ChannelChatfilterThreshold { get; set; }

        [JsonProperty("global:dialog:seenFtue")]
        public bool GlobalDialogSeenFtue { get; set; }

        [JsonProperty("global:skills:completedFtue")]
        public bool GlobalSkillsCompletedFtue { get; set; }
    }

    public partial class ChannelNotifications
    {
        [JsonProperty("ids")]
        public string[] Ids { get; set; }

        [JsonProperty("transports")]
        public string[] Transports { get; set; }
    }

    public partial class ChannelPlayer
    {
        [JsonProperty("vod")]
        public string Vod { get; set; }

        [JsonProperty("rtmp")]
        public string Rtmp { get; set; }

        [JsonProperty("ftl")]
        public string Ftl { get; set; }
    }

    public partial class Social
    {
        [JsonProperty("verified")]
        public object[] Verified { get; set; }
    }

    public partial class StoreSettings
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("msaCountryCode")]
        public string MsaCountryCode { get; set; }
    }

    public partial class TwoFactor
    {
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("codesViewed")]
        public bool CodesViewed { get; set; }
    }
}
