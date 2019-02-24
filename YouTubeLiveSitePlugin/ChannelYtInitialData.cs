using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using Codeplex.Data;
using Newtonsoft.Json;
using System.Diagnostics;

namespace YouTubeLiveSitePlugin.Low.ChannelYtInitialData
{
#pragma warning disable IDE1006 // Naming Styles
    public class Param
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class ServiceTrackingParam
    {
        public string service { get; set; }
        public List<Param> @params { get; set; }
    }

    public class WebResponseContextPreloadData
    {
        public List<string> preloadThumbnailUrls { get; set; }
    }

    public class YtConfigData
    {
        public string csn { get; set; }
        public string visitorData { get; set; }
        public int sessionIndex { get; set; }
        public string delegatedSessionId { get; set; }
        public int rootVisualElementType { get; set; }
    }

    public class Run
    {
        public string text { get; set; }
    }

    public class Title
    {
        public List<Run> runs { get; set; }
    }

    public class WebCommandMetadata
    {
        public string url { get; set; }
    }

    public class CommandMetadata
    {
        public WebCommandMetadata webCommandMetadata { get; set; }
    }

    public class UrlEndpoint
    {
        public string url { get; set; }
    }

    public class WebNavigationEndpointData
    {
        public string url { get; set; }
    }

    public class NavigationEndpoint
    {
        public CommandMetadata commandMetadata { get; set; }
        public UrlEndpoint urlEndpoint { get; set; }
        public WebNavigationEndpointData webNavigationEndpointData { get; set; }
    }

    public class Run2
    {
        public string text { get; set; }
        public NavigationEndpoint navigationEndpoint { get; set; }
    }

    public class Subtitle
    {
        public List<Run2> runs { get; set; }
    }

    public class Run3
    {
        public string text { get; set; }
    }

    public class Description
    {
        public List<Run3> runs { get; set; }
    }

    public class Run4
    {
        public string text { get; set; }
    }

    public class ResponsePlaceholder
    {
        public List<Run4> runs { get; set; }
    }

    public class PolymerOptOutFeedbackOptionRenderer
    {
        public string optionKey { get; set; }
        public Description description { get; set; }
        public ResponsePlaceholder responsePlaceholder { get; set; }
    }

    public class Run5
    {
        public string text { get; set; }
    }

    public class Description2
    {
        public List<Run5> runs { get; set; }
    }

    public class PolymerOptOutFeedbackNullOptionRenderer
    {
        public Description2 description { get; set; }
    }

    public class Option
    {
        public PolymerOptOutFeedbackOptionRenderer polymerOptOutFeedbackOptionRenderer { get; set; }
        public PolymerOptOutFeedbackNullOptionRenderer polymerOptOutFeedbackNullOptionRenderer { get; set; }
    }

    public class WebCommandMetadata2
    {
        public string url { get; set; }
    }

    public class CommandMetadata2
    {
        public WebCommandMetadata2 webCommandMetadata { get; set; }
    }

    public class UrlEndpoint2
    {
        public string url { get; set; }
    }

    public class WebNavigationEndpointData2
    {
        public string url { get; set; }
    }

    public class NavigationEndpoint2
    {
        public CommandMetadata2 commandMetadata { get; set; }
        public UrlEndpoint2 urlEndpoint { get; set; }
        public WebNavigationEndpointData2 webNavigationEndpointData { get; set; }
    }

    public class Run6
    {
        public string text { get; set; }
        public NavigationEndpoint2 navigationEndpoint { get; set; }
    }

    public class Disclaimer
    {
        public List<Run6> runs { get; set; }
    }

    public class Run7
    {
        public string text { get; set; }
    }

    public class Text
    {
        public List<Run7> runs { get; set; }
    }

    public class ButtonRenderer
    {
        public string style { get; set; }
        public string size { get; set; }
        public bool isDisabled { get; set; }
        public Text text { get; set; }
    }

    public class DismissButton
    {
        public ButtonRenderer buttonRenderer { get; set; }
    }

    public class Run8
    {
        public string text { get; set; }
    }

    public class Text2
    {
        public List<Run8> runs { get; set; }
    }

    public class ButtonRenderer2
    {
        public string style { get; set; }
        public string size { get; set; }
        public bool isDisabled { get; set; }
        public Text2 text { get; set; }
    }

    public class SubmitButton
    {
        public ButtonRenderer2 buttonRenderer { get; set; }
    }

    public class Icon
    {
        public string iconType { get; set; }
    }

    public class ButtonRenderer3
    {
        public string style { get; set; }
        public string size { get; set; }
        public bool isDisabled { get; set; }
        public Icon icon { get; set; }
    }

    public class CloseButton
    {
        public ButtonRenderer3 buttonRenderer { get; set; }
    }

    public class Run9
    {
        public string text { get; set; }
    }

    public class Text3
    {
        public List<Run9> runs { get; set; }
    }

    public class ButtonRenderer4
    {
        public string style { get; set; }
        public string size { get; set; }
        public bool isDisabled { get; set; }
        public Text3 text { get; set; }
    }

    public class CancelButton
    {
        public ButtonRenderer4 buttonRenderer { get; set; }
    }

    public class PolymerOptOutFeedbackDialogRenderer
    {
        public Title title { get; set; }
        public Subtitle subtitle { get; set; }
        public List<Option> options { get; set; }
        public Disclaimer disclaimer { get; set; }
        public DismissButton dismissButton { get; set; }
        public SubmitButton submitButton { get; set; }
        public CloseButton closeButton { get; set; }
        public CancelButton cancelButton { get; set; }
    }

    public class FeedbackDialog
    {
        public PolymerOptOutFeedbackDialogRenderer polymerOptOutFeedbackDialogRenderer { get; set; }
    }

    public class WebResponseContextExtensionData
    {
        public WebResponseContextPreloadData webResponseContextPreloadData { get; set; }
        public YtConfigData ytConfigData { get; set; }
        public FeedbackDialog feedbackDialog { get; set; }
    }

    public class ResponseContext
    {
        public List<ServiceTrackingParam> serviceTrackingParams { get; set; }
        public int maxAgeSeconds { get; set; }
        public WebResponseContextExtensionData webResponseContextExtensionData { get; set; }
    }

    public class WebCommandMetadata3
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class CommandMetadata3
    {
        public WebCommandMetadata3 webCommandMetadata { get; set; }
    }

    public class BrowseEndpoint
    {
        public string browseId { get; set; }
        public string @params { get; set; }
        public string canonicalBaseUrl { get; set; }
    }

    public class WebNavigationEndpointData3
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class Endpoint
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata3 commandMetadata { get; set; }
        public BrowseEndpoint browseEndpoint { get; set; }
        public WebNavigationEndpointData3 webNavigationEndpointData { get; set; }
    }

    public class Thumbnail2
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class WebThumbnailDetailsExtensionData
    {
        public bool isPreloaded { get; set; }
    }

    public class Thumbnail
    {
        public List<Thumbnail2> thumbnails { get; set; }
        public WebThumbnailDetailsExtensionData webThumbnailDetailsExtensionData { get; set; }
    }

    public class AccessibilityData
    {
        public string label { get; set; }
    }

    public class Accessibility
    {
        public AccessibilityData accessibilityData { get; set; }
    }

    public class Title2
    {
        public Accessibility accessibility { get; set; }
        public string simpleText { get; set; }
    }

    public class DescriptionSnippet
    {
        public string simpleText { get; set; }
    }

    public class Run10
    {
        public string text { get; set; }
    }

    public class ViewCountText
    {
        public List<Run10> runs { get; set; }
    }

    public class WebCommandMetadata4
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class CommandMetadata4
    {
        public WebCommandMetadata4 webCommandMetadata { get; set; }
    }

    public class WatchEndpoint
    {
        public string videoId { get; set; }
    }

    public class WebNavigationEndpointData4
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class NavigationEndpoint3
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata4 commandMetadata { get; set; }
        public WatchEndpoint watchEndpoint { get; set; }
        public WebNavigationEndpointData4 webNavigationEndpointData { get; set; }
    }

    public class MetadataBadgeRenderer
    {
        public string style { get; set; }
        public string label { get; set; }
        public string trackingParams { get; set; }
    }

    public class Badge
    {
        public MetadataBadgeRenderer metadataBadgeRenderer { get; set; }
    }

    public class Icon2
    {
        public string iconType { get; set; }
    }

    public class MetadataBadgeRenderer2
    {
        public Icon2 icon { get; set; }
        public string style { get; set; }
        public string tooltip { get; set; }
        public string trackingParams { get; set; }
    }

    public class OwnerBadge
    {
        public MetadataBadgeRenderer2 metadataBadgeRenderer { get; set; }
    }

    public class Thumbnail3
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class ChannelThumbnail
    {
        public List<Thumbnail3> thumbnails { get; set; }
    }

    public class Run11
    {
        public string text { get; set; }
    }

    public class ShortViewCountText
    {
        public List<Run11> runs { get; set; }
    }

    public class Run12
    {
        public string text { get; set; }
    }

    public class Text4
    {
        public List<Run12> runs { get; set; }
    }

    public class WebCommandMetadata5
    {
        public string url { get; set; }
        public bool sendPost { get; set; }
    }

    public class CommandMetadata5
    {
        public WebCommandMetadata5 webCommandMetadata { get; set; }
    }

    public class Action
    {
        public string addedVideoId { get; set; }
        public string action { get; set; }
    }

    public class PlaylistEditEndpoint
    {
        public string playlistId { get; set; }
        public List<Action> actions { get; set; }
    }

    public class AddToPlaylistServiceEndpoint
    {
        public string videoId { get; set; }
    }

    public class ServiceEndpoint
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata5 commandMetadata { get; set; }
        public PlaylistEditEndpoint playlistEditEndpoint { get; set; }
        public AddToPlaylistServiceEndpoint addToPlaylistServiceEndpoint { get; set; }
    }

    public class Icon3
    {
        public string iconType { get; set; }
    }

    public class MenuServiceItemRenderer
    {
        public Text4 text { get; set; }
        public ServiceEndpoint serviceEndpoint { get; set; }
        public string trackingParams { get; set; }
        public Icon3 icon { get; set; }
    }

    public class Item
    {
        public MenuServiceItemRenderer menuServiceItemRenderer { get; set; }
    }

    public class AccessibilityData2
    {
        public string label { get; set; }
    }

    public class Accessibility2
    {
        public AccessibilityData2 accessibilityData { get; set; }
    }

    public class MenuRenderer
    {
        public List<Item> items { get; set; }
        public string trackingParams { get; set; }
        public Accessibility2 accessibility { get; set; }
    }

    public class Menu
    {
        public MenuRenderer menuRenderer { get; set; }
    }

    public class UntoggledIcon
    {
        public string iconType { get; set; }
    }

    public class ToggledIcon
    {
        public string iconType { get; set; }
    }

    public class WebCommandMetadata6
    {
        public string url { get; set; }
        public bool sendPost { get; set; }
    }

    public class CommandMetadata6
    {
        public WebCommandMetadata6 webCommandMetadata { get; set; }
    }

    public class Action2
    {
        public string addedVideoId { get; set; }
        public string action { get; set; }
    }

    public class PlaylistEditEndpoint2
    {
        public string playlistId { get; set; }
        public List<Action2> actions { get; set; }
    }

    public class UntoggledServiceEndpoint
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata6 commandMetadata { get; set; }
        public PlaylistEditEndpoint2 playlistEditEndpoint { get; set; }
    }

    public class WebCommandMetadata7
    {
        public string url { get; set; }
        public bool sendPost { get; set; }
    }

    public class CommandMetadata7
    {
        public WebCommandMetadata7 webCommandMetadata { get; set; }
    }

    public class Action3
    {
        public string action { get; set; }
        public string removedVideoId { get; set; }
    }

    public class PlaylistEditEndpoint3
    {
        public string playlistId { get; set; }
        public List<Action3> actions { get; set; }
    }

    public class ToggledServiceEndpoint
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata7 commandMetadata { get; set; }
        public PlaylistEditEndpoint3 playlistEditEndpoint { get; set; }
    }

    public class AccessibilityData3
    {
        public string label { get; set; }
    }

    public class UntoggledAccessibility
    {
        public AccessibilityData3 accessibilityData { get; set; }
    }

    public class AccessibilityData4
    {
        public string label { get; set; }
    }

    public class ToggledAccessibility
    {
        public AccessibilityData4 accessibilityData { get; set; }
    }

    public class ThumbnailOverlayToggleButtonRenderer
    {
        public bool isToggled { get; set; }
        public UntoggledIcon untoggledIcon { get; set; }
        public ToggledIcon toggledIcon { get; set; }
        public string untoggledTooltip { get; set; }
        public string toggledTooltip { get; set; }
        public UntoggledServiceEndpoint untoggledServiceEndpoint { get; set; }
        public ToggledServiceEndpoint toggledServiceEndpoint { get; set; }
        public UntoggledAccessibility untoggledAccessibility { get; set; }
        public ToggledAccessibility toggledAccessibility { get; set; }
    }

    public class ThumbnailOverlay
    {
        public ThumbnailOverlayToggleButtonRenderer thumbnailOverlayToggleButtonRenderer { get; set; }
    }

    public class VideoRenderer
    {
        public string videoId { get; set; }
        public Thumbnail thumbnail { get; set; }
        public Title2 title { get; set; }
        public DescriptionSnippet descriptionSnippet { get; set; }
        public ViewCountText viewCountText { get; set; }
        public NavigationEndpoint3 navigationEndpoint { get; set; }
        public List<Badge> badges { get; set; }
        public List<OwnerBadge> ownerBadges { get; set; }
        public ChannelThumbnail channelThumbnail { get; set; }
        public string trackingParams { get; set; }
        public bool showActionMenu { get; set; }
        public ShortViewCountText shortViewCountText { get; set; }
        public Menu menu { get; set; }
        public List<ThumbnailOverlay> thumbnailOverlays { get; set; }
    }

    public class Content3
    {
        public VideoRenderer videoRenderer { get; set; }
    }

    public class ItemSectionRenderer
    {
        public List<Content3> contents { get; set; }
        public string trackingParams { get; set; }
    }

    public class Content2
    {
        public ItemSectionRenderer itemSectionRenderer { get; set; }
    }

    public class WebCommandMetadata8
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class CommandMetadata8
    {
        public WebCommandMetadata8 webCommandMetadata { get; set; }
    }

    public class BrowseEndpoint2
    {
        public string browseId { get; set; }
        public string @params { get; set; }
        public string canonicalBaseUrl { get; set; }
    }

    public class WebNavigationEndpointData5
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class Endpoint2
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata8 commandMetadata { get; set; }
        public BrowseEndpoint2 browseEndpoint { get; set; }
        public WebNavigationEndpointData5 webNavigationEndpointData { get; set; }
    }

    public class ContentTypeSubMenuItem
    {
        public Endpoint2 endpoint { get; set; }
        public string title { get; set; }
        public bool selected { get; set; }
    }

    public class WebCommandMetadata9
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class CommandMetadata9
    {
        public WebCommandMetadata9 webCommandMetadata { get; set; }
    }

    public class BrowseEndpoint3
    {
        public string browseId { get; set; }
        public string @params { get; set; }
        public string canonicalBaseUrl { get; set; }
    }

    public class WebNavigationEndpointData6
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class Endpoint3
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata9 commandMetadata { get; set; }
        public BrowseEndpoint3 browseEndpoint { get; set; }
        public WebNavigationEndpointData6 webNavigationEndpointData { get; set; }
    }

    public class FlowSubMenuItem
    {
        public Endpoint3 endpoint { get; set; }
        public string title { get; set; }
        public bool selected { get; set; }
    }

    public class WebCommandMetadata10
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class CommandMetadata10
    {
        public WebCommandMetadata10 webCommandMetadata { get; set; }
    }

    public class BrowseEndpoint4
    {
        public string browseId { get; set; }
        public string @params { get; set; }
        public string canonicalBaseUrl { get; set; }
    }

    public class WebNavigationEndpointData7
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class NavigationEndpoint4
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata10 commandMetadata { get; set; }
        public BrowseEndpoint4 browseEndpoint { get; set; }
        public WebNavigationEndpointData7 webNavigationEndpointData { get; set; }
    }

    public class SubMenuItem
    {
        public string title { get; set; }
        public bool selected { get; set; }
        public NavigationEndpoint4 navigationEndpoint { get; set; }
    }

    public class Icon4
    {
        public string iconType { get; set; }
    }

    public class AccessibilityData5
    {
        public string label { get; set; }
    }

    public class Accessibility3
    {
        public AccessibilityData5 accessibilityData { get; set; }
    }

    public class SortFilterSubMenuRenderer
    {
        public List<SubMenuItem> subMenuItems { get; set; }
        public string title { get; set; }
        public Icon4 icon { get; set; }
        public Accessibility3 accessibility { get; set; }
    }

    public class SortSetting
    {
        public SortFilterSubMenuRenderer sortFilterSubMenuRenderer { get; set; }
    }

    public class ChannelSubMenuRenderer
    {
        public List<ContentTypeSubMenuItem> contentTypeSubMenuItems { get; set; }
        public List<FlowSubMenuItem> flowSubMenuItems { get; set; }
        public SortSetting sortSetting { get; set; }
    }

    public class SubMenu
    {
        public ChannelSubMenuRenderer channelSubMenuRenderer { get; set; }
    }

    public class SectionListRenderer
    {
        public List<Content2> contents { get; set; }
        public string trackingParams { get; set; }
        public SubMenu subMenu { get; set; }
    }

    public class Content
    {
        public SectionListRenderer sectionListRenderer { get; set; }
    }

    public class TabRenderer
    {
        public Endpoint endpoint { get; set; }
        public string title { get; set; }
        public bool selected { get; set; }
        public string trackingParams { get; set; }
        public Content content { get; set; }
    }

    public class WebCommandMetadata11
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class CommandMetadata11
    {
        public WebCommandMetadata11 webCommandMetadata { get; set; }
    }

    public class BrowseEndpoint5
    {
        public string browseId { get; set; }
        public string @params { get; set; }
        public string canonicalBaseUrl { get; set; }
    }

    public class WebNavigationEndpointData8
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class Endpoint4
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata11 commandMetadata { get; set; }
        public BrowseEndpoint5 browseEndpoint { get; set; }
        public WebNavigationEndpointData8 webNavigationEndpointData { get; set; }
    }

    public class ExpandableTabRenderer
    {
        public Endpoint4 endpoint { get; set; }
        public string title { get; set; }
        public bool selected { get; set; }
    }

    public class Tab
    {
        public TabRenderer tabRenderer { get; set; }
        public ExpandableTabRenderer expandableTabRenderer { get; set; }
    }

    public class TwoColumnBrowseResultsRenderer
    {
        public List<Tab> tabs { get; set; }
    }

    public class Contents
    {
        public TwoColumnBrowseResultsRenderer twoColumnBrowseResultsRenderer { get; set; }
    }

    public class WebCommandMetadata12
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class CommandMetadata12
    {
        public WebCommandMetadata12 webCommandMetadata { get; set; }
    }

    public class BrowseEndpoint6
    {
        public string browseId { get; set; }
        public string canonicalBaseUrl { get; set; }
    }

    public class WebNavigationEndpointData9
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class NavigationEndpoint5
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata12 commandMetadata { get; set; }
        public BrowseEndpoint6 browseEndpoint { get; set; }
        public WebNavigationEndpointData9 webNavigationEndpointData { get; set; }
    }

    public class Thumbnail4
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class WebThumbnailDetailsExtensionData2
    {
        public bool isPreloaded { get; set; }
    }

    public class Avatar
    {
        public List<Thumbnail4> thumbnails { get; set; }
        public WebThumbnailDetailsExtensionData2 webThumbnailDetailsExtensionData { get; set; }
    }

    public class Thumbnail5
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class WebThumbnailDetailsExtensionData3
    {
        public bool isPreloaded { get; set; }
    }

    public class Banner
    {
        public List<Thumbnail5> thumbnails { get; set; }
        public WebThumbnailDetailsExtensionData3 webThumbnailDetailsExtensionData { get; set; }
    }

    public class Icon5
    {
        public string iconType { get; set; }
    }

    public class MetadataBadgeRenderer3
    {
        public Icon5 icon { get; set; }
        public string style { get; set; }
        public string tooltip { get; set; }
        public string trackingParams { get; set; }
    }

    public class Badge2
    {
        public MetadataBadgeRenderer3 metadataBadgeRenderer { get; set; }
    }

    public class WebCommandMetadata13
    {
        public string url { get; set; }
    }

    public class CommandMetadata13
    {
        public WebCommandMetadata13 webCommandMetadata { get; set; }
    }

    public class UrlEndpoint3
    {
        public string url { get; set; }
        public string target { get; set; }
    }

    public class WebNavigationEndpointData10
    {
        public string url { get; set; }
    }

    public class NavigationEndpoint6
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata13 commandMetadata { get; set; }
        public UrlEndpoint3 urlEndpoint { get; set; }
        public WebNavigationEndpointData10 webNavigationEndpointData { get; set; }
    }

    public class Thumbnail6
    {
        public string url { get; set; }
    }

    public class Icon6
    {
        public List<Thumbnail6> thumbnails { get; set; }
    }

    public class Title3
    {
        public string simpleText { get; set; }
    }

    public class PrimaryLink
    {
        public NavigationEndpoint6 navigationEndpoint { get; set; }
        public Icon6 icon { get; set; }
        public Title3 title { get; set; }
    }

    public class WebCommandMetadata14
    {
        public string url { get; set; }
    }

    public class CommandMetadata14
    {
        public WebCommandMetadata14 webCommandMetadata { get; set; }
    }

    public class UrlEndpoint4
    {
        public string url { get; set; }
        public string target { get; set; }
    }

    public class WebNavigationEndpointData11
    {
        public string url { get; set; }
    }

    public class NavigationEndpoint7
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata14 commandMetadata { get; set; }
        public UrlEndpoint4 urlEndpoint { get; set; }
        public WebNavigationEndpointData11 webNavigationEndpointData { get; set; }
    }

    public class Thumbnail7
    {
        public string url { get; set; }
    }

    public class Icon7
    {
        public List<Thumbnail7> thumbnails { get; set; }
    }

    public class Title4
    {
        public string simpleText { get; set; }
    }

    public class SecondaryLink
    {
        public NavigationEndpoint7 navigationEndpoint { get; set; }
        public Icon7 icon { get; set; }
        public Title4 title { get; set; }
    }

    public class ChannelHeaderLinksRenderer
    {
        public List<PrimaryLink> primaryLinks { get; set; }
        public List<SecondaryLink> secondaryLinks { get; set; }
        public bool hack { get; set; }
    }

    public class HeaderLinks
    {
        public ChannelHeaderLinksRenderer channelHeaderLinksRenderer { get; set; }
    }

    public class Run13
    {
        public string text { get; set; }
    }

    public class ButtonText
    {
        public List<Run13> runs { get; set; }
    }

    public class SubscriberCountText
    {
        public string simpleText { get; set; }
    }

    public class SubscriberCountWithSubscribeText
    {
        public string simpleText { get; set; }
    }

    public class Run14
    {
        public string text { get; set; }
        public bool? deemphasize { get; set; }
    }

    public class SubscribedButtonText
    {
        public List<Run14> runs { get; set; }
    }

    public class Run15
    {
        public string text { get; set; }
        public bool? deemphasize { get; set; }
    }

    public class UnsubscribedButtonText
    {
        public List<Run15> runs { get; set; }
    }

    public class Run16
    {
        public string text { get; set; }
    }

    public class UnsubscribeButtonText
    {
        public List<Run16> runs { get; set; }
    }

    public class WebCommandMetadata15
    {
        public string url { get; set; }
        public bool sendPost { get; set; }
    }

    public class CommandMetadata15
    {
        public WebCommandMetadata15 webCommandMetadata { get; set; }
    }

    public class SubscribeEndpoint
    {
        public List<string> channelIds { get; set; }
        public string @params { get; set; }
    }

    public class Run17
    {
        public string text { get; set; }
    }

    public class Title5
    {
        public List<Run17> runs { get; set; }
    }

    public class Run18
    {
        public string text { get; set; }
    }

    public class DialogMessage
    {
        public List<Run18> runs { get; set; }
    }

    public class Run19
    {
        public string text { get; set; }
    }

    public class Text5
    {
        public List<Run19> runs { get; set; }
    }

    public class WebCommandMetadata16
    {
        public string url { get; set; }
        public bool sendPost { get; set; }
    }

    public class CommandMetadata16
    {
        public WebCommandMetadata16 webCommandMetadata { get; set; }
    }

    public class UnsubscribeEndpoint
    {
        public List<string> channelIds { get; set; }
        public string @params { get; set; }
    }

    public class ServiceEndpoint3
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata16 commandMetadata { get; set; }
        public UnsubscribeEndpoint unsubscribeEndpoint { get; set; }
    }

    public class ButtonRenderer5
    {
        public string style { get; set; }
        public string size { get; set; }
        public Text5 text { get; set; }
        public ServiceEndpoint3 serviceEndpoint { get; set; }
        public string trackingParams { get; set; }
    }

    public class ConfirmButton
    {
        public ButtonRenderer5 buttonRenderer { get; set; }
    }

    public class Run20
    {
        public string text { get; set; }
    }

    public class Text6
    {
        public List<Run20> runs { get; set; }
    }

    public class ButtonRenderer6
    {
        public string style { get; set; }
        public string size { get; set; }
        public Text6 text { get; set; }
        public string trackingParams { get; set; }
    }

    public class CancelButton2
    {
        public ButtonRenderer6 buttonRenderer { get; set; }
    }

    public class ConfirmDialogRenderer
    {
        public Title5 title { get; set; }
        public string trackingParams { get; set; }
        public List<DialogMessage> dialogMessages { get; set; }
        public ConfirmButton confirmButton { get; set; }
        public CancelButton2 cancelButton { get; set; }
        public bool primaryIsCancel { get; set; }
    }

    public class Popup
    {
        public ConfirmDialogRenderer confirmDialogRenderer { get; set; }
    }

    public class OpenPopupAction
    {
        public Popup popup { get; set; }
        public string popupType { get; set; }
    }

    public class Action4
    {
        public OpenPopupAction openPopupAction { get; set; }
    }

    public class SignalServiceEndpoint
    {
        public string signal { get; set; }
        public List<Action4> actions { get; set; }
    }

    public class ServiceEndpoint2
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata15 commandMetadata { get; set; }
        public SubscribeEndpoint subscribeEndpoint { get; set; }
        public SignalServiceEndpoint signalServiceEndpoint { get; set; }
    }

    public class Run21
    {
        public string text { get; set; }
    }

    public class LongSubscriberCountText
    {
        public List<Run21> runs { get; set; }
    }

    public class Style
    {
        public string styleType { get; set; }
    }

    public class DefaultIcon
    {
        public string iconType { get; set; }
    }

    public class WebCommandMetadata17
    {
        public string url { get; set; }
        public bool sendPost { get; set; }
    }

    public class CommandMetadata17
    {
        public WebCommandMetadata17 webCommandMetadata { get; set; }
    }

    public class ModifyChannelNotificationPreferenceEndpoint
    {
        public string @params { get; set; }
    }

    public class DefaultServiceEndpoint
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata17 commandMetadata { get; set; }
        public ModifyChannelNotificationPreferenceEndpoint modifyChannelNotificationPreferenceEndpoint { get; set; }
    }

    public class ToggledIcon2
    {
        public string iconType { get; set; }
    }

    public class WebCommandMetadata18
    {
        public string url { get; set; }
        public bool sendPost { get; set; }
    }

    public class CommandMetadata18
    {
        public WebCommandMetadata18 webCommandMetadata { get; set; }
    }

    public class ModifyChannelNotificationPreferenceEndpoint2
    {
        public string @params { get; set; }
    }

    public class ToggledServiceEndpoint2
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata18 commandMetadata { get; set; }
        public ModifyChannelNotificationPreferenceEndpoint2 modifyChannelNotificationPreferenceEndpoint { get; set; }
    }

    public class ToggledStyle
    {
        public string styleType { get; set; }
    }

    public class AccessibilityData7
    {
        public string label { get; set; }
    }

    public class AccessibilityData6
    {
        public AccessibilityData7 accessibilityData { get; set; }
    }

    public class AccessibilityData8
    {
        public string label { get; set; }
    }

    public class ToggledAccessibilityData
    {
        public AccessibilityData8 accessibilityData { get; set; }
    }

    public class ToggleButtonRenderer
    {
        public Style style { get; set; }
        public bool isToggled { get; set; }
        public bool isDisabled { get; set; }
        public DefaultIcon defaultIcon { get; set; }
        public DefaultServiceEndpoint defaultServiceEndpoint { get; set; }
        public ToggledIcon2 toggledIcon { get; set; }
        public ToggledServiceEndpoint2 toggledServiceEndpoint { get; set; }
        public string trackingParams { get; set; }
        public string defaultTooltip { get; set; }
        public string toggledTooltip { get; set; }
        public ToggledStyle toggledStyle { get; set; }
        public AccessibilityData6 accessibilityData { get; set; }
        public ToggledAccessibilityData toggledAccessibilityData { get; set; }
    }

    public class NotificationPreferenceToggleButton
    {
        public ToggleButtonRenderer toggleButtonRenderer { get; set; }
    }

    public class ShortSubscriberCountText
    {
        public string simpleText { get; set; }
    }

    public class AccessibilityData9
    {
        public string label { get; set; }
    }

    public class SubscribeAccessibility
    {
        public AccessibilityData9 accessibilityData { get; set; }
    }

    public class AccessibilityData10
    {
        public string label { get; set; }
    }

    public class UnsubscribeAccessibility
    {
        public AccessibilityData10 accessibilityData { get; set; }
    }

    public class SubscribeButtonRenderer
    {
        public ButtonText buttonText { get; set; }
        public SubscriberCountText subscriberCountText { get; set; }
        public bool subscribed { get; set; }
        public bool enabled { get; set; }
        public string type { get; set; }
        public string channelId { get; set; }
        public bool showPreferences { get; set; }
        public SubscriberCountWithSubscribeText subscriberCountWithSubscribeText { get; set; }
        public SubscribedButtonText subscribedButtonText { get; set; }
        public UnsubscribedButtonText unsubscribedButtonText { get; set; }
        public string trackingParams { get; set; }
        public UnsubscribeButtonText unsubscribeButtonText { get; set; }
        public List<ServiceEndpoint2> serviceEndpoints { get; set; }
        public LongSubscriberCountText longSubscriberCountText { get; set; }
        public NotificationPreferenceToggleButton notificationPreferenceToggleButton { get; set; }
        public ShortSubscriberCountText shortSubscriberCountText { get; set; }
        public SubscribeAccessibility subscribeAccessibility { get; set; }
        public UnsubscribeAccessibility unsubscribeAccessibility { get; set; }
    }

    public class SubscribeButton
    {
        public SubscribeButtonRenderer subscribeButtonRenderer { get; set; }
    }

    public class VisitTracking
    {
        public string remarketingPing { get; set; }
    }

    public class Run22
    {
        public string text { get; set; }
    }

    public class SubscriberCountText2
    {
        public List<Run22> runs { get; set; }
    }

    public class Thumbnail8
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class WebThumbnailDetailsExtensionData4
    {
        public bool isPreloaded { get; set; }
    }

    public class TvBanner
    {
        public List<Thumbnail8> thumbnails { get; set; }
        public WebThumbnailDetailsExtensionData4 webThumbnailDetailsExtensionData { get; set; }
    }

    public class Thumbnail9
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class WebThumbnailDetailsExtensionData5
    {
        public bool isPreloaded { get; set; }
    }

    public class MobileBanner
    {
        public List<Thumbnail9> thumbnails { get; set; }
        public WebThumbnailDetailsExtensionData5 webThumbnailDetailsExtensionData { get; set; }
    }

    public class C4TabbedHeaderRenderer
    {
        public string channelId { get; set; }
        public string title { get; set; }
        public NavigationEndpoint5 navigationEndpoint { get; set; }
        public Avatar avatar { get; set; }
        public Banner banner { get; set; }
        public List<Badge2> badges { get; set; }
        public HeaderLinks headerLinks { get; set; }
        public SubscribeButton subscribeButton { get; set; }
        public VisitTracking visitTracking { get; set; }
        public SubscriberCountText2 subscriberCountText { get; set; }
        public TvBanner tvBanner { get; set; }
        public MobileBanner mobileBanner { get; set; }
        public string trackingParams { get; set; }
    }

    public class Header
    {
        public C4TabbedHeaderRenderer c4TabbedHeaderRenderer { get; set; }
    }

    public class Thumbnail10
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Avatar2
    {
        public List<Thumbnail10> thumbnails { get; set; }
    }

    public class ChannelMetadataRenderer
    {
        public string title { get; set; }
        public string description { get; set; }
        public string rssUrl { get; set; }
        public string plusPageLink { get; set; }
        public string channelConversionUrl { get; set; }
        public string externalId { get; set; }
        public string keywords { get; set; }
        public List<string> ownerUrls { get; set; }
        public bool isPaidChannel { get; set; }
        public Avatar2 avatar { get; set; }
        public string channelUrl { get; set; }
        public bool isFamilySafe { get; set; }
        public List<string> availableCountryCodes { get; set; }
        public string androidDeepLink { get; set; }
        public string androidAppindexingLink { get; set; }
        public string iosAppindexingLink { get; set; }
        public string tabPath { get; set; }
    }

    public class Metadata
    {
        public ChannelMetadataRenderer channelMetadataRenderer { get; set; }
    }

    public class IconImage
    {
        public string iconType { get; set; }
    }

    public class Run23
    {
        public string text { get; set; }
    }

    public class TooltipText
    {
        public List<Run23> runs { get; set; }
    }

    public class WebCommandMetadata19
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class CommandMetadata19
    {
        public WebCommandMetadata19 webCommandMetadata { get; set; }
    }

    public class BrowseEndpoint7
    {
        public string browseId { get; set; }
    }

    public class WebNavigationEndpointData12
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class Endpoint5
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata19 commandMetadata { get; set; }
        public BrowseEndpoint7 browseEndpoint { get; set; }
        public WebNavigationEndpointData12 webNavigationEndpointData { get; set; }
    }

    public class TopbarLogoRenderer
    {
        public IconImage iconImage { get; set; }
        public TooltipText tooltipText { get; set; }
        public Endpoint5 endpoint { get; set; }
        public string trackingParams { get; set; }
    }

    public class Logo
    {
        public TopbarLogoRenderer topbarLogoRenderer { get; set; }
    }

    public class Icon8
    {
        public string iconType { get; set; }
    }

    public class Run24
    {
        public string text { get; set; }
    }

    public class PlaceholderText
    {
        public List<Run24> runs { get; set; }
    }

    public class WebSearchboxConfig
    {
        public string requestLanguage { get; set; }
        public string requestDomain { get; set; }
        public bool hasOnscreenKeyboard { get; set; }
        public bool focusSearchbox { get; set; }
    }

    public class Config
    {
        public WebSearchboxConfig webSearchboxConfig { get; set; }
    }

    public class WebCommandMetadata20
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class CommandMetadata20
    {
        public WebCommandMetadata20 webCommandMetadata { get; set; }
    }

    public class SearchEndpoint2
    {
        public string query { get; set; }
    }

    public class WebNavigationEndpointData13
    {
        public string url { get; set; }
        public string webPageType { get; set; }
    }

    public class SearchEndpoint
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata20 commandMetadata { get; set; }
        public SearchEndpoint2 searchEndpoint { get; set; }
        public WebNavigationEndpointData13 webNavigationEndpointData { get; set; }
    }

    public class FusionSearchboxRenderer
    {
        public Icon8 icon { get; set; }
        public PlaceholderText placeholderText { get; set; }
        public Config config { get; set; }
        public string trackingParams { get; set; }
        public SearchEndpoint searchEndpoint { get; set; }
    }

    public class Searchbox
    {
        public FusionSearchboxRenderer fusionSearchboxRenderer { get; set; }
    }

    public class Icon9
    {
        public string iconType { get; set; }
    }

    public class WebCommandMetadata21
    {
        public string url { get; set; }
    }

    public class CommandMetadata21
    {
        public WebCommandMetadata21 webCommandMetadata { get; set; }
    }

    public class UploadEndpoint
    {
        public bool hack { get; set; }
    }

    public class WebNavigationEndpointData14
    {
        public string url { get; set; }
    }

    public class NavigationEndpoint8
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata21 commandMetadata { get; set; }
        public UploadEndpoint uploadEndpoint { get; set; }
        public WebNavigationEndpointData14 webNavigationEndpointData { get; set; }
    }

    public class Accessibility4
    {
        public string label { get; set; }
    }

    public class AccessibilityData12
    {
        public string label { get; set; }
    }

    public class AccessibilityData11
    {
        public AccessibilityData12 accessibilityData { get; set; }
    }

    public class ButtonRenderer7
    {
        public string style { get; set; }
        public string size { get; set; }
        public bool isDisabled { get; set; }
        public Icon9 icon { get; set; }
        public NavigationEndpoint8 navigationEndpoint { get; set; }
        public Accessibility4 accessibility { get; set; }
        public string tooltip { get; set; }
        public string trackingParams { get; set; }
        public AccessibilityData11 accessibilityData { get; set; }
    }

    public class Icon10
    {
        public string iconType { get; set; }
    }

    public class Icon11
    {
        public string iconType { get; set; }
    }

    public class Run25
    {
        public string text { get; set; }
    }

    public class Title6
    {
        public List<Run25> runs { get; set; }
    }

    public class WebCommandMetadata22
    {
        public string url { get; set; }
    }

    public class CommandMetadata22
    {
        public WebCommandMetadata22 webCommandMetadata { get; set; }
    }

    public class UrlEndpoint5
    {
        public string url { get; set; }
        public string target { get; set; }
    }

    public class WebNavigationEndpointData15
    {
        public string url { get; set; }
    }

    public class NavigationEndpoint9
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata22 commandMetadata { get; set; }
        public UrlEndpoint5 urlEndpoint { get; set; }
        public WebNavigationEndpointData15 webNavigationEndpointData { get; set; }
    }

    public class CompactLinkRenderer
    {
        public Icon11 icon { get; set; }
        public Title6 title { get; set; }
        public NavigationEndpoint9 navigationEndpoint { get; set; }
        public string trackingParams { get; set; }
    }

    public class Item2
    {
        public CompactLinkRenderer compactLinkRenderer { get; set; }
    }

    public class MultiPageMenuSectionRenderer
    {
        public List<Item2> items { get; set; }
        public string trackingParams { get; set; }
    }

    public class Section
    {
        public MultiPageMenuSectionRenderer multiPageMenuSectionRenderer { get; set; }
    }

    public class MultiPageMenuRenderer
    {
        public List<Section> sections { get; set; }
        public string trackingParams { get; set; }
    }

    public class MenuRenderer2
    {
        public MultiPageMenuRenderer multiPageMenuRenderer { get; set; }
    }

    public class AccessibilityData13
    {
        public string label { get; set; }
    }

    public class Accessibility5
    {
        public AccessibilityData13 accessibilityData { get; set; }
    }

    public class Thumbnail11
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class WebThumbnailDetailsExtensionData6
    {
        public bool excludeFromVpl { get; set; }
    }

    public class Avatar3
    {
        public List<Thumbnail11> thumbnails { get; set; }
        public WebThumbnailDetailsExtensionData6 webThumbnailDetailsExtensionData { get; set; }
    }

    public class WebCommandMetadata23
    {
        public string url { get; set; }
        public bool sendPost { get; set; }
    }

    public class CommandMetadata23
    {
        public WebCommandMetadata23 webCommandMetadata { get; set; }
    }

    public class GetAccountMenuEndpoint
    {
        public bool hack { get; set; }
    }

    public class MenuRequest
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata23 commandMetadata { get; set; }
        public GetAccountMenuEndpoint getAccountMenuEndpoint { get; set; }
    }

    public class TopbarMenuButtonRenderer
    {
        public Icon10 icon { get; set; }
        public MenuRenderer2 menuRenderer { get; set; }
        public string trackingParams { get; set; }
        public Accessibility5 accessibility { get; set; }
        public string tooltip { get; set; }
        public string style { get; set; }
        public Avatar3 avatar { get; set; }
        public MenuRequest menuRequest { get; set; }
    }

    public class Icon12
    {
        public string iconType { get; set; }
    }

    public class WebCommandMetadata24
    {
        public string url { get; set; }
        public bool sendPost { get; set; }
    }

    public class CommandMetadata24
    {
        public WebCommandMetadata24 webCommandMetadata { get; set; }
    }

    public class SignalServiceEndpoint2
    {
        public string signal { get; set; }
    }

    public class MenuRequest2
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata24 commandMetadata { get; set; }
        public SignalServiceEndpoint2 signalServiceEndpoint { get; set; }
    }

    public class AccessibilityData14
    {
        public string label { get; set; }
    }

    public class Accessibility6
    {
        public AccessibilityData14 accessibilityData { get; set; }
    }

    public class WebCommandMetadata25
    {
        public string url { get; set; }
        public bool sendPost { get; set; }
    }

    public class CommandMetadata25
    {
        public WebCommandMetadata25 webCommandMetadata { get; set; }
    }

    public class SignalServiceEndpoint3
    {
        public string signal { get; set; }
    }

    public class UpdateUnseenCountEndpoint
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata25 commandMetadata { get; set; }
        public SignalServiceEndpoint3 signalServiceEndpoint { get; set; }
    }

    public class WebCommandMetadata26
    {
        public string url { get; set; }
        public bool sendPost { get; set; }
    }

    public class CommandMetadata26
    {
        public WebCommandMetadata26 webCommandMetadata { get; set; }
    }

    public class SignalServiceEndpoint4
    {
        public string signal { get; set; }
    }

    public class GetHighPriorityNotificationEndpoint
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata26 commandMetadata { get; set; }
        public SignalServiceEndpoint4 signalServiceEndpoint { get; set; }
    }

    public class NotificationTopbarButtonRenderer
    {
        public Icon12 icon { get; set; }
        public MenuRequest2 menuRequest { get; set; }
        public string style { get; set; }
        public string trackingParams { get; set; }
        public Accessibility6 accessibility { get; set; }
        public string tooltip { get; set; }
        public UpdateUnseenCountEndpoint updateUnseenCountEndpoint { get; set; }
        public int notificationCount { get; set; }
        public List<string> handlerDatas { get; set; }
        public GetHighPriorityNotificationEndpoint getHighPriorityNotificationEndpoint { get; set; }
    }

    public class TopbarButton
    {
        public ButtonRenderer7 buttonRenderer { get; set; }
        public TopbarMenuButtonRenderer topbarMenuButtonRenderer { get; set; }
        public NotificationTopbarButtonRenderer notificationTopbarButtonRenderer { get; set; }
    }

    public class Run26
    {
        public string text { get; set; }
    }

    public class Title7
    {
        public List<Run26> runs { get; set; }
    }

    public class Run27
    {
        public string text { get; set; }
    }

    public class Title8
    {
        public List<Run27> runs { get; set; }
    }

    public class Run28
    {
        public string text { get; set; }
    }

    public class Label
    {
        public List<Run28> runs { get; set; }
    }

    public class HotkeyDialogSectionOptionRenderer
    {
        public Label label { get; set; }
        public string hotkey { get; set; }
    }

    public class Option2
    {
        public HotkeyDialogSectionOptionRenderer hotkeyDialogSectionOptionRenderer { get; set; }
    }

    public class HotkeyDialogSectionRenderer
    {
        public Title8 title { get; set; }
        public List<Option2> options { get; set; }
    }

    public class Section2
    {
        public HotkeyDialogSectionRenderer hotkeyDialogSectionRenderer { get; set; }
    }

    public class Text7
    {
        public string simpleText { get; set; }
    }

    public class ButtonRenderer8
    {
        public string style { get; set; }
        public string size { get; set; }
        public bool isDisabled { get; set; }
        public Text7 text { get; set; }
        public string trackingParams { get; set; }
    }

    public class DismissButton2
    {
        public ButtonRenderer8 buttonRenderer { get; set; }
    }

    public class HotkeyDialogRenderer
    {
        public Title7 title { get; set; }
        public List<Section2> sections { get; set; }
        public DismissButton2 dismissButton { get; set; }
    }

    public class HotkeyDialog
    {
        public HotkeyDialogRenderer hotkeyDialogRenderer { get; set; }
    }

    public class DesktopTopbarRenderer
    {
        public Logo logo { get; set; }
        public Searchbox searchbox { get; set; }
        public string trackingParams { get; set; }
        public string countryCode { get; set; }
        public List<TopbarButton> topbarButtons { get; set; }
        public HotkeyDialog hotkeyDialog { get; set; }
    }

    public class Topbar
    {
        public DesktopTopbarRenderer desktopTopbarRenderer { get; set; }
    }

    public class Thumbnail13
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Thumbnail12
    {
        public List<Thumbnail13> thumbnails { get; set; }
    }

    public class LinkAlternate
    {
        public string hrefUrl { get; set; }
    }

    public class MicroformatDataRenderer
    {
        public string urlCanonical { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Thumbnail12 thumbnail { get; set; }
        public string siteName { get; set; }
        public string appName { get; set; }
        public string androidPackage { get; set; }
        public string iosAppStoreId { get; set; }
        public string iosAppArguments { get; set; }
        public string ogType { get; set; }
        public string urlApplinksWeb { get; set; }
        public string urlApplinksIos { get; set; }
        public string urlApplinksAndroid { get; set; }
        public string urlTwitterIos { get; set; }
        public string urlTwitterAndroid { get; set; }
        public string twitterCardType { get; set; }
        public string twitterSiteHandle { get; set; }
        public string schemaDotOrgType { get; set; }
        public bool noindex { get; set; }
        public bool unlisted { get; set; }
        public List<string> tags { get; set; }
        public List<LinkAlternate> linkAlternates { get; set; }
    }

    public class Microformat
    {
        public MicroformatDataRenderer microformatDataRenderer { get; set; }
    }

    public class RootObject
    {
        public ResponseContext responseContext { get; set; }
        public Contents contents { get; set; }
        public Header header { get; set; }
        public Metadata metadata { get; set; }
        public string trackingParams { get; set; }
        public Topbar topbar { get; set; }
        public Microformat microformat { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}
