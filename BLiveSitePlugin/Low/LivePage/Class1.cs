using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLiveSitePlugin.Low.LivePage
{
    public partial class RootObject
    {
        [JsonProperty("leagueRankStore")]
        public LeagueRankStore LeagueRankStore { get; set; }

        [JsonProperty("moviePageStore")]
        public MoviePageStore MoviePageStore { get; set; }
    }

    public partial class LeagueRankStore
    {
        [JsonProperty("leagueKey")]
        public string LeagueKey { get; set; }
    }

    public partial class MoviePageStore
    {
        [JsonProperty("stores")]
        public object Stores { get; set; }

        [JsonProperty("yellStore")]
        public YellStore YellStore { get; set; }

        [JsonProperty("currentPlayerType")]
        public string CurrentPlayerType { get; set; }

        [JsonProperty("playerTypeUpdateIntervalId")]
        public long PlayerTypeUpdateIntervalId { get; set; }

        [JsonProperty("chatArticleWidth")]
        public long ChatArticleWidth { get; set; }

        [JsonProperty("popularLiveStreams")]
        public List<object> PopularLiveStreams { get; set; }

        [JsonProperty("liveStreamsOfSameGame")]
        public List<object> LiveStreamsOfSameGame { get; set; }

        [JsonProperty("moviesOfChannel")]
        public List<object> MoviesOfChannel { get; set; }

        [JsonProperty("chatPopoutWindow")]
        public object ChatPopoutWindow { get; set; }

        [JsonProperty("isChatPopoutPage")]
        public bool IsChatPopoutPage { get; set; }

        [JsonProperty("movieArticle")]
        public object MovieArticle { get; set; }

        [JsonProperty("chatHost")]
        public string ChatHost { get; set; }

        [JsonProperty("hasAlreadyPostedViewLog")]
        public bool HasAlreadyPostedViewLog { get; set; }

        [JsonProperty("_socket")]
        public object Socket { get; set; }

        [JsonProperty("movieStore")]
        public MovieStore MovieStore { get; set; }

        [JsonProperty("chatSettingStore")]
        public ChatSettingStore ChatSettingStore { get; set; }

        [JsonProperty("chatStore")]
        public ChatStore ChatStore { get; set; }

        [JsonProperty("commentStore")]
        public CommentStore CommentStore { get; set; }

        [JsonProperty("userCardStore")]
        public UserCardStore UserCardStore { get; set; }
    }

    public partial class ChatSettingStore
    {
        [JsonProperty("bannedWords")]
        public List<object> BannedWords { get; set; }

        [JsonProperty("menuType")]
        public string MenuType { get; set; }

        [JsonProperty("menuVisible")]
        public bool MenuVisible { get; set; }

        [JsonProperty("delayChat")]
        public bool DelayChat { get; set; }

        [JsonProperty("muteFreshUser")]
        public bool MuteFreshUser { get; set; }

        [JsonProperty("muteWarnedUser")]
        public bool MuteWarnedUser { get; set; }

        [JsonProperty("muteForbiddenWord")]
        public bool MuteForbiddenWord { get; set; }

        [JsonProperty("isPremiumHidden")]
        public bool IsPremiumHidden { get; set; }

        [JsonProperty("userColor")]
        public string UserColor { get; set; }

        [JsonProperty("limitedContinuousChat")]
        public bool LimitedContinuousChat { get; set; }

        [JsonProperty("continuousChatThreshold")]
        public long ContinuousChatThreshold { get; set; }

        [JsonProperty("limitedUnfollowerChat")]
        public bool LimitedUnfollowerChat { get; set; }

        [JsonProperty("unfollowerChatThreshold")]
        public long UnfollowerChatThreshold { get; set; }

        [JsonProperty("limitedFreshUserChat")]
        public bool LimitedFreshUserChat { get; set; }

        [JsonProperty("freshUserChatThreshold")]
        public long FreshUserChatThreshold { get; set; }

        [JsonProperty("limitedTemporaryBlacklist")]
        public bool LimitedTemporaryBlacklist { get; set; }

        [JsonProperty("temporaryBlacklistThreshold")]
        public long TemporaryBlacklistThreshold { get; set; }

        [JsonProperty("limitedWarnedUserChat")]
        public bool LimitedWarnedUserChat { get; set; }

        [JsonProperty("hasRenderUnreadChatRule")]
        public bool HasRenderUnreadChatRule { get; set; }

        [JsonProperty("chatRule")]
        public string ChatRule { get; set; }
    }

    public partial class ChatStore
    {
        [JsonProperty("stores")]
        public object Stores { get; set; }

        [JsonProperty("chats")]
        public List<object> Chats { get; set; }

        [JsonProperty("blacklist")]
        public List<object> Blacklist { get; set; }

        [JsonProperty("moderator")]
        public List<object> Moderator { get; set; }

        [JsonProperty("bannedWords")]
        public List<object> BannedWords { get; set; }

        [JsonProperty("chatScrollEnable")]
        public bool ChatScrollEnable { get; set; }

        [JsonProperty("isLastPage")]
        public bool IsLastPage { get; set; }

        [JsonProperty("isPopout")]
        public bool IsPopout { get; set; }

        [JsonProperty("isTracing")]
        public bool IsTracing { get; set; }

        [JsonProperty("yellProducts")]
        public List<object> YellProducts { get; set; }

        [JsonProperty("displayOwnerRule")]
        public bool DisplayOwnerRule { get; set; }

        [JsonProperty("isChatVisible")]
        public bool IsChatVisible { get; set; }

        [JsonProperty("enabledChatInDvr")]
        public bool EnabledChatInDvr { get; set; }

        [JsonProperty("isScrollingByAuto")]
        public bool IsScrollingByAuto { get; set; }

        [JsonProperty("chatQue")]
        public List<object> ChatQue { get; set; }

        [JsonProperty("archiveChatQue")]
        public List<object> ArchiveChatQue { get; set; }

        [JsonProperty("chatLoading")]
        public bool ChatLoading { get; set; }

        [JsonProperty("archiveChatLoading")]
        public bool ArchiveChatLoading { get; set; }

        [JsonProperty("nextArchiveChatExists")]
        public bool NextArchiveChatExists { get; set; }

        [JsonProperty("forcedScrollBottom")]
        public bool ForcedScrollBottom { get; set; }

        [JsonProperty("isScrolledByAuto")]
        public bool IsScrolledByAuto { get; set; }

        [JsonProperty("forcedAutoScrollCount")]
        public long ForcedAutoScrollCount { get; set; }

        [JsonProperty("chatSettingStore")]
        public ChatSettingStore ChatSettingStore { get; set; }
    }

    public partial class CommentStore
    {
        [JsonProperty("stores")]
        public object Stores { get; set; }

        [JsonProperty("comments")]
        public List<object> Comments { get; set; }
    }

    public partial class MovieStore
    {
        [JsonProperty("stores")]
        public object Stores { get; set; }

        [JsonProperty("isCompletedFetchMovie")]
        public bool IsCompletedFetchMovie { get; set; }

        [JsonProperty("isCompletedFetchMovieDetail")]
        public bool IsCompletedFetchMovieDetail { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("movieId")]
        public long MovieId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("introduction")]
        public string Introduction { get; set; }

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("casts")]
        public List<Cast> Casts { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("sSpriteImageUrl")]
        public string SSpriteImageUrl { get; set; }

        [JsonProperty("lSpriteImageUrl")]
        public string LSpriteImageUrl { get; set; }

        [JsonProperty("next")]
        public object Next { get; set; }

        [JsonProperty("totalViews")]
        public long TotalViews { get; set; }

        [JsonProperty("liveViews")]
        public long LiveViews { get; set; }

        [JsonProperty("totalYells")]
        public long TotalYells { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("startedAt")]
        public DateTimeOffset StartedAt { get; set; }

        [JsonProperty("willStartAt")]
        public string WillStartAt { get; set; }

        [JsonProperty("startTime")]
        public long StartTime { get; set; }

        [JsonProperty("playTime")]
        public long PlayTime { get; set; }

        [JsonProperty("notFound")]
        public bool NotFound { get; set; }

        [JsonProperty("monetizeStatus")]
        public long MonetizeStatus { get; set; }

        [JsonProperty("onairStatus")]
        public long OnairStatus { get; set; }

        [JsonProperty("uploadStatus")]
        public string UploadStatus { get; set; }

        [JsonProperty("isLowLatency")]
        public bool IsLowLatency { get; set; }

        [JsonProperty("isUploadedMovie")]
        public bool IsUploadedMovie { get; set; }

        [JsonProperty("isMobile")]
        public bool IsMobile { get; set; }

        [JsonProperty("isDvr")]
        public bool IsDvr { get; set; }

        [JsonProperty("finishedLiveStreaming")]
        public bool FinishedLiveStreaming { get; set; }

        [JsonProperty("enabledAd")]
        public bool EnabledAd { get; set; }

        [JsonProperty("enabledYell")]
        public bool EnabledYell { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("playPostion")]
        public long PlayPostion { get; set; }

        [JsonProperty("orientation")]
        public long Orientation { get; set; }

        [JsonProperty("deviceType")]
        public long DeviceType { get; set; }

        [JsonProperty("movieType")]
        public string MovieType { get; set; }

        [JsonProperty("connectCount")]
        public long ConnectCount { get; set; }

        [JsonProperty("media")]
        public Media Media { get; set; }

        [JsonProperty("game")]
        public Game Game { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }

        [JsonProperty("blacklist")]
        public List<Blacklist> Blacklist { get; set; }

        [JsonProperty("ad")]
        public Ad Ad { get; set; }

        [JsonProperty("viewsLimit")]
        public ViewsLimit ViewsLimit { get; set; }

        [JsonProperty("settings")]
        public Settings Settings { get; set; }
    }

    public partial class Ad
    {
        [JsonProperty("webStream")]
        public string WebStream { get; set; }
    }

    public partial class Blacklist
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }
    }

    public partial class Cast
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("recxuserId")]
        public long RecxuserId { get; set; }

        [JsonProperty("bliveUserId")]
        public long BLiveUserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("userIconImageUrl")]
        public string UserIconImageUrl { get; set; }

        [JsonProperty("isOfficial")]
        public bool IsOfficial { get; set; }

        [JsonProperty("isPremium")]
        public bool IsPremium { get; set; }

        [JsonProperty("isFresh")]
        public bool IsFresh { get; set; }

        [JsonProperty("isWarned")]
        public bool IsWarned { get; set; }

        [JsonProperty("isModerator")]
        public bool IsModerator { get; set; }

        [JsonProperty("userColor")]
        public string UserColor { get; set; }

        [JsonProperty("isPremiumHidden")]
        public bool IsPremiumHidden { get; set; }
    }

    public partial class Channel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("recxuserId")]
        public long RecxuserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("followers")]
        public long Followers { get; set; }

        [JsonProperty("teams")]
        public List<object> Teams { get; set; }

        [JsonProperty("iconImageUrl")]
        public string IconImageUrl { get; set; }

        [JsonProperty("isModerating")]
        public bool IsModerating { get; set; }

        [JsonProperty("isPremium")]
        public bool IsPremium { get; set; }

        [JsonProperty("isOfficial")]
        public bool IsOfficial { get; set; }

        [JsonProperty("isFresh")]
        public bool IsFresh { get; set; }

        [JsonProperty("isWarned")]
        public bool IsWarned { get; set; }
    }

    public partial class Game
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("ceroRating")]
        public long CeroRating { get; set; }
    }

    public partial class Media
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("urlLowLatency")]
        public string UrlLowLatency { get; set; }

        [JsonProperty("urlTrailer")]
        public string UrlTrailer { get; set; }

        [JsonProperty("urlSource")]
        public string UrlSource { get; set; }
    }

    public partial class Settings
    {
        [JsonProperty("limitedContinuousChat")]
        public bool LimitedContinuousChat { get; set; }

        [JsonProperty("continuousChatThreshold")]
        public long ContinuousChatThreshold { get; set; }

        [JsonProperty("limitedUnfollowerChat")]
        public bool LimitedUnfollowerChat { get; set; }

        [JsonProperty("unfollowerChatThreshold")]
        public long UnfollowerChatThreshold { get; set; }

        [JsonProperty("limitedFreshUserChat")]
        public bool LimitedFreshUserChat { get; set; }

        [JsonProperty("freshUserChatThreshold")]
        public long FreshUserChatThreshold { get; set; }

        [JsonProperty("limitedTemporaryBlacklist")]
        public bool LimitedTemporaryBlacklist { get; set; }

        [JsonProperty("temporaryBlacklistThreshold")]
        public long TemporaryBlacklistThreshold { get; set; }

        [JsonProperty("limitedWarnedUserChat")]
        public bool LimitedWarnedUserChat { get; set; }

        [JsonProperty("chatRule")]
        public string ChatRule { get; set; }
    }

    public partial class ViewsLimit
    {
        [JsonProperty("hasPermission")]
        public bool HasPermission { get; set; }

        [JsonProperty("isViewed")]
        public bool IsViewed { get; set; }

        [JsonProperty("remain")]
        public long Remain { get; set; }

        [JsonProperty("restrictionMethod")]
        public string RestrictionMethod { get; set; }

        [JsonProperty("secondsRemaining")]
        public long SecondsRemaining { get; set; }
    }

    public partial class UserCardStore
    {
        [JsonProperty("stores")]
        public object Stores { get; set; }

        [JsonProperty("userCard")]
        public UserCard UserCard { get; set; }
    }

    public partial class UserCard
    {
        [JsonProperty("element")]
        public object Element { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("followed")]
        public bool Followed { get; set; }

        [JsonProperty("blacklisted")]
        public bool Blacklisted { get; set; }

        [JsonProperty("moderated")]
        public bool Moderated { get; set; }

        [JsonProperty("userIconImag\u000d\neUrl")]
        public string UserIconImagEUrl { get; set; }

        [JsonProperty("userCoverImageUrl")]
        public string UserCoverImageUrl { get; set; }
    }

    public partial class YellStore
    {
        [JsonProperty("currentTab")]
        public string CurrentTab { get; set; }

        [JsonProperty("newSuppoter")]
        public NewSuppoter NewSuppoter { get; set; }

        [JsonProperty("newSuppoterQueue")]
        public List<object> NewSuppoterQueue { get; set; }

        [JsonProperty("newYells")]
        public List<object> NewYells { get; set; }

        [JsonProperty("yellRanks")]
        public List<object> YellRanks { get; set; }

        [JsonProperty("yellRanksOfMonthly")]
        public List<object> YellRanksOfMonthly { get; set; }

        [JsonProperty("pageOfNewYells")]
        public long PageOfNewYells { get; set; }

        [JsonProperty("pageOfYellRanks")]
        public long PageOfYellRanks { get; set; }

        [JsonProperty("pageOfMonthlyYellRanks")]
        public long PageOfMonthlyYellRanks { get; set; }

        [JsonProperty("isLastPageOfNewYells")]
        public bool IsLastPageOfNewYells { get; set; }

        [JsonProperty("isLastPageOfYellRanks")]
        public bool IsLastPageOfYellRanks { get; set; }

        [JsonProperty("isLastPageOfMonthlyYellRanks")]
        public bool IsLastPageOfMonthlyYellRanks { get; set; }

        [JsonProperty("isLoading")]
        public bool IsLoading { get; set; }
    }

    public partial class NewSuppoter
    {
        [JsonProperty("currentTime")]
        public long CurrentTime { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("supporter")]
        public object Supporter { get; set; }
    }
}
