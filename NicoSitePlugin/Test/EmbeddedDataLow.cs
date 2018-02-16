using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

/// <summary>
/// http://live2.nicovideo.jp/watch/lv\d+ のdata-propsをデシリアライズするためのクラス群
/// </summary>
namespace NicoSitePlugin.Old.EmbeddedDataLow
{
    public class TrackingParams
    {
        public string siteId { get; set; }
        public string pageId { get; set; }
        public string mode { get; set; }
        public string programStatus { get; set; }
    }

    public class Account
    {
        public string loginPageUrl { get; set; }
        public string logoutPageUrl { get; set; }
        public string accountRegistrationPageUrl { get; set; }
        public string premiumMemberRegistrationPageUrl { get; set; }
        public TrackingParams trackingParams { get; set; }
    }

    public class App
    {
        public string topPageUrl { get; set; }
    }

    public class Atsumaru
    {
        public string topPageUrl { get; set; }
    }

    public class Blomaga
    {
        public string topPageUrl { get; set; }
    }

    public class Channel
    {
        public string topPageUrl { get; set; }
        public string forOrganizationAndCompanyPageUrl { get; set; }
    }

    public class Commons
    {
        public string topPageUrl { get; set; }
    }

    public class Community
    {
        public string topPageUrl { get; set; }
    }

    public class Denfaminicogamer
    {
        public string topPageUrl { get; set; }
    }

    public class Dic
    {
        public string topPageUrl { get; set; }
    }

    public class Help
    {
        public string liveHelpPageUrl { get; set; }
        public string systemRequirementsPageUrl { get; set; }
    }

    public class Ichiba
    {
        public string configBaseUrl { get; set; }
        public string scriptUrl { get; set; }
        public string topPageUrl { get; set; }
    }

    public class Jk
    {
        public string topPageUrl { get; set; }
    }

    public class Kakuyomu
    {
        public string topPageUrl { get; set; }
    }

    public class Mastodon
    {
        public string topPageUrl { get; set; }
    }

    public class News
    {
        public string topPageUrl { get; set; }
    }

    public class Niconare
    {
        public string topPageUrl { get; set; }
    }

    public class Niconico
    {
        public string topPageUrl { get; set; }
    }

    public class Point
    {
        public string topPageUrl { get; set; }
        public string purchasePageUrl { get; set; }
    }

    public class Seiga
    {
        public string topPageUrl { get; set; }
    }

    public class Site2
    {
        public string serviceListPageUrl { get; set; }
        public string salesAdvertisingPageUrl { get; set; }
    }

    public class Solid
    {
        public string topPageUrl { get; set; }
    }

    public class Uad
    {
        public string topPageUrl { get; set; }
    }

    public class Video
    {
        public string topPageUrl { get; set; }
        public string myPageUrl { get; set; }
    }

    public class Bbs
    {
        public string requestPageUrl { get; set; }
    }

    public class RightsControlProgram
    {
        public string pageUrl { get; set; }
    }

    public class LicenseSearch
    {
        public string pageUrl { get; set; }
    }

    public class Info
    {
        public string warnForPhishingPageUrl { get; set; }
        public string smartphoneSdkPageUrl { get; set; }
        public string nintendoGuidelinePageUrl { get; set; }
    }

    public class FamilyService
    {
        public Account account { get; set; }
        public App app { get; set; }
        public Atsumaru atsumaru { get; set; }
        public Blomaga blomaga { get; set; }
        public Channel channel { get; set; }
        public Commons commons { get; set; }
        public Community community { get; set; }
        public Denfaminicogamer denfaminicogamer { get; set; }
        public Dic dic { get; set; }
        public Help help { get; set; }
        public Ichiba ichiba { get; set; }
        public Jk jk { get; set; }
        public Kakuyomu kakuyomu { get; set; }
        public Mastodon mastodon { get; set; }
        public News news { get; set; }
        public Niconare niconare { get; set; }
        public Niconico niconico { get; set; }
        public Point point { get; set; }
        public Seiga seiga { get; set; }
        public Site2 site { get; set; }
        public Solid solid { get; set; }
        public Uad uad { get; set; }
        public Video video { get; set; }
        public Bbs bbs { get; set; }
        public RightsControlProgram rightsControlProgram { get; set; }
        public LicenseSearch licenseSearch { get; set; }
        public Info info { get; set; }
    }

    public class Environments
    {
        public string runningMode { get; set; }
    }

    public class Relive
    {
        public string apiBaseUrl { get; set; }
        public string webSocketUrl { get; set; }
        public string csrfToken { get; set; }
    }

    public class Information
    {
        public string html5PlayerInformationPageUrl { get; set; }
        public string flashPlayerInstallInformationPageUrl { get; set; }
    }

    public class Rule
    {
        public string agreementPageUrl { get; set; }
        public string guidelinePageUrl { get; set; }
    }

    public class Spec
    {
        public string watchUsageAndDevicePageUrl { get; set; }
        public string broadcastUsageDevicePageUrl { get; set; }
    }

    public class Ad
    {
        public string adsApiBaseUrl { get; set; }
    }

    public class Program
    {
        public int liveCount { get; set; }
    }

    public class Tag
    {
        public string suggestionApiUrl { get; set; }
        public int revisionCheckIntervalMs { get; set; }
        public string registerHelpPageUrl { get; set; }
        public int userRegistrableMax { get; set; }
        public int textMaxLength { get; set; }
    }

    public class Coe
    {
        public string resourcesBaseUrl { get; set; }
    }

    public class CommonHeader
    {
        public string siteId { get; set; }
        public string apiKey { get; set; }
        public string apiDate { get; set; }
        public string apiVersion { get; set; }
        public string jsonpUrl { get; set; }
    }

    public class Notify
    {
        public string unreadApiUrl { get; set; }
        public string contentApiUrl { get; set; }
        public int updateUnreadIntervalMs { get; set; }
    }

    public class Timeshift
    {
        public string reservationDetailListApiUrl { get; set; }
    }

    public class NiconicoLiveEncoder
    {
        public string downloadUrl { get; set; }
    }

    public class Broadcast
    {
        public string usageHelpPageUrl { get; set; }
        public string stableBroadcastHelpPageUrl { get; set; }
        public NiconicoLiveEncoder niconicoLiveEncoder { get; set; }
    }

    public class Enquete
    {
        public string usageHelpPageUrl { get; set; }
    }

    public class TrialWatch
    {
        public string usageHelpPageUrl { get; set; }
    }

    public class Site
    {
        public string locale { get; set; }
        public long serverTime { get; set; }
        public string apiBaseUrl { get; set; }
        public string staticResourceBaseUrl { get; set; }
        public string topPageUrl { get; set; }
        public string editstreamPageUrl { get; set; }
        public string historyPageUrl { get; set; }
        public string myPageUrl { get; set; }
        public string rankingPageUrl { get; set; }
        public string searchPageUrl { get; set; }
        public FamilyService familyService { get; set; }
        public Environments environments { get; set; }
        public Relive relive { get; set; }
        public Information information { get; set; }
        public Rule rule { get; set; }
        public Spec spec { get; set; }
        public Ad ad { get; set; }
        public Program program { get; set; }
        public Tag tag { get; set; }
        public Coe coe { get; set; }
        public CommonHeader commonHeader { get; set; }
        public Notify notify { get; set; }
        public Timeshift timeshift { get; set; }
        public Broadcast broadcast { get; set; }
        public Enquete enquete { get; set; }
        public TrialWatch trialWatch { get; set; }
    }

    public class User
    {
        public string id { get; set; }
        public string nickname { get; set; }
        public bool isLoggedIn { get; set; }
        public string accountType { get; set; }
        public bool isOperator { get; set; }
        public bool isBroadcaster { get; set; }
        public string premiumOrigin { get; set; }
        public List<object> permissions { get; set; }
    }

    public class Thumbnail
    {
        public string imageUrl { get; set; }
    }

    public class NicopediaArticle
    {
        public string pageUrl { get; set; }
        public bool exists { get; set; }
    }

    public class Supplier
    {
        public string name { get; set; }
        public string pageUrl { get; set; }
        public NicopediaArticle nicopediaArticle { get; set; }
    }

    public class Substitute
    {
    }

    public class List
    {
        public string text { get; set; }
        public bool existsNicopediaArticle { get; set; }
        public string nicopediaArticlePageUrlPath { get; set; }
        public string type { get; set; }
        public bool isLocked { get; set; }
        public bool isDeletable { get; set; }
    }

    public class Tag2
    {
        public List<List> list { get; set; }
        public string apiUrl { get; set; }
        public string registerApiUrl { get; set; }
        public string deleteApiUrl { get; set; }
        public string apiToken { get; set; }
        public bool isLocked { get; set; }
    }

    public class Links
    {
        public string feedbackPageUrl { get; set; }
        public string commentReportPageUrl { get; set; }
        public string flashPlayerWatchPageUrl { get; set; }
        public string html5PlayerWatchPageUrl { get; set; }
        public string contentsTreePageUrl { get; set; }
        public string programReportPageUrl { get; set; }
        public string tagReportPageUrl { get; set; }
    }

    public class Banner
    {
        public string apiUrl { get; set; }
    }

    public class Player
    {
        public string embedUrl { get; set; }
        public Banner banner { get; set; }
    }

    public class Zapping
    {
        public string listApiUrl { get; set; }
        public int listUpdateIntervalMs { get; set; }
    }

    public class Report
    {
        public string imageApiUrl { get; set; }
    }

    public class Program2
    {
        public string nicoliveProgramId { get; set; }
        public string reliveProgramId { get; set; }
        public string broadcastId { get; set; }
        public string providerType { get; set; }
        public string visualProviderType { get; set; }
        public string title { get; set; }
        public Thumbnail thumbnail { get; set; }
        public Supplier supplier { get; set; }
        public int openTime { get; set; }
        public int beginTime { get; set; }
        public int endTime { get; set; }
        public int scheduledEndTime { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public Substitute substitute { get; set; }
        public Tag2 tag { get; set; }
        public Links links { get; set; }
        public Player player { get; set; }
        public string watchPageUrl { get; set; }
        public string mediaServerType { get; set; }
        public bool isEnabledHtml5Player { get; set; }
        public bool isPrivate { get; set; }
        public Zapping zapping { get; set; }
        public Report report { get; set; }
        public bool isFollowerOnly { get; set; }
    }

    public class SocialGroup
    {
        public string type { get; set; }
        public string id { get; set; }
        public string broadcastHistoryPageUrl { get; set; }
        public string description { get; set; }
        public string name { get; set; }
        public string socialGroupPageUrl { get; set; }
        public string thumbnailImageUrl { get; set; }
        public string thumbnailSmallImageUrl { get; set; }
        public int level { get; set; }
    }

    public class Wall
    {
    }

    public class ProgramEventState
    {
        public bool commentLocked { get; set; }
        public string audienceCommentLayout { get; set; }
    }

    public class Player2
    {
        public string name { get; set; }
        public string audienceToken { get; set; }
        public bool isJumpDisabled { get; set; }
        public bool disablePlayVideoAd { get; set; }
        public bool isRestrictedCommentPost { get; set; }
        public bool enableClientLog { get; set; }
        public ProgramEventState programEventState { get; set; }
        public string streamAllocationType { get; set; }
    }

    public class Ad2
    {
        public bool isSiteHeaderBannerEnabled { get; set; }
        public bool isSideWallEnabled { get; set; }
        public bool isProgramInformationEnabled { get; set; }
        public bool isFooterEnabled { get; set; }
    }

    public class BtnFeedback
    {
        public string png { get; set; }
    }

    public class Favicon
    {
        public string ico { get; set; }
    }

    public class FooterArrow
    {
        public string png { get; set; }
    }

    public class Glass
    {
        public string png { get; set; }
    }

    public class FacebookIcon
    {
        public string svg { get; set; }
    }

    public class FollowCheck
    {
        public string svg { get; set; }
    }

    public class FollowCheckWhite
    {
        public string svg { get; set; }
    }

    public class FollowWhite
    {
        public string svg { get; set; }
    }

    public class LineIcon
    {
        public string svg { get; set; }
    }

    public class SharesIcon
    {
        public string svg { get; set; }
    }

    public class TwitterIcon
    {
        public string svg { get; set; }
    }

    public class Icon
    {
        public FacebookIcon facebook_icon { get; set; }
        public FollowCheck follow_check { get; set; }
        public FollowCheckWhite follow_check_white { get; set; }
        public FollowWhite follow_white { get; set; }
        public LineIcon line_icon { get; set; }
        public SharesIcon shares_icon { get; set; }
        public TwitterIcon twitter_icon { get; set; }
    }

    public class LineButton
    {
        public string png { get; set; }
    }

    public class Logo
    {
        public string png { get; set; }
        public string svg { get; set; }
    }

    public class NotificationBarIcon
    {
        public string png { get; set; }
    }

    public class PagetopArrow
    {
        public string png { get; set; }
    }

    public class SnsSprite
    {
        public string png { get; set; }
    }

    public class Base
    {
        public BtnFeedback btn_feedback { get; set; }
        public Favicon favicon { get; set; }
        public FooterArrow footer_arrow { get; set; }
        public Glass glass { get; set; }
        public Icon icon { get; set; }
        public LineButton line_button { get; set; }
        public Logo logo { get; set; }
        [JsonProperty("notification-bar-icon")]
        public NotificationBarIcon notification_bar_icon { get; set; }
        public PagetopArrow pagetop_arrow { get; set; }
        public SnsSprite sns_sprite { get; set; }
    }

    public class Arrows
    {
        public string png { get; set; }
    }

    public class TagIcons
    {
        public string png { get; set; }
    }

    public class Module
    {
        public Arrows arrows { get; set; }
        public TagIcons tag_icons { get; set; }
    }

    public class Background
    {
        public string jpg { get; set; }
    }

    public class FormMeterCover
    {
        public string svg { get; set; }
    }

    public class FormSelectArrow
    {
        public string svg { get; set; }
    }

    public class IconVolumeMic
    {
        public string svg { get; set; }
    }

    public class NleBnDownload
    {
        public string png { get; set; }
    }

    public class NleBnStarting
    {
        public string png { get; set; }
    }

    public class ProgramProvider
    {
        public Background background { get; set; }
        public FormMeterCover form_meter_cover { get; set; }
        public FormSelectArrow form_select_arrow { get; set; }
        public IconVolumeMic icon_volume_mic { get; set; }
        public NleBnDownload nle_bn_download { get; set; }
        public NleBnStarting nle_bn_starting { get; set; }
    }

    public class CountryRestricted
    {
        public string png { get; set; }
    }

    public class Resizable
    {
        public CountryRestricted country_restricted { get; set; }
    }

    public class SelectArrow
    {
        public string png { get; set; }
    }

    public class TagReport
    {
        public SelectArrow select_arrow { get; set; }
    }

    public class MailIcon
    {
        public string png { get; set; }
    }

    public class AudienceMail
    {
        public MailIcon mail_icon { get; set; }
    }

    public class BnrBroadcastSettingNleDownload
    {
        public string png { get; set; }
    }

    public class BnrBroadcastSettingNleStartup
    {
        public string png { get; set; }
    }

    public class BnrPremiumPlayerSp
    {
        public string gif { get; set; }
    }

    public class Banner2
    {
        [JsonProperty("bnr_broadcast-setting-nle-download")]
        public BnrBroadcastSettingNleDownload bnr_broadcast_setting_nle_download { get; set; }
        [JsonProperty("bnr_broadcast-setting-nle-startup")]
        public BnrBroadcastSettingNleStartup bnr_broadcast_setting_nle_startup { get; set; }
        public BnrPremiumPlayerSp bnr_premium_player_sp { get; set; }
    }

    public class BourbonBackground
    {
        public string png { get; set; }
    }

    public class Bourbon
    {
        public BourbonBackground bourbon_background { get; set; }
    }

    public class IconBan
    {
        public string png { get; set; }
    }

    public class CommentBan
    {
        public IconBan icon_ban { get; set; }
    }

    public class CreatorBtnIcon
    {
        public string png { get; set; }
    }

    public class Creator
    {
        public CreatorBtnIcon creator_btn_icon { get; set; }
    }

    public class BgBlomaga
    {
        public string png { get; set; }
    }

    public class BgBlomagaArticle
    {
        public string png { get; set; }
    }

    public class BgBlomagaArticleNP
    {
        public string png { get; set; }
    }

    public class BgBlomagaNP
    {
        public string png { get; set; }
    }

    public class BgSubmit
    {
        public string png { get; set; }
    }

    public class BpnFkdV2
    {
        public string png { get; set; }
    }

    public class BpnNoimageBig
    {
        public string jpg { get; set; }
    }

    public class BpnNoimageSml
    {
        public string jpg { get; set; }
    }

    public class BpnRatingBig
    {
        public string jpg { get; set; }
    }

    public class BpnRatingSml
    {
        public string jpg { get; set; }
    }

    public class BtnGoIchiba
    {
        public string png { get; set; }
    }

    public class IconMat
    {
        public string png { get; set; }
    }

    public class IconPia
    {
        public string png { get; set; }
    }

    public class BpnTab0
    {
        public string png { get; set; }
    }

    public class BpnTab1
    {
        public string png { get; set; }
    }

    public class BpnTabBg
    {
        public string png { get; set; }
    }

    public class Search
    {
        public BpnTab0 bpn_tab_0 { get; set; }
        public BpnTab1 bpn_tab_1 { get; set; }
        public BpnTabBg bpn_tab_bg { get; set; }
    }

    public class Ichiba2
    {
        public BgBlomaga bgBlomaga { get; set; }
        public BgBlomagaArticle bgBlomagaArticle { get; set; }
        public BgBlomagaArticleNP bgBlomagaArticleNP { get; set; }
        public BgBlomagaNP bgBlomagaNP { get; set; }
        public BgSubmit bg_submit { get; set; }
        public BpnFkdV2 bpn_fkd_v2 { get; set; }
        public BpnNoimageBig bpn_noimage_big { get; set; }
        public BpnNoimageSml bpn_noimage_sml { get; set; }
        public BpnRatingBig bpn_rating_big { get; set; }
        public BpnRatingSml bpn_rating_sml { get; set; }
        public BtnGoIchiba btn_go_ichiba { get; set; }
        public IconMat icon_mat { get; set; }
        public IconPia icon_pia { get; set; }
        public Search search { get; set; }
    }

    public class OtayoriSplite
    {
        public string png { get; set; }
    }

    public class Otayori
    {
        public OtayoriSplite otayori_splite { get; set; }
    }

    public class AdobeFlashPlayer
    {
        public string svg { get; set; }
    }

    public class Html5
    {
        public string svg { get; set; }
    }

    public class TvchanFillWhite
    {
        public string svg { get; set; }
    }

    public class Svg
    {
        public AdobeFlashPlayer Adobe_Flash_Player { get; set; }
        public Html5 html5 { get; set; }
        public TvchanFillWhite tvchan_fill_white { get; set; }
    }

    public class ExistNicopedia
    {
        public string svg { get; set; }
    }

    public class NonNicopedia
    {
        public string svg { get; set; }
    }

    public class TagSprite
    {
        public string png { get; set; }
    }

    public class Tag3
    {
        public ExistNicopedia exist_nicopedia { get; set; }
        public NonNicopedia non_nicopedia { get; set; }
        public TagSprite tag_sprite { get; set; }
    }

    public class ProviderIconSprite
    {
        public string png { get; set; }
    }

    public class SpriteColor
    {
        public string png { get; set; }
    }

    public class Title
    {
        public ProviderIconSprite provider_icon_sprite { get; set; }
        public SpriteColor sprite_color { get; set; }
    }

    public class ZappingSprite
    {
        public string png { get; set; }
    }

    public class Zapping2
    {
        public ZappingSprite zapping_sprite { get; set; }
    }

    public class Watch
    {
        public AudienceMail audience_mail { get; set; }
        public Banner2 banner { get; set; }
        public Bourbon bourbon { get; set; }
        public CommentBan comment_ban { get; set; }
        public Creator creator { get; set; }
        public Ichiba2 ichiba_2 { get; set; }
        public Otayori otayori { get; set; }
        public Svg svg { get; set; }
        public Tag3 tag { get; set; }
        public Title title { get; set; }
        public Zapping2 zapping { get; set; }
    }

    public class Common
    {
        public Base @base { get; set; }
        public Module module { get; set; }
        public ProgramProvider program_provider { get; set; }
        public Resizable resizable { get; set; }
        public TagReport tag_report { get; set; }
        public Watch watch { get; set; }
    }

    public class Colorbars
    {
        public string svg { get; set; }
    }

    public class Nicoex
    {
        public Colorbars colorbars { get; set; }
    }

    public class Images
    {
        public Common common { get; set; }
        public Nicoex nicoex { get; set; }
    }

    public class Scripts
    {
        [JsonProperty("pc-watch")]
        public string pc_watch { get; set; }
        [JsonProperty("operator-tools")]
        public string operator_tools { get; set; }
        [JsonProperty("pc-watch.all")]
        public string pc_watch_all { get; set; }
        public string common { get; set; }
        public string polyfill { get; set; }
        public string nicoheader { get; set; }
        public string ichiba { get; set; }
    }

    public class Stylesheets
    {
        [JsonProperty("pc-watch.all")]
        public string pc_watch_all { get; set; }
    }

    public class AriesBroadcaster
    {
        public string swf { get; set; }
    }

    public class AriesPlayer2
    {
        public string swf { get; set; }
    }

    public class HDSModule
    {
        public string swf { get; set; }
    }

    public class MessageserverConnector
    {
        public string swf { get; set; }
    }

    public class ResizablePlayer
    {
        public string swf { get; set; }
    }

    public class AriesPlayer
    {
        public AriesBroadcaster AriesBroadcaster { get; set; }
        [JsonProperty("AriesPlayer")]
        public AriesPlayer2 AriesPlayer2 { get; set; }
        public HDSModule HDSModule { get; set; }
        public MessageserverConnector MessageserverConnector { get; set; }
        public ResizablePlayer ResizablePlayer { get; set; }
    }

    public class RtmpStreamPlayer
    {
        public string swf { get; set; }
    }

    public class ExternalPlayer
    {
        public RtmpStreamPlayer RtmpStreamPlayer { get; set; }
    }

    public class Common2
    {
        [JsonProperty("aries-player")]
        public AriesPlayer aries_player { get; set; }
        [JsonProperty("external-player")]
        public ExternalPlayer external_player { get; set; }
    }

    public class Swfs
    {
        public Common2 common { get; set; }
    }

    public class Eventemitter2
    {
        public string js { get; set; }
    }

    public class Eventemitter
    {
        public Eventemitter2 eventemitter { get; set; }
    }

    public class FacebookSdk
    {
        public string js { get; set; }
    }

    public class Hls
    {
        public string js { get; set; }
    }

    public class Hlsjs
    {
        public Hls hls { get; set; }
    }

    public class Ichiba22
    {
        public string css { get; set; }
    }

    public class IchibaVanilla
    {
        public string js { get; set; }
    }

    public class BgBlomaga2
    {
        public string png { get; set; }
    }

    public class BgBlomagaArticle2
    {
        public string png { get; set; }
    }

    public class BgBlomagaArticleNP2
    {
        public string png { get; set; }
    }

    public class BgBlomagaNP2
    {
        public string png { get; set; }
    }

    public class BgSubmit2
    {
        public string png { get; set; }
    }

    public class BpnFkdV22
    {
        public string png { get; set; }
    }

    public class BpnNoimageBig2
    {
        public string jpg { get; set; }
    }

    public class BpnNoimageSml2
    {
        public string jpg { get; set; }
    }

    public class BpnRatingBig2
    {
        public string jpg { get; set; }
    }

    public class BpnRatingSml2
    {
        public string jpg { get; set; }
    }

    public class BtnGoIchiba2
    {
        public string png { get; set; }
    }

    public class IconMat2
    {
        public string png { get; set; }
    }

    public class IconPia2
    {
        public string png { get; set; }
    }

    public class BpnTab02
    {
        public string png { get; set; }
    }

    public class BpnTab12
    {
        public string png { get; set; }
    }

    public class BpnTabBg2
    {
        public string png { get; set; }
    }

    public class Search2
    {
        public BpnTab02 bpn_tab_0 { get; set; }
        public BpnTab12 bpn_tab_1 { get; set; }
        public BpnTabBg2 bpn_tab_bg { get; set; }
    }

    public class Img
    {
        public BgBlomaga2 bgBlomaga { get; set; }
        public BgBlomagaArticle2 bgBlomagaArticle { get; set; }
        public BgBlomagaArticleNP2 bgBlomagaArticleNP { get; set; }
        public BgBlomagaNP2 bgBlomagaNP { get; set; }
        public BgSubmit2 bg_submit { get; set; }
        public BpnFkdV22 bpn_fkd_v2 { get; set; }
        public BpnNoimageBig2 bpn_noimage_big { get; set; }
        public BpnNoimageSml2 bpn_noimage_sml { get; set; }
        public BpnRatingBig2 bpn_rating_big { get; set; }
        public BpnRatingSml2 bpn_rating_sml { get; set; }
        public BtnGoIchiba2 btn_go_ichiba { get; set; }
        public IconMat2 icon_mat { get; set; }
        public IconPia2 icon_pia { get; set; }
        public Search2 search { get; set; }
    }

    public class Ichiba3
    {
        public Ichiba22 ichiba_2 { get; set; }
        public IchibaVanilla ichiba_vanilla { get; set; }
        public Img img { get; set; }
    }

    public class InputModeChecker
    {
        public string js { get; set; }
    }

    public class LatolatinBold
    {
        public string woff { get; set; }
        public string woff2 { get; set; }
        public string ttf { get; set; }
    }

    public class LatolatinRegular
    {
        public string woff { get; set; }
        public string woff2 { get; set; }
        public string ttf { get; set; }
    }

    public class Lato
    {
        [JsonProperty("latolatin-bold")]
        public LatolatinBold latolatin_bold { get; set; }
        [JsonProperty("latolatin-regular")]
        public LatolatinRegular latolatin_regular { get; set; }
    }

    public class CommunityProgramEndedGuide
    {
        public string png { get; set; }
    }

    public class Fullscreen
    {
        public string css { get; set; }
        public string js { get; set; }
    }

    public class Leo
    {
        public CommunityProgramEndedGuide community_program_ended_guide { get; set; }
        public Fullscreen fullscreen { get; set; }
    }

    public class Lodash2
    {
        public string js { get; set; }
    }

    public class Lodash
    {
        public Lodash2 lodash { get; set; }
    }

    public class NicolibCommonNotificationHeader
    {
        public string js { get; set; }
    }

    public class Arrow
    {
        public string png { get; set; }
    }

    public class BgNotification
    {
        public string png { get; set; }
    }

    public class BtnSwitch
    {
        public string png { get; set; }
    }

    public class IconNiconico
    {
        public string png { get; set; }
    }

    public class NicolibCommonNotificationHeader2
    {
        public string css { get; set; }
    }

    public class NicoliveModified
    {
        public string css { get; set; }
    }

    public class NicopoAdd
    {
        public string png { get; set; }
    }

    public class SiteHeader
    {
        public string css { get; set; }
    }

    public class SiteheaderOptionbutton
    {
        public string png { get; set; }
    }

    public class Tooltip
    {
        public string png { get; set; }
    }

    public class Resources
    {
        public Arrow arrow { get; set; }
        public BgNotification bg_notification { get; set; }
        public BtnSwitch btn_switch { get; set; }
        public IconNiconico icon_niconico { get; set; }
        [JsonProperty("nicolib-CommonNotificationHeader")]
        public NicolibCommonNotificationHeader2 nicolib_CommonNotificationHeader { get; set; }
        [JsonProperty("nicolive-modified")]
        public NicoliveModified nicolive_modified { get; set; }
        public NicopoAdd nicopo_add { get; set; }
        public SiteHeader siteHeader { get; set; }
        [JsonProperty("siteheader-optionbutton")]
        public SiteheaderOptionbutton siteheader_optionbutton { get; set; }
        public Tooltip tooltip { get; set; }
    }

    public class SiteHeader2
    {
        public string js { get; set; }
    }

    public class Nicoheader
    {
        [JsonProperty("nicolib-CommonNotificationHeader")]
        public NicolibCommonNotificationHeader nicolib_CommonNotificationHeader { get; set; }
        public Resources resources { get; set; }
        public SiteHeader2 siteHeader { get; set; }
    }

    public class Placeholders
    {
        public string js { get; set; }
    }

    public class BrowserPolyfill
    {
        public string js { get; set; }
    }

    public class Es5Shim
    {
        public string js { get; set; }
    }

    public class Es6Promise
    {
        public string js { get; set; }
    }

    public class Html5shiv
    {
        public string js { get; set; }
    }

    public class Polyfill
    {
        [JsonProperty("browser-polyfill")]
        public BrowserPolyfill browser_polyfill { get; set; }
        [JsonProperty("es5-shim")]
        public Es5Shim es5_shim { get; set; }
        [JsonProperty("es6-promise")]
        public Es6Promise es6_promise { get; set; }
        public Html5shiv html5shiv { get; set; }
    }

    public class ReactDom
    {
        public string js { get; set; }
    }

    public class ReactWithAddons
    {
        public string js { get; set; }
    }

    public class React
    {
        [JsonProperty("react-dom")]
        public ReactDom react_dom { get; set; }
        [JsonProperty("react-with-addons")]
        public ReactWithAddons react_with_addons { get; set; }
    }

    public class ReactDom2
    {
        public string js { get; set; }
    }

    public class React2
    {
        public string js { get; set; }
    }

    public class React16
    {
        [JsonProperty("react-dom")]
        public ReactDom2 react_dom { get; set; }
        public React2 react { get; set; }
    }

    public class Statuses2
    {
        public string js { get; set; }
    }

    public class Statuses
    {
        public Statuses2 statuses { get; set; }
    }

    public class Swfobject2
    {
        public string js { get; set; }
    }

    public class Swfobject
    {
        public Swfobject2 swfobject { get; set; }
    }

    public class Common3
    {
        public Eventemitter eventemitter { get; set; }
        public FacebookSdk facebook_sdk { get; set; }
        public Hlsjs hlsjs { get; set; }
        public Ichiba3 ichiba { get; set; }
        [JsonProperty("input-mode-checker")]
        public InputModeChecker input_mode_checker { get; set; }
        public Lato lato { get; set; }
        public Leo leo { get; set; }
        public Lodash lodash { get; set; }
        public Nicoheader nicoheader { get; set; }
        public Placeholders placeholders { get; set; }
        public Polyfill polyfill { get; set; }
        public React react { get; set; }
        public React16 react16 { get; set; }
        public Statuses statuses { get; set; }
        public Swfobject swfobject { get; set; }
    }

    public class Vendor
    {
        public Common3 common { get; set; }
    }

    public class Assets
    {
        public Images images { get; set; }
        public Scripts scripts { get; set; }
        public Stylesheets stylesheets { get; set; }
        public Swfs swfs { get; set; }
        public Vendor vendor { get; set; }
    }

    public class NicoEnquete
    {
        public bool isEnabled { get; set; }
    }

    public class Ichiba4
    {
        public bool isEnabled { get; set; }
    }

    public class Community2
    {
        public string id { get; set; }
        public string followPageUrl { get; set; }
        public string unfollowPageUrl { get; set; }
    }

    public class Record
    {
        public string communityId { get; set; }
        public string userId { get; set; }
    }

    public class CommunityFollower
    {
        public List<Record> records { get; set; }
    }

    public class RootObject
    {
        public Site site { get; set; }
        public User user { get; set; }
        public Program2 program { get; set; }
        public SocialGroup socialGroup { get; set; }
        public Wall wall { get; set; }
        public Player2 player { get; set; }
        public Ad2 ad { get; set; }
        public Assets assets { get; set; }
        public NicoEnquete nicoEnquete { get; set; }
        public Ichiba4 ichiba { get; set; }
        public Community2 community { get; set; }
        public CommunityFollower communityFollower { get; set; }
    }
}
