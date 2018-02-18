using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeLiveSitePlugin.Low.LiveChat
{
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

    public class YtConfigData
    {
        public string csn { get; set; }
        public string visitorData { get; set; }
        public int sessionIndex { get; set; }
        public string delegatedSessionId { get; set; }
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
        public YtConfigData ytConfigData { get; set; }
        public FeedbackDialog feedbackDialog { get; set; }
    }

    public class ResponseContext
    {
        public List<ServiceTrackingParam> serviceTrackingParams { get; set; }
        public WebResponseContextExtensionData webResponseContextExtensionData { get; set; }
    }
    public class InvalidationId
    {
        public int objectSource { get; set; }
        public string protoCreationTimestampMs { get; set; }
        public string objectId { get; set; }
    }
    public class InvalidationContinuationData
    {
        public InvalidationId invalidationId { get; set; }
        public int timeoutMs { get; set; }
        public string continuation { get; set; }
        public string clickTrackingParams { get; set; }
    }
    public class TimedContinuationData
    {
        public int timeoutMs { get; set; }
        public string continuation { get; set; }
        public string clickTrackingParams { get; set; }
    }

    public class Continuation
    {
        public TimedContinuationData timedContinuationData { get; set; }
        public InvalidationContinuationData invalidationContinuationData { get; set; }
    }

    public class Thumbnail
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class AccessibilityData
    {
        public string label { get; set; }
    }

    public class Accessibility
    {
        public AccessibilityData accessibilityData { get; set; }
    }

    public class Image
    {
        public List<Thumbnail> thumbnails { get; set; }
        public Accessibility accessibility { get; set; }
    }

    public class Emoji
    {
        public string emojiId { get; set; }
        public List<string> shortcuts { get; set; }
        public List<string> searchTerms { get; set; }
        public Image image { get; set; }
        public bool isCustomEmoji { get; set; }
    }

    public class WebCommandMetadata3
    {
        public string url { get; set; }
    }

    public class CommandMetadata3
    {
        public WebCommandMetadata3 webCommandMetadata { get; set; }
    }

    public class UrlEndpoint3
    {
        public string url { get; set; }
        public string target { get; set; }
    }

    public class WebNavigationEndpointData3
    {
        public string url { get; set; }
    }

    public class NavigationEndpoint3
    {
        public string clickTrackingParams { get; set; }
        public CommandMetadata3 commandMetadata { get; set; }
        public UrlEndpoint3 urlEndpoint { get; set; }
        public WebNavigationEndpointData3 webNavigationEndpointData { get; set; }
    }

    public class Run10
    {
        public string text { get; set; }
        public Emoji emoji { get; set; }
        public NavigationEndpoint3 navigationEndpoint { get; set; }
    }

    public class Message
    {
        public List<Run10> runs { get; set; }
    }

    public class AuthorName
    {
        public string simpleText { get; set; }
    }

    public class Thumbnail2
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class AuthorPhoto
    {
        public List<Thumbnail2> thumbnails { get; set; }
    }

    public class LiveChatItemContextMenuEndpoint
    {
        public string @params { get; set; }
    }

    public class ContextMenuEndpoint
    {
        public LiveChatItemContextMenuEndpoint liveChatItemContextMenuEndpoint { get; set; }
    }

    public class Thumbnail3
    {
        public string url { get; set; }
    }

    public class CustomThumbnail
    {
        public List<Thumbnail3> thumbnails { get; set; }
    }

    public class AccessibilityData2
    {
        public string label { get; set; }
    }

    public class Accessibility2
    {
        public AccessibilityData2 accessibilityData { get; set; }
    }

    public class Icon2
    {
        public string iconType { get; set; }
    }

    public class LiveChatAuthorBadgeRenderer
    {
        public CustomThumbnail customThumbnail { get; set; }
        public string tooltip { get; set; }
        public Accessibility2 accessibility { get; set; }
        public Icon2 icon { get; set; }
    }

    public class AuthorBadge
    {
        public LiveChatAuthorBadgeRenderer liveChatAuthorBadgeRenderer { get; set; }
    }

    public class AccessibilityData3
    {
        public string label { get; set; }
    }

    public class ContextMenuAccessibility
    {
        public AccessibilityData3 accessibilityData { get; set; }
    }
    public class LiveChatPaidMessageRenderer
    {
        public string id { get; set; }
        public Accessibility contextMenuAccessibility { get; set; }
        public long timestampColor { get; set; }
        public ContextMenuEndpoint contextMenuEndpoint { get; set; }
        public long headerTextColor { get; set; }
        public string timestampUsec { get; set; }
        public List<AuthorBadge> authorBadges { get; set; }
        public Text purchaseAmountText { get; set; }
        public string authorExternalChannelId { get; set; }
        public AuthorPhoto authorPhoto { get; set; }
        public long headerBackgroundColor { get; set; }
        public long bodyBackgroundColor { get; set; }
        public long authorNameTextColor { get; set; }
        public Text authorName { get; set; }
        public long bodyTextColor { get; set; }
        public Text message { get; set; }
    }
    public class LiveChatTextMessageRenderer
    {
        public Message message { get; set; }
        public AuthorName authorName { get; set; }
        public AuthorPhoto authorPhoto { get; set; }
        public ContextMenuEndpoint contextMenuEndpoint { get; set; }
        public string id { get; set; }
        public string timestampUsec { get; set; }
        public List<AuthorBadge> authorBadges { get; set; }
        public string authorExternalChannelId { get; set; }
        public ContextMenuAccessibility contextMenuAccessibility { get; set; }
    }

    public class Item
    {
        public LiveChatTextMessageRenderer liveChatTextMessageRenderer { get; set; }
        public LiveChatPaidMessageRenderer liveChatPaidMessageRenderer { get; set; }
    }

    public class AddChatItemAction
    {
        public Item item { get; set; }
        public string clientId { get; set; }
    }

    public class Run11
    {
        public string text { get; set; }
    }

    public class DeletedStateMessage
    {
        public List<Run11> runs { get; set; }
    }

    public class MarkChatItemAsDeletedAction
    {
        public DeletedStateMessage deletedStateMessage { get; set; }
        public string targetItemId { get; set; }
    }

    public class Action
    {
        public AddChatItemAction addChatItemAction { get; set; }
        public MarkChatItemAsDeletedAction markChatItemAsDeletedAction { get; set; }
    }

    public class Run12
    {
        public string text { get; set; }
    }

    public class Message2
    {
        public List<Run12> runs { get; set; }
    }

    public class Icon3
    {
        public string iconType { get; set; }
    }

    public class AccessibilityData4
    {
        public string label { get; set; }
    }

    public class Accessibility3
    {
        public AccessibilityData4 accessibilityData { get; set; }
    }

    public class LiveChatIconToggleButtonRenderer
    {
        public string targetId { get; set; }
        public Icon3 icon { get; set; }
        public string tooltip { get; set; }
        public Accessibility3 accessibility { get; set; }
    }

    public class Button
    {
        public LiveChatIconToggleButtonRenderer liveChatIconToggleButtonRenderer { get; set; }
    }

    public class Text4
    {
        public string simpleText { get; set; }
    }

    public class LiveChatPurchaseMessageEndpoint
    {
        public string @params { get; set; }
    }

    public class NavigationEndpoint4
    {
        public LiveChatPurchaseMessageEndpoint liveChatPurchaseMessageEndpoint { get; set; }
    }

    public class Accessibility4
    {
        public string label { get; set; }
    }

    public class ButtonRenderer5
    {
        public string style { get; set; }
        public string size { get; set; }
        public bool isDisabled { get; set; }
        public Text4 text { get; set; }
        public NavigationEndpoint4 navigationEndpoint { get; set; }
        public Accessibility4 accessibility { get; set; }
        public string trackingParams { get; set; }
    }

    public class Button2
    {
        public ButtonRenderer5 buttonRenderer { get; set; }
    }

    public class Text5
    {
        public string simpleText { get; set; }
    }

    public class Accessibility5
    {
        public string label { get; set; }
    }

    public class ButtonRenderer6
    {
        public string style { get; set; }
        public string size { get; set; }
        public bool isDisabled { get; set; }
        public Text5 text { get; set; }
        public Accessibility5 accessibility { get; set; }
        public string trackingParams { get; set; }
    }

    public class CancelButton2
    {
        public ButtonRenderer6 buttonRenderer { get; set; }
    }

    public class LiveChatCreatorSupportRenderer
    {
        public string id { get; set; }
        public List<Button2> buttons { get; set; }
        public CancelButton2 cancelButton { get; set; }
    }

    public class Panel
    {
        public LiveChatCreatorSupportRenderer liveChatCreatorSupportRenderer { get; set; }
    }

    public class LiveChatRestrictedParticipationRenderer
    {
        public Message2 message { get; set; }
        public List<Button> buttons { get; set; }
        public List<Panel> panels { get; set; }
    }

    public class ActionPanel
    {
        public LiveChatRestrictedParticipationRenderer liveChatRestrictedParticipationRenderer { get; set; }
    }

    public class Icon4
    {
        public string iconType { get; set; }
    }

    public class AccessibilityData6
    {
        public string label { get; set; }
    }

    public class AccessibilityData5
    {
        public AccessibilityData6 accessibilityData { get; set; }
    }

    public class ButtonRenderer7
    {
        public string style { get; set; }
        public Icon4 icon { get; set; }
        public string trackingParams { get; set; }
        public AccessibilityData5 accessibilityData { get; set; }
    }

    public class MoreCommentsBelowButton
    {
        public ButtonRenderer7 buttonRenderer { get; set; }
    }

    public class LiveChatItemListRenderer
    {
        public int maxItemsToDisplay { get; set; }
        public MoreCommentsBelowButton moreCommentsBelowButton { get; set; }
        public bool enablePauseChatKeyboardShortcuts { get; set; }
    }

    public class ItemList
    {
        public LiveChatItemListRenderer liveChatItemListRenderer { get; set; }
    }

    public class Run13
    {
        public string text { get; set; }
    }

    public class Text6
    {
        public List<Run13> runs { get; set; }
    }

    public class Icon5
    {
        public string iconType { get; set; }
    }

    public class ShowLiveChatParticipantsEndpoint
    {
        public bool hack { get; set; }
    }

    public class ToggleLiveChatTimestampsEndpoint
    {
        public bool hack { get; set; }
    }

    public class ServiceEndpoint
    {
        public ShowLiveChatParticipantsEndpoint showLiveChatParticipantsEndpoint { get; set; }
        public ToggleLiveChatTimestampsEndpoint toggleLiveChatTimestampsEndpoint { get; set; }
    }

    public class MenuServiceItemRenderer
    {
        public Text6 text { get; set; }
        public Icon5 icon { get; set; }
        public ServiceEndpoint serviceEndpoint { get; set; }
        public string trackingParams { get; set; }
    }

    public class Run14
    {
        public string text { get; set; }
    }

    public class Text7
    {
        public List<Run14> runs { get; set; }
    }

    public class Icon6
    {
        public string iconType { get; set; }
    }

    public class UserFeedbackEndpoint
    {
        public bool hack { get; set; }
        public string bucketIdentifier { get; set; }
    }

    public class NavigationEndpoint5
    {
        public UserFeedbackEndpoint userFeedbackEndpoint { get; set; }
    }

    public class MenuNavigationItemRenderer
    {
        public Text7 text { get; set; }
        public Icon6 icon { get; set; }
        public NavigationEndpoint5 navigationEndpoint { get; set; }
        public string trackingParams { get; set; }
    }

    public class Item2
    {
        public MenuServiceItemRenderer menuServiceItemRenderer { get; set; }
        public MenuNavigationItemRenderer menuNavigationItemRenderer { get; set; }
    }

    public class AccessibilityData7
    {
        public string label { get; set; }
    }

    public class Accessibility6
    {
        public AccessibilityData7 accessibilityData { get; set; }
    }

    public class MenuRenderer
    {
        public List<Item2> items { get; set; }
        public string trackingParams { get; set; }
        public Accessibility6 accessibility { get; set; }
    }

    public class OverflowMenu
    {
        public MenuRenderer menuRenderer { get; set; }
    }

    public class Accessibility7
    {
        public string label { get; set; }
    }

    public class ButtonRenderer8
    {
        public string style { get; set; }
        public string size { get; set; }
        public bool isDisabled { get; set; }
        public Accessibility7 accessibility { get; set; }
        public string trackingParams { get; set; }
    }

    public class CollapseButton
    {
        public ButtonRenderer8 buttonRenderer { get; set; }
    }

    public class ReloadContinuationData
    {
        public string continuation { get; set; }
        public string clickTrackingParams { get; set; }
    }

    public class Continuation2
    {
        public ReloadContinuationData reloadContinuationData { get; set; }
    }

    public class SubMenuItem
    {
        public string title { get; set; }
        public bool selected { get; set; }
        public Continuation2 continuation { get; set; }
        public string subtitle { get; set; }
    }

    public class SortFilterSubMenuRenderer
    {
        public List<SubMenuItem> subMenuItems { get; set; }
    }

    public class ViewSelector
    {
        public SortFilterSubMenuRenderer sortFilterSubMenuRenderer { get; set; }
    }

    public class LiveChatHeaderRenderer
    {
        public OverflowMenu overflowMenu { get; set; }
        public CollapseButton collapseButton { get; set; }
        public ViewSelector viewSelector { get; set; }
    }

    public class Header
    {
        public LiveChatHeaderRenderer liveChatHeaderRenderer { get; set; }
    }

    public class LiveChatTickerRenderer
    {
        public bool sentinel { get; set; }
    }

    public class Ticker
    {
        public LiveChatTickerRenderer liveChatTickerRenderer { get; set; }
    }

    public class Run15
    {
        public string text { get; set; }
    }

    public class Title2
    {
        public List<Run15> runs { get; set; }
    }

    public class Icon7
    {
        public string iconType { get; set; }
    }

    public class AccessibilityData9
    {
        public string label { get; set; }
    }

    public class AccessibilityData8
    {
        public AccessibilityData9 accessibilityData { get; set; }
    }

    public class ButtonRenderer9
    {
        public Icon7 icon { get; set; }
        public string trackingParams { get; set; }
        public AccessibilityData8 accessibilityData { get; set; }
    }

    public class BackButton
    {
        public ButtonRenderer9 buttonRenderer { get; set; }
    }

    public class AuthorName2
    {
        public string simpleText { get; set; }
    }

    public class Thumbnail4
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class AuthorPhoto2
    {
        public List<Thumbnail4> thumbnails { get; set; }
    }

    public class Icon8
    {
        public string iconType { get; set; }
    }

    public class AccessibilityData10
    {
        public string label { get; set; }
    }

    public class Accessibility8
    {
        public AccessibilityData10 accessibilityData { get; set; }
    }

    public class LiveChatAuthorBadgeRenderer2
    {
        public Icon8 icon { get; set; }
        public string tooltip { get; set; }
        public Accessibility8 accessibility { get; set; }
    }

    public class AuthorBadge2
    {
        public LiveChatAuthorBadgeRenderer2 liveChatAuthorBadgeRenderer { get; set; }
    }

    public class LiveChatParticipantRenderer
    {
        public AuthorName2 authorName { get; set; }
        public AuthorPhoto2 authorPhoto { get; set; }
        public List<AuthorBadge2> authorBadges { get; set; }
    }

    public class Participant
    {
        public LiveChatParticipantRenderer liveChatParticipantRenderer { get; set; }
    }

    public class LiveChatParticipantsListRenderer
    {
        public Title2 title { get; set; }
        public BackButton backButton { get; set; }
        public List<Participant> participants { get; set; }
    }

    public class ParticipantsList
    {
        public LiveChatParticipantsListRenderer liveChatParticipantsListRenderer { get; set; }
    }

    public class Thumbnail5
    {
        public string url { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
    }

    public class AccessibilityData11
    {
        public string label { get; set; }
    }

    public class Accessibility9
    {
        public AccessibilityData11 accessibilityData { get; set; }
    }

    public class Image2
    {
        public List<Thumbnail5> thumbnails { get; set; }
        public Accessibility9 accessibility { get; set; }
    }

    public class Emoji2
    {
        public string emojiId { get; set; }
        public List<string> shortcuts { get; set; }
        public List<string> searchTerms { get; set; }
        public Image2 image { get; set; }
        public bool? supportsSkinTone { get; set; }
        public bool? isCustomEmoji { get; set; }
    }

    public class Run16
    {
        public string text { get; set; }
    }

    public class ReconnectMessage
    {
        public List<Run16> runs { get; set; }
    }

    public class Run17
    {
        public string text { get; set; }
    }

    public class UnableToReconnectMessage
    {
        public List<Run17> runs { get; set; }
    }

    public class Run18
    {
        public string text { get; set; }
    }

    public class FatalError
    {
        public List<Run18> runs { get; set; }
    }

    public class Run19
    {
        public string text { get; set; }
    }

    public class ReconnectedMessage
    {
        public List<Run19> runs { get; set; }
    }

    public class Run20
    {
        public string text { get; set; }
    }

    public class GenericError
    {
        public List<Run20> runs { get; set; }
    }

    public class ClientMessages
    {
        public ReconnectMessage reconnectMessage { get; set; }
        public UnableToReconnectMessage unableToReconnectMessage { get; set; }
        public FatalError fatalError { get; set; }
        public ReconnectedMessage reconnectedMessage { get; set; }
        public GenericError genericError { get; set; }
    }

    public class LiveChatRenderer
    {
        public List<Continuation> continuations { get; set; }
        public List<Action> actions { get; set; }
        public ActionPanel actionPanel { get; set; }
        public ItemList itemList { get; set; }
        public Header header { get; set; }
        public Ticker ticker { get; set; }
        public string trackingParams { get; set; }
        public ParticipantsList participantsList { get; set; }
        public List<Emoji2> emojis { get; set; }
        public ClientMessages clientMessages { get; set; }
        public string viewerName { get; set; }
    }

    public class Contents
    {
        public LiveChatRenderer liveChatRenderer { get; set; }
    }

    public class RootObject
    {
        public ResponseContext responseContext { get; set; }
        public Contents contents { get; set; }
        public string trackingParams { get; set; }
    }
}
