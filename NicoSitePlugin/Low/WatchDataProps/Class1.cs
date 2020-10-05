using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicoSitePlugin.Low.WatchDataProps
{
    public partial class RootObject
    {
        [JsonProperty("site")]
        public RootObjectSite Site { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }

        [JsonProperty("program")]
        public RootObjectProgram Program { get; set; }

        [JsonProperty("socialGroup")]
        public SocialGroup SocialGroup { get; set; }

        [JsonProperty("player")]
        public RootObjectPlayer Player { get; set; }

        [JsonProperty("ad")]
        public RootObjectAd Ad { get; set; }

        [JsonProperty("billboard")]
        public Billboard Billboard { get; set; }

        [JsonProperty("assets")]
        public Assets Assets { get; set; }

        [JsonProperty("nicoEnquete")]
        public NicoEnqueteClass NicoEnquete { get; set; }

        [JsonProperty("ichiba")]
        public NicoEnqueteClass Ichiba { get; set; }

        [JsonProperty("community")]
        public Community Community { get; set; }

        [JsonProperty("communityFollower")]
        public CommunityFollower CommunityFollower { get; set; }

        [JsonProperty("googleAnalytics")]
        public GoogleAnalytics GoogleAnalytics { get; set; }
    }

    public partial class RootObjectAd
    {
        [JsonProperty("isSiteHeaderBannerEnabled")]
        public bool IsSiteHeaderBannerEnabled { get; set; }

        [JsonProperty("isProgramInformationEnabled")]
        public bool IsProgramInformationEnabled { get; set; }

        [JsonProperty("isFooterEnabled")]
        public bool IsFooterEnabled { get; set; }

        [JsonProperty("isBillboardEnabled")]
        public bool IsBillboardEnabled { get; set; }

        [JsonProperty("adsJsUrl")]
        public Uri AdsJsUrl { get; set; }
    }

    public partial class Assets
    {
        [JsonProperty("images")]
        public Images Images { get; set; }

        [JsonProperty("scripts")]
        public Scripts Scripts { get; set; }

        [JsonProperty("stylesheets")]
        public Stylesheets Stylesheets { get; set; }

        [JsonProperty("swfs")]
        public Swfs Swfs { get; set; }

        [JsonProperty("vendor")]
        public Vendor Vendor { get; set; }
    }

    public partial class Images
    {
        [JsonProperty("common")]
        public ImagesCommon Common { get; set; }

        [JsonProperty("leo-player-vc")]
        public LeoPlayerVc LeoPlayerVc { get; set; }

        [JsonProperty("nicoex")]
        public ImagesNicoex Nicoex { get; set; }

        [JsonProperty("nicolive-spweb")]
        public NicoliveSpweb NicoliveSpweb { get; set; }
    }

    public partial class ImagesCommon
    {
        [JsonProperty("base")]
        public Base Base { get; set; }

        [JsonProperty("module")]
        public Module Module { get; set; }

        [JsonProperty("program_provider")]
        public ProgramProvider ProgramProvider { get; set; }

        [JsonProperty("resizable")]
        public Resizable Resizable { get; set; }

        [JsonProperty("tag_report")]
        public TagReport TagReport { get; set; }

        [JsonProperty("watch")]
        public Watch Watch { get; set; }
    }

    public partial class Base
    {
        [JsonProperty("btn_feedback")]
        public TartuGecko BtnFeedback { get; set; }

        [JsonProperty("default-thumbnail")]
        public Colorbars DefaultThumbnail { get; set; }

        [JsonProperty("favicon")]
        public Favicon Favicon { get; set; }

        [JsonProperty("footer_arrow")]
        public TartuGecko FooterArrow { get; set; }

        [JsonProperty("glass")]
        public TartuGecko Glass { get; set; }

        [JsonProperty("icon")]
        public BaseIcon Icon { get; set; }

        [JsonProperty("line_button")]
        public TartuGecko LineButton { get; set; }

        [JsonProperty("logo")]
        public Logo Logo { get; set; }

        [JsonProperty("notification-bar-icon")]
        public TartuGecko NotificationBarIcon { get; set; }

        [JsonProperty("pagetop_arrow")]
        public TartuGecko PagetopArrow { get; set; }

        [JsonProperty("sns_sprite")]
        public TartuGecko SnsSprite { get; set; }
    }

    public partial class TartuGecko
    {
        [JsonProperty("png")]
        public string Png { get; set; }
    }

    public partial class Colorbars
    {
        [JsonProperty("svg")]
        public string Svg { get; set; }
    }

    public partial class Favicon
    {
        [JsonProperty("ico")]
        public string Ico { get; set; }
    }

    public partial class BaseIcon
    {
        [JsonProperty("facebook_icon")]
        public Colorbars FacebookIcon { get; set; }

        [JsonProperty("follow_check")]
        public Colorbars FollowCheck { get; set; }

        [JsonProperty("follow_check_white")]
        public Colorbars FollowCheckWhite { get; set; }

        [JsonProperty("follow_white")]
        public Colorbars FollowWhite { get; set; }

        [JsonProperty("line_icon")]
        public Colorbars LineIcon { get; set; }

        [JsonProperty("shares_icon")]
        public Colorbars SharesIcon { get; set; }

        [JsonProperty("twitter_icon")]
        public Colorbars TwitterIcon { get; set; }
    }

    public partial class Logo
    {
        [JsonProperty("png")]
        public string Png { get; set; }

        [JsonProperty("svg")]
        public string Svg { get; set; }
    }

    public partial class Module
    {
        [JsonProperty("arrows")]
        public TartuGecko Arrows { get; set; }

        [JsonProperty("tag_icons")]
        public TartuGecko TagIcons { get; set; }
    }

    public partial class ProgramProvider
    {
        [JsonProperty("background")]
        public Background Background { get; set; }

        [JsonProperty("form_meter_cover")]
        public Colorbars FormMeterCover { get; set; }

        [JsonProperty("form_select_arrow")]
        public Colorbars FormSelectArrow { get; set; }

        [JsonProperty("icon_volume_mic")]
        public Colorbars IconVolumeMic { get; set; }

        [JsonProperty("nle_bn_download")]
        public TartuGecko NleBnDownload { get; set; }

        [JsonProperty("nle_bn_starting")]
        public TartuGecko NleBnStarting { get; set; }
    }

    public partial class Background
    {
        [JsonProperty("jpg")]
        public string Jpg { get; set; }
    }

    public partial class Resizable
    {
        [JsonProperty("country_restricted")]
        public TartuGecko CountryRestricted { get; set; }
    }

    public partial class TagReport
    {
        [JsonProperty("select_arrow")]
        public TartuGecko SelectArrow { get; set; }
    }

    public partial class Watch
    {
        [JsonProperty("audience_mail")]
        public AudienceMail AudienceMail { get; set; }

        [JsonProperty("banner")]
        public WatchBanner Banner { get; set; }

        [JsonProperty("bourbon")]
        public Bourbon Bourbon { get; set; }

        [JsonProperty("comment_ban")]
        public CommentBan CommentBan { get; set; }

        [JsonProperty("creator")]
        public Creator Creator { get; set; }

        [JsonProperty("ichiba_2")]
        public ImgClass Ichiba2 { get; set; }

        [JsonProperty("otayori")]
        public Otayori Otayori { get; set; }

        [JsonProperty("svg")]
        public Svg Svg { get; set; }

        [JsonProperty("tag")]
        public WatchTag Tag { get; set; }

        [JsonProperty("title")]
        public Title Title { get; set; }

        [JsonProperty("zapping")]
        public WatchZapping Zapping { get; set; }
    }

    public partial class AudienceMail
    {
        [JsonProperty("mail_icon")]
        public TartuGecko MailIcon { get; set; }
    }

    public partial class WatchBanner
    {
        [JsonProperty("bnr_premium_player_sp")]
        public IndicateAnimation BnrPremiumPlayerSp { get; set; }

        [JsonProperty("nair-download-newlabel")]
        public TartuGecko NairDownloadNewlabel { get; set; }

        [JsonProperty("nair-download")]
        public TartuGecko NairDownload { get; set; }

        [JsonProperty("nle-download")]
        public TartuGecko NleDownload { get; set; }
    }

    public partial class IndicateAnimation
    {
        [JsonProperty("gif")]
        public string Gif { get; set; }
    }

    public partial class Bourbon
    {
        [JsonProperty("bourbon_background")]
        public TartuGecko BourbonBackground { get; set; }
    }

    public partial class CommentBan
    {
        [JsonProperty("icon_ban")]
        public TartuGecko IconBan { get; set; }
    }

    public partial class Creator
    {
        [JsonProperty("creator_btn_icon")]
        public TartuGecko CreatorBtnIcon { get; set; }
    }

    public partial class ImgClass
    {
        [JsonProperty("bgBlomaga")]
        public TartuGecko BgBlomaga { get; set; }

        [JsonProperty("bgBlomagaArticle")]
        public TartuGecko BgBlomagaArticle { get; set; }

        [JsonProperty("bgBlomagaArticleNP")]
        public TartuGecko BgBlomagaArticleNp { get; set; }

        [JsonProperty("bgBlomagaNP")]
        public TartuGecko BgBlomagaNp { get; set; }

        [JsonProperty("bg_submit")]
        public TartuGecko BgSubmit { get; set; }

        [JsonProperty("bpn_fkd_v2")]
        public TartuGecko BpnFkdV2 { get; set; }

        [JsonProperty("bpn_noimage_big")]
        public Background BpnNoimageBig { get; set; }

        [JsonProperty("bpn_noimage_sml")]
        public Background BpnNoimageSml { get; set; }

        [JsonProperty("bpn_rating_big")]
        public Background BpnRatingBig { get; set; }

        [JsonProperty("bpn_rating_sml")]
        public Background BpnRatingSml { get; set; }

        [JsonProperty("btn_go_ichiba")]
        public TartuGecko BtnGoIchiba { get; set; }

        [JsonProperty("icon_mat")]
        public TartuGecko IconMat { get; set; }

        [JsonProperty("icon_pia")]
        public TartuGecko IconPia { get; set; }

        [JsonProperty("search")]
        public Ichiba2_Search Search { get; set; }
    }

    public partial class Ichiba2_Search
    {
        [JsonProperty("bpn_tab_0")]
        public TartuGecko BpnTab0 { get; set; }

        [JsonProperty("bpn_tab_1")]
        public TartuGecko BpnTab1 { get; set; }

        [JsonProperty("bpn_tab_bg")]
        public TartuGecko BpnTabBg { get; set; }
    }

    public partial class Otayori
    {
        [JsonProperty("otayori_splite")]
        public TartuGecko OtayoriSplite { get; set; }
    }

    public partial class Svg
    {
        [JsonProperty("Adobe_Flash_Player")]
        public Colorbars AdobeFlashPlayer { get; set; }

        [JsonProperty("html5")]
        public Colorbars Html5 { get; set; }

        [JsonProperty("tvchan_fill_white")]
        public Colorbars TvchanFillWhite { get; set; }
    }

    public partial class WatchTag
    {
        [JsonProperty("exist_nicopedia")]
        public Colorbars ExistNicopedia { get; set; }

        [JsonProperty("non_nicopedia")]
        public Colorbars NonNicopedia { get; set; }

        [JsonProperty("tag_sprite")]
        public TartuGecko TagSprite { get; set; }
    }

    public partial class Title
    {
        [JsonProperty("provider_icon_sprite")]
        public TartuGecko ProviderIconSprite { get; set; }

        [JsonProperty("sprite_color")]
        public TartuGecko SpriteColor { get; set; }
    }

    public partial class WatchZapping
    {
        [JsonProperty("zapping_sprite")]
        public TartuGecko ZappingSprite { get; set; }
    }

    public partial class LeoPlayerVc
    {
        [JsonProperty("indicate_animation")]
        public IndicateAnimation IndicateAnimation { get; set; }
    }

    public partial class ImagesNicoex
    {
        [JsonProperty("bnr_app-coming-soon")]
        public TartuGecko BnrAppComingSoon { get; set; }

        [JsonProperty("bnr_broadcast-setting-nair-download")]
        public TartuGecko BnrBroadcastSettingNairDownload { get; set; }

        [JsonProperty("bnr_filler-app-promotion")]
        public TartuGecko BnrFillerAppPromotion { get; set; }

        [JsonProperty("colorbars")]
        public Colorbars Colorbars { get; set; }

        [JsonProperty("ichiba-pushed-outside-baloon")]
        public Colorbars IchibaPushedOutsideBaloon { get; set; }

        [JsonProperty("ichiba_counter-smoke_effect")]
        public TartuGecko IchibaCounterSmokeEffect { get; set; }

        [JsonProperty("ichiba_launcher_icon_error")]
        public TartuGecko IchibaLauncherIconError { get; set; }

        [JsonProperty("ichiba_launcher_icon_gift")]
        public TartuGecko IchibaLauncherIconGift { get; set; }

        [JsonProperty("ichiba_launcher_icon_nicoad")]
        public TartuGecko IchibaLauncherIconNicoad { get; set; }

        [JsonProperty("inform-broadcaster-stream-screenshot")]
        public TartuGecko InformBroadcasterStreamScreenshot { get; set; }
    }

    public partial class NicoliveSpweb
    {
        [JsonProperty("icon")]
        public NicoliveSpwebIcon Icon { get; set; }

        [JsonProperty("nicocas-android-app-download-banner")]
        public TartuGecko NicocasAndroidAppDownloadBanner { get; set; }

        [JsonProperty("nicocas-ios-app-download-banner")]
        public TartuGecko NicocasIosAppDownloadBanner { get; set; }

        [JsonProperty("pickup-antenachan")]
        public TartuGecko PickupAntenachan { get; set; }

        [JsonProperty("pickup-denpakun-antenachan")]
        public TartuGecko PickupDenpakunAntenachan { get; set; }

        [JsonProperty("pickup-denpakun")]
        public TartuGecko PickupDenpakun { get; set; }
    }

    public partial class NicoliveSpwebIcon
    {
        [JsonProperty("window-icon")]
        public Colorbars WindowIcon { get; set; }
    }

    public partial class Scripts
    {
        [JsonProperty("UseradLogo")]
        public string UseradLogo { get; set; }

        [JsonProperty("IchibaLogo")]
        public string IchibaLogo { get; set; }

        [JsonProperty("EarthquakeLogo")]
        public string EarthquakeLogo { get; set; }

        [JsonProperty("CruiseLogo")]
        public string CruiseLogo { get; set; }

        [JsonProperty("nicolib")]
        public string Nicolib { get; set; }

        [JsonProperty("pc-watch")]
        public string PcWatch { get; set; }

        [JsonProperty("operator-tools")]
        public string OperatorTools { get; set; }

        [JsonProperty("broadcaster-tool")]
        public string BroadcasterTool { get; set; }

        [JsonProperty("pc-watch.all")]
        public string PcWatchAll { get; set; }

        [JsonProperty("vendor")]
        public string Vendor { get; set; }

        [JsonProperty("polyfill")]
        public string Polyfill { get; set; }

        [JsonProperty("nicoheader")]
        public string Nicoheader { get; set; }

        [JsonProperty("ichiba")]
        public string Ichiba { get; set; }
    }

    public partial class Stylesheets
    {
        [JsonProperty("pc-watch.all")]
        public string PcWatchAll { get; set; }
    }

    public partial class Swfs
    {
        [JsonProperty("common")]
        public SwfsCommon Common { get; set; }
    }

    public partial class SwfsCommon
    {
        [JsonProperty("aries-player")]
        public AriesPlayer AriesPlayer { get; set; }

        [JsonProperty("external-player")]
        public ExternalPlayer ExternalPlayer { get; set; }
    }

    public partial class AriesPlayer
    {
        [JsonProperty("AriesBroadcaster")]
        public AriesBroadcaster AriesBroadcaster { get; set; }

        [JsonProperty("AriesPlayer")]
        public AriesBroadcaster AriesPlayerAriesPlayer { get; set; }

        [JsonProperty("HDSModule")]
        public AriesBroadcaster HdsModule { get; set; }

        [JsonProperty("MessageserverConnector")]
        public AriesBroadcaster MessageserverConnector { get; set; }

        [JsonProperty("ResizablePlayer")]
        public AriesBroadcaster ResizablePlayer { get; set; }
    }

    public partial class AriesBroadcaster
    {
        [JsonProperty("swf")]
        public string Swf { get; set; }
    }

    public partial class ExternalPlayer
    {
        [JsonProperty("RtmpStreamPlayer")]
        public AriesBroadcaster RtmpStreamPlayer { get; set; }
    }

    public partial class Vendor
    {
        [JsonProperty("common")]
        public VendorCommon Common { get; set; }
    }

    public partial class VendorCommon
    {
        [JsonProperty("eventemitter")]
        public Eventemitter Eventemitter { get; set; }

        [JsonProperty("facebook_sdk")]
        public FacebookSdk FacebookSdk { get; set; }

        [JsonProperty("hlsjs")]
        public Hlsjs Hlsjs { get; set; }

        [JsonProperty("ichiba")]
        public CommonIchiba Ichiba { get; set; }

        [JsonProperty("input-mode-checker")]
        public FacebookSdk InputModeChecker { get; set; }

        [JsonProperty("lato")]
        public Lato Lato { get; set; }

        [JsonProperty("leo")]
        public Leo Leo { get; set; }

        [JsonProperty("lodash")]
        public Lodash Lodash { get; set; }

        [JsonProperty("nicoheader")]
        public Nicoheader Nicoheader { get; set; }

        [JsonProperty("placeholders")]
        public FacebookSdk Placeholders { get; set; }

        [JsonProperty("polyfill")]
        public Polyfill Polyfill { get; set; }

        [JsonProperty("react")]
        public Dictionary<string, FacebookSdk> React { get; set; }

        [JsonProperty("statuses")]
        public Statuses Statuses { get; set; }

        [JsonProperty("swfobject")]
        public Swfobject Swfobject { get; set; }
    }

    public partial class Eventemitter
    {
        [JsonProperty("eventemitter")]
        public FacebookSdk EventemitterEventemitter { get; set; }
    }

    public partial class FacebookSdk
    {
        [JsonProperty("js")]
        public string Js { get; set; }
    }

    public partial class Hlsjs
    {
        [JsonProperty("hls")]
        public FacebookSdk Hls { get; set; }
    }

    public partial class CommonIchiba
    {
        [JsonProperty("ichiba_2")]
        public NicolibCommonNotificationHeaderClass Ichiba2 { get; set; }

        [JsonProperty("ichiba_vanilla")]
        public FacebookSdk IchibaVanilla { get; set; }

        [JsonProperty("img")]
        public ImgClass Img { get; set; }
    }

    public partial class NicolibCommonNotificationHeaderClass
    {
        [JsonProperty("css")]
        public string Css { get; set; }
    }

    public partial class Lato
    {
        [JsonProperty("latolatin-bold")]
        public Latolatin LatolatinBold { get; set; }

        [JsonProperty("latolatin-regular")]
        public Latolatin LatolatinRegular { get; set; }
    }

    public partial class Latolatin
    {
        [JsonProperty("woff")]
        public string Woff { get; set; }

        [JsonProperty("woff2")]
        public string Woff2 { get; set; }

        [JsonProperty("ttf")]
        public string Ttf { get; set; }
    }

    public partial class Leo
    {
        [JsonProperty("community_program_ended_guide")]
        public TartuGecko CommunityProgramEndedGuide { get; set; }

        [JsonProperty("fullscreen")]
        public Fullscreen Fullscreen { get; set; }
    }

    public partial class Fullscreen
    {
        [JsonProperty("css")]
        public string Css { get; set; }

        [JsonProperty("js")]
        public string Js { get; set; }
    }

    public partial class Lodash
    {
        [JsonProperty("lodash")]
        public FacebookSdk LodashLodash { get; set; }
    }

    public partial class Nicoheader
    {
        [JsonProperty("nicolib-CommonNotificationHeader")]
        public FacebookSdk NicolibCommonNotificationHeader { get; set; }

        [JsonProperty("resources")]
        public Resources Resources { get; set; }

        [JsonProperty("siteHeader")]
        public FacebookSdk SiteHeader { get; set; }
    }

    public partial class Resources
    {
        [JsonProperty("arrow")]
        public TartuGecko Arrow { get; set; }

        [JsonProperty("bg_notification")]
        public TartuGecko BgNotification { get; set; }

        [JsonProperty("btn_switch")]
        public TartuGecko BtnSwitch { get; set; }

        [JsonProperty("icon_niconico")]
        public TartuGecko IconNiconico { get; set; }

        [JsonProperty("nicolib-CommonNotificationHeader")]
        public NicolibCommonNotificationHeaderClass NicolibCommonNotificationHeader { get; set; }

        [JsonProperty("nicolive-modified")]
        public NicolibCommonNotificationHeaderClass NicoliveModified { get; set; }

        [JsonProperty("nicopo_add")]
        public TartuGecko NicopoAdd { get; set; }

        [JsonProperty("siteHeader")]
        public NicolibCommonNotificationHeaderClass SiteHeader { get; set; }

        [JsonProperty("siteheader-optionbutton")]
        public TartuGecko SiteheaderOptionbutton { get; set; }

        [JsonProperty("tooltip")]
        public TartuGecko Tooltip { get; set; }
    }

    public partial class Polyfill
    {
        [JsonProperty("browser-polyfill")]
        public FacebookSdk BrowserPolyfill { get; set; }

        [JsonProperty("es5-shim")]
        public FacebookSdk Es5Shim { get; set; }

        [JsonProperty("es6-promise")]
        public FacebookSdk Es6Promise { get; set; }

        [JsonProperty("html5shiv")]
        public FacebookSdk Html5Shiv { get; set; }
    }

    public partial class Statuses
    {
        [JsonProperty("statuses")]
        public FacebookSdk StatusesStatuses { get; set; }
    }

    public partial class Swfobject
    {
        [JsonProperty("swfobject")]
        public FacebookSdk SwfobjectSwfobject { get; set; }
    }

    public partial class Billboard
    {
    }

    public partial class Community
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("followPageUrl")]
        public Uri FollowPageUrl { get; set; }

        [JsonProperty("unfollowPageUrl")]
        public Uri UnfollowPageUrl { get; set; }
    }

    public partial class CommunityFollower
    {
        [JsonProperty("records")]
        public object[] Records { get; set; }
    }

    public partial class GoogleAnalytics
    {
        [JsonProperty("tagManager")]
        public TagManager TagManager { get; set; }
    }

    public partial class TagManager
    {
        [JsonProperty("dataLayer")]
        public DataLayer DataLayer { get; set; }
    }

    public partial class DataLayer
    {
        [JsonProperty("push")]
        public Push Push { get; set; }
    }

    public partial class Push
    {
        [JsonProperty("customDimension")]
        public CustomDimension CustomDimension { get; set; }
    }

    public partial class CustomDimension
    {
        [JsonProperty("eventInfoKey")]
        public string EventInfoKey { get; set; }
    }

    public partial class NicoEnqueteClass
    {
        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; }
    }

    public partial class RootObjectPlayer
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("audienceToken")]
        public string AudienceToken { get; set; }

        [JsonProperty("isJumpDisabled")]
        public bool IsJumpDisabled { get; set; }

        [JsonProperty("disablePlayVideoAd")]
        public bool DisablePlayVideoAd { get; set; }

        [JsonProperty("isRestrictedCommentPost")]
        public bool IsRestrictedCommentPost { get; set; }

        [JsonProperty("streamAllocationType")]
        public string StreamAllocationType { get; set; }
    }

    public partial class RootObjectProgram
    {
        [JsonProperty("allegation")]
        public Allegation Allegation { get; set; }

        [JsonProperty("nicoliveProgramId")]
        public string NicoliveProgramId { get; set; }

        [JsonProperty("reliveProgramId")]
        public string ReliveProgramId { get; set; }

        [JsonProperty("broadcastId")]
        public string BroadcastId { get; set; }

        [JsonProperty("providerType")]
        public string ProviderType { get; set; }

        [JsonProperty("visualProviderType")]
        public string VisualProviderType { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("thumbnail")]
        public Thumbnail Thumbnail { get; set; }

        [JsonProperty("supplier")]
        public Supplier Supplier { get; set; }

        [JsonProperty("openTime")]
        public long OpenTime { get; set; }

        [JsonProperty("beginTime")]
        public long BeginTime { get; set; }

        [JsonProperty("vposBaseTime")]
        public long VposBaseTime { get; set; }

        [JsonProperty("endTime")]
        public long EndTime { get; set; }

        [JsonProperty("scheduledEndTime")]
        public long ScheduledEndTime { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("substitute")]
        public Billboard Substitute { get; set; }

        [JsonProperty("tag")]
        public ProgramTag Tag { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }

        [JsonProperty("player")]
        public ProgramPlayer Player { get; set; }

        [JsonProperty("watchPageUrl")]
        public Uri WatchPageUrl { get; set; }

        [JsonProperty("gatePageUrl")]
        public Uri GatePageUrl { get; set; }

        [JsonProperty("mediaServerType")]
        public string MediaServerType { get; set; }

        [JsonProperty("isPrivate")]
        public bool IsPrivate { get; set; }

        [JsonProperty("isSdk")]
        public bool IsSdk { get; set; }

        [JsonProperty("zapping")]
        public ProgramZapping Zapping { get; set; }

        [JsonProperty("report")]
        public Report Report { get; set; }

        [JsonProperty("isFollowerOnly")]
        public bool IsFollowerOnly { get; set; }

        [JsonProperty("cueSheet")]
        public CueSheet CueSheet { get; set; }

        [JsonProperty("cueSheetSnapshot")]
        public CueSheetSnapshot CueSheetSnapshot { get; set; }

        [JsonProperty("nicoad")]
        public ProgramNicoad Nicoad { get; set; }

        [JsonProperty("isGiftEnabled")]
        public bool IsGiftEnabled { get; set; }

        [JsonProperty("stream")]
        public Stream Stream { get; set; }

        [JsonProperty("superichiba")]
        public ProgramSuperichiba Superichiba { get; set; }

        [JsonProperty("isChasePlayEnabled")]
        public bool IsChasePlayEnabled { get; set; }
    }

    public partial class Allegation
    {
        [JsonProperty("commentAllegationApiUrl")]
        public Uri CommentAllegationApiUrl { get; set; }
    }

    public partial class CueSheet
    {
        [JsonProperty("eventsApiUrl")]
        public Uri EventsApiUrl { get; set; }
    }

    public partial class CueSheetSnapshot
    {
        [JsonProperty("commentLocked")]
        public bool CommentLocked { get; set; }

        [JsonProperty("audienceCommentLayout")]
        public string AudienceCommentLayout { get; set; }
    }

    public partial class Links
    {
        [JsonProperty("feedbackPageUrl")]
        public Uri FeedbackPageUrl { get; set; }

        [JsonProperty("contentsTreePageUrl")]
        public Uri ContentsTreePageUrl { get; set; }

        [JsonProperty("programReportPageUrl")]
        public Uri ProgramReportPageUrl { get; set; }

        [JsonProperty("tagReportPageUrl")]
        public Uri TagReportPageUrl { get; set; }
    }

    public partial class ProgramNicoad
    {
        [JsonProperty("totalPoint")]
        public long TotalPoint { get; set; }

        [JsonProperty("ranking")]
        public object[] Ranking { get; set; }
    }

    public partial class ProgramPlayer
    {
        [JsonProperty("embedUrl")]
        public Uri EmbedUrl { get; set; }

        [JsonProperty("banner")]
        public PlayerBanner Banner { get; set; }
    }

    public partial class PlayerBanner
    {
        [JsonProperty("apiUrl")]
        public Uri ApiUrl { get; set; }
    }

    public partial class Report
    {
        [JsonProperty("imageApiUrl")]
        public Uri ImageApiUrl { get; set; }
    }

    public partial class Stream
    {
        [JsonProperty("maxQuality")]
        public string MaxQuality { get; set; }
    }

    public partial class ProgramSuperichiba
    {
        [JsonProperty("allowAudienceToAddNeta")]
        public bool AllowAudienceToAddNeta { get; set; }

        [JsonProperty("ownerId")]
        public string OwnerId { get; set; }

        [JsonProperty("canSupplierUse")]
        public bool CanSupplierUse { get; set; }
    }

    public partial class Supplier
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("pageUrl")]
        public Uri PageUrl { get; set; }

        [JsonProperty("nicopediaArticle")]
        public NicopediaArticle NicopediaArticle { get; set; }

        [JsonProperty("programProviderId")]
        public string ProgramProviderId { get; set; }

        [JsonProperty("icons")]
        public Icons Icons { get; set; }

        [JsonProperty("level")]
        public long? Level { get; set; }
    }

    public partial class Icons
    {
        [JsonProperty("uri50x50")]
        public Uri Uri50X50 { get; set; }

        [JsonProperty("uri150x150")]
        public Uri Uri150X150 { get; set; }
    }

    public partial class NicopediaArticle
    {
        [JsonProperty("pageUrl")]
        public Uri PageUrl { get; set; }

        [JsonProperty("exists")]
        public bool Exists { get; set; }
    }

    public partial class ProgramTag
    {
        [JsonProperty("list")]
        public List[] List { get; set; }

        [JsonProperty("apiUrl")]
        public Uri ApiUrl { get; set; }

        [JsonProperty("registerApiUrl")]
        public Uri RegisterApiUrl { get; set; }

        [JsonProperty("deleteApiUrl")]
        public Uri DeleteApiUrl { get; set; }

        [JsonProperty("apiToken")]
        public string ApiToken { get; set; }

        [JsonProperty("isLocked")]
        public bool IsLocked { get; set; }
    }

    public partial class List
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("existsNicopediaArticle")]
        public bool ExistsNicopediaArticle { get; set; }

        [JsonProperty("nicopediaArticlePageUrlPath")]
        public string NicopediaArticlePageUrlPath { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("isLocked")]
        public bool IsLocked { get; set; }

        [JsonProperty("isDeletable")]
        public bool IsDeletable { get; set; }
    }

    public partial class Thumbnail
    {
        [JsonProperty("small")]
        public Uri Small { get; set; }

        [JsonProperty("huge")]
        public Huge Huge { get; set; }
    }

    public partial class Huge
    {
        [JsonProperty("s352x198")]
        public Uri S352X198 { get; set; }

        [JsonProperty("s640x360")]
        public Uri S640X360 { get; set; }

        [JsonProperty("s1280x720")]
        public Uri S1280X720 { get; set; }

        [JsonProperty("s1920x1080")]
        public Uri S1920X1080 { get; set; }
    }

    public partial class ProgramZapping
    {
        [JsonProperty("listApiUrl")]
        public Uri ListApiUrl { get; set; }

        [JsonProperty("listUpdateIntervalMs")]
        public long ListUpdateIntervalMs { get; set; }
    }

    public partial class RootObjectSite
    {
        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("serverTime")]
        public long ServerTime { get; set; }

        [JsonProperty("frontendVersion")]
        public string FrontendVersion { get; set; }

        [JsonProperty("apiBaseUrl")]
        public Uri ApiBaseUrl { get; set; }

        [JsonProperty("staticResourceBaseUrl")]
        public Uri StaticResourceBaseUrl { get; set; }

        [JsonProperty("topPageUrl")]
        public Uri TopPageUrl { get; set; }

        [JsonProperty("programCreatePageUrl")]
        public Uri ProgramCreatePageUrl { get; set; }

        [JsonProperty("programEditPageUrl")]
        public Uri ProgramEditPageUrl { get; set; }

        [JsonProperty("historyPageUrl")]
        public Uri HistoryPageUrl { get; set; }

        [JsonProperty("myPageUrl")]
        public Uri MyPageUrl { get; set; }

        [JsonProperty("rankingPageUrl")]
        public Uri RankingPageUrl { get; set; }

        [JsonProperty("searchPageUrl")]
        public Uri SearchPageUrl { get; set; }

        [JsonProperty("familyService")]
        public FamilyService FamilyService { get; set; }

        [JsonProperty("environments")]
        public Environments Environments { get; set; }

        [JsonProperty("relive")]
        public Relive Relive { get; set; }

        [JsonProperty("information")]
        public Information Information { get; set; }

        [JsonProperty("rule")]
        public Rule Rule { get; set; }

        [JsonProperty("spec")]
        public Spec Spec { get; set; }

        [JsonProperty("ad")]
        public SiteAd Ad { get; set; }

        [JsonProperty("program")]
        public SiteProgram Program { get; set; }

        [JsonProperty("tag")]
        public SiteTag Tag { get; set; }

        [JsonProperty("coe")]
        public Coe Coe { get; set; }

        [JsonProperty("notify")]
        public Notify Notify { get; set; }

        [JsonProperty("timeshift")]
        public Timeshift Timeshift { get; set; }

        [JsonProperty("broadcast")]
        public Broadcast Broadcast { get; set; }

        [JsonProperty("enquete")]
        public AutoExtend Enquete { get; set; }

        [JsonProperty("trialWatch")]
        public AutoExtend TrialWatch { get; set; }

        [JsonProperty("videoQuote")]
        public AutoExtend VideoQuote { get; set; }

        [JsonProperty("autoExtend")]
        public AutoExtend AutoExtend { get; set; }

        [JsonProperty("nicobus")]
        public Nicobus Nicobus { get; set; }

        [JsonProperty("dmc")]
        public Dmc Dmc { get; set; }
    }

    public partial class SiteAd
    {
        [JsonProperty("adsApiBaseUrl")]
        public Uri AdsApiBaseUrl { get; set; }
    }

    public partial class AutoExtend
    {
        [JsonProperty("usageHelpPageUrl")]
        public Uri UsageHelpPageUrl { get; set; }
    }

    public partial class Broadcast
    {
        [JsonProperty("usageHelpPageUrl")]
        public Uri UsageHelpPageUrl { get; set; }

        [JsonProperty("stableBroadcastHelpPageUrl")]
        public Uri StableBroadcastHelpPageUrl { get; set; }

        [JsonProperty("niconicoLiveEncoder")]
        public Nair NiconicoLiveEncoder { get; set; }

        [JsonProperty("nair")]
        public Nair Nair { get; set; }

        [JsonProperty("broadcasterStreamHelpPageUrl")]
        public Uri BroadcasterStreamHelpPageUrl { get; set; }
    }

    public partial class Nair
    {
        [JsonProperty("downloadPageUrl")]
        public Uri DownloadPageUrl { get; set; }
    }

    public partial class Coe
    {
        [JsonProperty("resourcesBaseUrl")]
        public Uri ResourcesBaseUrl { get; set; }

        [JsonProperty("coeContentBaseUrl")]
        public Uri CoeContentBaseUrl { get; set; }
    }

    public partial class Dmc
    {
        [JsonProperty("webRtc")]
        public WebRtc WebRtc { get; set; }
    }

    public partial class WebRtc
    {
        [JsonProperty("stunServerUrls")]
        public object[] StunServerUrls { get; set; }
    }

    public partial class Environments
    {
        [JsonProperty("runningMode")]
        public string RunningMode { get; set; }
    }

    public partial class FamilyService
    {
        [JsonProperty("account")]
        public Account Account { get; set; }

        [JsonProperty("app")]
        public App App { get; set; }

        [JsonProperty("atsumaru")]
        public App Atsumaru { get; set; }

        [JsonProperty("blomaga")]
        public App Blomaga { get; set; }

        [JsonProperty("channel")]
        public Channel Channel { get; set; }

        [JsonProperty("commons")]
        public App Commons { get; set; }

        [JsonProperty("community")]
        public App Community { get; set; }

        [JsonProperty("denfaminicogamer")]
        public App Denfaminicogamer { get; set; }

        [JsonProperty("dic")]
        public App Dic { get; set; }

        [JsonProperty("help")]
        public Help Help { get; set; }

        [JsonProperty("ichiba")]
        public FamilyServiceIchiba Ichiba { get; set; }

        [JsonProperty("jk")]
        public App Jk { get; set; }

        [JsonProperty("mastodon")]
        public App Mastodon { get; set; }

        [JsonProperty("news")]
        public App News { get; set; }

        [JsonProperty("nicoad")]
        public FamilyServiceNicoad Nicoad { get; set; }

        [JsonProperty("niconico")]
        public App Niconico { get; set; }

        [JsonProperty("niconicoQ")]
        public App NiconicoQ { get; set; }

        [JsonProperty("point")]
        public Point Point { get; set; }

        [JsonProperty("seiga")]
        public App Seiga { get; set; }

        [JsonProperty("site")]
        public FamilyServiceSite Site { get; set; }

        [JsonProperty("solid")]
        public App Solid { get; set; }

        [JsonProperty("uad")]
        public App Uad { get; set; }

        [JsonProperty("video")]
        public Video Video { get; set; }

        [JsonProperty("faq")]
        public Bugreport Faq { get; set; }

        [JsonProperty("bugreport")]
        public Bugreport Bugreport { get; set; }

        [JsonProperty("rightsControlProgram")]
        public Bugreport RightsControlProgram { get; set; }

        [JsonProperty("licenseSearch")]
        public Bugreport LicenseSearch { get; set; }

        [JsonProperty("info")]
        public Info Info { get; set; }

        [JsonProperty("search")]
        public FamilyServiceSearch Search { get; set; }

        [JsonProperty("nicoex")]
        public FamilyServiceNicoex Nicoex { get; set; }

        [JsonProperty("superichiba")]
        public FamilyServiceSuperichiba Superichiba { get; set; }

        [JsonProperty("nAir")]
        public App NAir { get; set; }
    }

    public partial class Account
    {
        [JsonProperty("accountRegistrationPageUrl")]
        public Uri AccountRegistrationPageUrl { get; set; }

        [JsonProperty("loginPageUrl")]
        public Uri LoginPageUrl { get; set; }

        [JsonProperty("logoutPageUrl")]
        public Uri LogoutPageUrl { get; set; }

        [JsonProperty("premiumMemberRegistrationPageUrl")]
        public Uri PremiumMemberRegistrationPageUrl { get; set; }

        [JsonProperty("trackingParams")]
        public TrackingParams TrackingParams { get; set; }

        [JsonProperty("profileRegistrationPageUrl")]
        public Uri ProfileRegistrationPageUrl { get; set; }

        [JsonProperty("contactsPageUrl")]
        public Uri ContactsPageUrl { get; set; }

        [JsonProperty("verifyEmailsPageUrl")]
        public Uri VerifyEmailsPageUrl { get; set; }
    }

    public partial class TrackingParams
    {
        [JsonProperty("siteId")]
        public string SiteId { get; set; }

        [JsonProperty("pageId")]
        public string PageId { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("programStatus")]
        public string ProgramStatus { get; set; }
    }

    public partial class App
    {
        [JsonProperty("topPageUrl")]
        public Uri TopPageUrl { get; set; }
    }

    public partial class Bugreport
    {
        [JsonProperty("pageUrl")]
        public Uri PageUrl { get; set; }
    }

    public partial class Channel
    {
        [JsonProperty("topPageUrl")]
        public Uri TopPageUrl { get; set; }

        [JsonProperty("forOrganizationAndCompanyPageUrl")]
        public Uri ForOrganizationAndCompanyPageUrl { get; set; }
    }

    public partial class Help
    {
        [JsonProperty("liveHelpPageUrl")]
        public Uri LiveHelpPageUrl { get; set; }

        [JsonProperty("systemRequirementsPageUrl")]
        public Uri SystemRequirementsPageUrl { get; set; }
    }

    public partial class FamilyServiceIchiba
    {
        [JsonProperty("configBaseUrl")]
        public Uri ConfigBaseUrl { get; set; }

        [JsonProperty("scriptUrl")]
        public Uri ScriptUrl { get; set; }

        [JsonProperty("topPageUrl")]
        public Uri TopPageUrl { get; set; }
    }

    public partial class Info
    {
        [JsonProperty("warnForPhishingPageUrl")]
        public Uri WarnForPhishingPageUrl { get; set; }

        [JsonProperty("smartphoneSdkPageUrl")]
        public Uri SmartphoneSdkPageUrl { get; set; }

        [JsonProperty("nintendoGuidelinePageUrl")]
        public Uri NintendoGuidelinePageUrl { get; set; }
    }

    public partial class FamilyServiceNicoad
    {
        [JsonProperty("topPageUrl")]
        public Uri TopPageUrl { get; set; }

        [JsonProperty("apiBaseUrl")]
        public Uri ApiBaseUrl { get; set; }
    }

    public partial class FamilyServiceNicoex
    {
        [JsonProperty("apiBaseUrl")]
        public Uri ApiBaseUrl { get; set; }
    }

    public partial class Point
    {
        [JsonProperty("topPageUrl")]
        public Uri TopPageUrl { get; set; }

        [JsonProperty("purchasePageUrl")]
        public Uri PurchasePageUrl { get; set; }
    }

    public partial class FamilyServiceSearch
    {
        [JsonProperty("suggestionApiUrl")]
        public Uri SuggestionApiUrl { get; set; }
    }

    public partial class FamilyServiceSite
    {
        [JsonProperty("serviceListPageUrl")]
        public Uri ServiceListPageUrl { get; set; }

        [JsonProperty("salesAdvertisingPageUrl")]
        public Uri SalesAdvertisingPageUrl { get; set; }
    }

    public partial class FamilyServiceSuperichiba
    {
        [JsonProperty("apiBaseUrl")]
        public Uri ApiBaseUrl { get; set; }

        [JsonProperty("launchApiBaseUrl")]
        public Uri LaunchApiBaseUrl { get; set; }

        [JsonProperty("oroshiuriIchibaBaseUrl")]
        public Uri OroshiuriIchibaBaseUrl { get; set; }
    }

    public partial class Video
    {
        [JsonProperty("topPageUrl")]
        public Uri TopPageUrl { get; set; }

        [JsonProperty("myPageUrl")]
        public Uri MyPageUrl { get; set; }
    }

    public partial class Information
    {
        [JsonProperty("html5PlayerInformationPageUrl")]
        public Uri Html5PlayerInformationPageUrl { get; set; }

        [JsonProperty("flashPlayerInstallInformationPageUrl")]
        public Uri FlashPlayerInstallInformationPageUrl { get; set; }
    }

    public partial class Nicobus
    {
        [JsonProperty("publicApiBaseUrl")]
        public Uri PublicApiBaseUrl { get; set; }
    }

    public partial class Notify
    {
        [JsonProperty("unreadApiUrl")]
        public Uri UnreadApiUrl { get; set; }

        [JsonProperty("contentApiUrl")]
        public Uri ContentApiUrl { get; set; }

        [JsonProperty("updateUnreadIntervalMs")]
        public long UpdateUnreadIntervalMs { get; set; }
    }

    public partial class SiteProgram
    {
        [JsonProperty("liveCount")]
        public long LiveCount { get; set; }
    }

    public partial class Relive
    {
        [JsonProperty("apiBaseUrl")]
        public Uri ApiBaseUrl { get; set; }

        [JsonProperty("webSocketUrl")]
        public string WebSocketUrl { get; set; }

        [JsonProperty("csrfToken")]
        public string CsrfToken { get; set; }

        [JsonProperty("audienceToken")]
        public string AudienceToken { get; set; }
    }

    public partial class Rule
    {
        [JsonProperty("agreementPageUrl")]
        public Uri AgreementPageUrl { get; set; }

        [JsonProperty("guidelinePageUrl")]
        public Uri GuidelinePageUrl { get; set; }
    }

    public partial class Spec
    {
        [JsonProperty("watchUsageAndDevicePageUrl")]
        public Uri WatchUsageAndDevicePageUrl { get; set; }

        [JsonProperty("broadcastUsageDevicePageUrl")]
        public Uri BroadcastUsageDevicePageUrl { get; set; }

        [JsonProperty("minogashiProgramPageUrl")]
        public Uri MinogashiProgramPageUrl { get; set; }

        [JsonProperty("cruisePageUrl")]
        public Uri CruisePageUrl { get; set; }
    }

    public partial class SiteTag
    {
        [JsonProperty("revisionCheckIntervalMs")]
        public long RevisionCheckIntervalMs { get; set; }

        [JsonProperty("registerHelpPageUrl")]
        public Uri RegisterHelpPageUrl { get; set; }

        [JsonProperty("userRegistrableMax")]
        public long UserRegistrableMax { get; set; }

        [JsonProperty("textMaxLength")]
        public long TextMaxLength { get; set; }
    }

    public partial class Timeshift
    {
        [JsonProperty("reservationDetailListApiUrl")]
        public Uri ReservationDetailListApiUrl { get; set; }
    }

    public partial class SocialGroup
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("broadcastHistoryPageUrl")]
        public Uri BroadcastHistoryPageUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("socialGroupPageUrl")]
        public Uri SocialGroupPageUrl { get; set; }

        [JsonProperty("thumbnailImageUrl")]
        public Uri ThumbnailImageUrl { get; set; }

        [JsonProperty("thumbnailSmallImageUrl")]
        public Uri ThumbnailSmallImageUrl { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("isFollowed")]
        public bool IsFollowed { get; set; }

        [JsonProperty("isJoined")]
        public bool IsJoined { get; set; }
    }

    public partial class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("birthday")]
        public DateTimeOffset Birthday { get; set; }

        [JsonProperty("isExplicitlyLoginable")]
        public bool IsExplicitlyLoginable { get; set; }

        [JsonProperty("isMobileMailAddressRegistered")]
        public bool IsMobileMailAddressRegistered { get; set; }

        [JsonProperty("isMailRegistered")]
        public bool IsMailRegistered { get; set; }

        [JsonProperty("isProfileRegistered")]
        public bool IsProfileRegistered { get; set; }

        [JsonProperty("isLoggedIn")]
        public bool IsLoggedIn { get; set; }

        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("isOperator")]
        public bool IsOperator { get; set; }

        [JsonProperty("isBroadcaster")]
        public bool IsBroadcaster { get; set; }

        [JsonProperty("premiumOrigin")]
        public string PremiumOrigin { get; set; }

        [JsonProperty("permissions")]
        public string[] Permissions { get; set; }

        [JsonProperty("nicosid")]
        public string Nicosid { get; set; }

        [JsonProperty("superichiba")]
        public UserSuperichiba Superichiba { get; set; }
    }

    public partial class UserSuperichiba
    {
        [JsonProperty("deletable")]
        public bool Deletable { get; set; }

        [JsonProperty("hasBroadcasterRole")]
        public bool HasBroadcasterRole { get; set; }
    }
}
