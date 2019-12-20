using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixerSitePlugin.Low.Channels
{
    public partial class RootObject
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("costreamId")]
        public object CostreamId { get; set; }

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

        [JsonProperty("numFollowers")]
        public long NumFollowers { get; set; }

        [JsonProperty("online")]
        public bool Online { get; set; }

        [JsonProperty("partnered")]
        public bool Partnered { get; set; }

        [JsonProperty("transcodingProfileId")]
        public long? TranscodingProfileId { get; set; }

        [JsonProperty("viewersCurrent")]
        public long ViewersCurrent { get; set; }

        [JsonProperty("audience")]
        public string Audience { get; set; }

        [JsonProperty("badgeId")]
        public object BadgeId { get; set; }

        [JsonProperty("bannerUrl")]
        public object BannerUrl { get; set; }

        [JsonProperty("coverId")]
        public long? CoverId { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("deletedAt")]
        public object DeletedAt { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("languageId")]
        public string LanguageId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("suspended")]
        public bool Suspended { get; set; }

        [JsonProperty("thumbnailId")]
        public long? ThumbnailId { get; set; }

        [JsonProperty("typeId")]
        public long? TypeId { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("viewersTotal")]
        public long ViewersTotal { get; set; }

        [JsonProperty("vodsEnabled")]
        public bool VodsEnabled { get; set; }

        [JsonProperty("badge")]
        public object Badge { get; set; }

        [JsonProperty("cover")]
        public Cover Cover { get; set; }

        [JsonProperty("type")]
        public TypeClass Type { get; set; }

        [JsonProperty("thumbnail")]
        public Thumbnail Thumbnail { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("preferences")]
        public Preferences Preferences { get; set; }
    }

    public partial class Cover
    {
        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("meta")]
        public CoverMeta Meta { get; set; }

        [JsonProperty("relid")]
        public object Relid { get; set; }

        [JsonProperty("remotePath")]
        public string RemotePath { get; set; }

        [JsonProperty("store")]
        public string Store { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public partial class CoverMeta
    {
        [JsonProperty("small")]
        public Uri Small { get; set; }
    }

    public partial class Preferences
    {
        [JsonProperty("costream:allow")]
        public string CostreamAllow { get; set; }

        [JsonProperty("mixer:featured:allow")]
        public bool MixerFeaturedAllow { get; set; }

        [JsonProperty("hosting:allow")]
        public bool HostingAllow { get; set; }

        [JsonProperty("hypezone:allow")]
        public bool HypezoneAllow { get; set; }

        [JsonProperty("hosting:allowlive")]
        public bool HostingAllowlive { get; set; }

        [JsonProperty("channel:offline:autoplayVod")]
        public bool ChannelOfflineAutoplayVod { get; set; }

        [JsonProperty("channel:bannedwords")]
        public object[] ChannelBannedwords { get; set; }

        [JsonProperty("channel:catbot:level")]
        public long ChannelCatbotLevel { get; set; }

        [JsonProperty("channel:directPurchase:enabled")]
        public bool ChannelDirectPurchaseEnabled { get; set; }

        [JsonProperty("channel:notify:directPurchaseMessage")]
        public string ChannelNotifyDirectPurchaseMessage { get; set; }

        [JsonProperty("channel:notify:donatemessage")]
        public string ChannelNotifyDonatemessage { get; set; }

        [JsonProperty("channel:donations:enabled")]
        public bool ChannelDonationsEnabled { get; set; }

        [JsonProperty("channel:notify:followmessage")]
        public string ChannelNotifyFollowmessage { get; set; }

        [JsonProperty("channel:notify:hostedBy")]
        public string ChannelNotifyHostedBy { get; set; }

        [JsonProperty("channel:notify:hosting")]
        public string ChannelNotifyHosting { get; set; }

        [JsonProperty("channel:links:allowed")]
        public bool ChannelLinksAllowed { get; set; }

        [JsonProperty("channel:links:clickable")]
        public bool ChannelLinksClickable { get; set; }

        [JsonProperty("channel:player:muteOwn")]
        public bool ChannelPlayerMuteOwn { get; set; }

        [JsonProperty("channel:notify:directPurchase")]
        public bool ChannelNotifyDirectPurchase { get; set; }

        [JsonProperty("channel:notify:donate")]
        public bool ChannelNotifyDonate { get; set; }

        [JsonProperty("channel:notify:follow")]
        public bool ChannelNotifyFollow { get; set; }

        [JsonProperty("channel:notify:subscribe")]
        public bool ChannelNotifySubscribe { get; set; }

        [JsonProperty("channel:notify:subscriptionGift")]
        public bool ChannelNotifySubscriptionGift { get; set; }

        [JsonProperty("channel:notify:subscriptionGiftMessage")]
        public string ChannelNotifySubscriptionGiftMessage { get; set; }

        [JsonProperty("sharetext")]
        public string Sharetext { get; set; }

        [JsonProperty("channel:slowchat")]
        public long ChannelSlowchat { get; set; }

        [JsonProperty("channel:partner:submail")]
        public string ChannelPartnerSubmail { get; set; }

        [JsonProperty("channel:notify:subscribemessage")]
        public string ChannelNotifySubscribemessage { get; set; }

        [JsonProperty("channel:chat:hostswitch")]
        public bool ChannelChatHostswitch { get; set; }

        [JsonProperty("channel:tweet:enabled")]
        public bool ChannelTweetEnabled { get; set; }

        [JsonProperty("channel:tweet:body")]
        public string ChannelTweetBody { get; set; }

        [JsonProperty("channel:users:levelRestrict")]
        public long ChannelUsersLevelRestrict { get; set; }
    }

    public partial class Thumbnail
    {
        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("meta")]
        public ThumbnailMeta Meta { get; set; }

        [JsonProperty("relid")]
        public long Relid { get; set; }

        [JsonProperty("remotePath")]
        public string RemotePath { get; set; }

        [JsonProperty("store")]
        public string Store { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public partial class ThumbnailMeta
    {
        [JsonProperty("size")]
        public long[] Size { get; set; }
    }

    public partial class TypeClass
    {
        [JsonProperty("availableAt")]
        public object AvailableAt { get; set; }

        [JsonProperty("backgroundUrl")]
        public Uri BackgroundUrl { get; set; }

        [JsonProperty("coverUrl")]
        public Uri CoverUrl { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("online")]
        public long Online { get; set; }

        [JsonProperty("parent")]
        public string Parent { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("viewersCurrent")]
        public long ViewersCurrent { get; set; }
    }

    public partial class User
    {
        [JsonProperty("avatarUrl")]
        public Uri AvatarUrl { get; set; }

        [JsonProperty("bio")]
        public string Bio { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("deletedAt")]
        public object DeletedAt { get; set; }

        [JsonProperty("experience")]
        public long Experience { get; set; }

        [JsonProperty("groups")]
        public Group[] Groups { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("primaryTeam")]
        public object PrimaryTeam { get; set; }

        [JsonProperty("social")]
        public Social Social { get; set; }

        [JsonProperty("sparks")]
        public long Sparks { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }
    }

    public partial class Group
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Social
    {
        [JsonProperty("facebook")]
        public Uri Facebook { get; set; }

        [JsonProperty("instagram")]
        public Uri Instagram { get; set; }

        [JsonProperty("soundcloud")]
        public Uri Soundcloud { get; set; }

        [JsonProperty("twitter")]
        public Uri Twitter { get; set; }

        [JsonProperty("verified")]
        public object[] Verified { get; set; }

        [JsonProperty("youtube")]
        public Uri Youtube { get; set; }
    }
}
