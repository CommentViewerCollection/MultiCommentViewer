using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeLiveSitePlugin.Low.GetLiveChat
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

    public class Run10
    {
        public string text { get; set; }
    }

    public class Message
    {
        public List<Run10> runs { get; set; }
    }

    public class AuthorName
    {
        public string simpleText { get; set; }
    }

    public class Thumbnail
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class AuthorPhoto
    {
        public List<Thumbnail> thumbnails { get; set; }
    }

    public class LiveChatItemContextMenuEndpoint
    {
        public string @params { get; set; }
    }

    public class ContextMenuEndpoint
    {
        public LiveChatItemContextMenuEndpoint liveChatItemContextMenuEndpoint { get; set; }
    }

    public class AccessibilityData
    {
        public string label { get; set; }
    }

    public class ContextMenuAccessibility
    {
        public AccessibilityData accessibilityData { get; set; }
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
    public class LiveChatAuthorBadgeRenderer
    {
        public CustomThumbnail customThumbnail { get; set; }
        public string tooltip { get; set; }
        public Accessibility2 accessibility { get; set; }
    }
    public class AuthorBadge
    {
        public LiveChatAuthorBadgeRenderer liveChatAuthorBadgeRenderer { get; set; }
    }
    public class LiveChatPaidMessageRenderer
    {
        public string id { get; set; }
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

    public class Action
    {
        public AddChatItemAction addChatItemAction { get; set; }
    }

    public class LiveChatContinuation
    {
        public List<Continuation> continuations { get; set; }
        public List<Action> actions { get; set; }
        public string trackingParams { get; set; }
    }

    public class ContinuationContents
    {
        public LiveChatContinuation liveChatContinuation { get; set; }
    }

    public class Response
    {
        public ResponseContext responseContext { get; set; }
        public ContinuationContents continuationContents { get; set; }
        public string trackingParams { get; set; }
    }

    public class Info
    {
        public int st { get; set; }
    }

    public class Timing
    {
        public Info info { get; set; }
    }

    public class RootObject
    {
        public Response response { get; set; }
        public string csn { get; set; }
        public string url { get; set; }
        public string xsrf_token { get; set; }
        public Timing timing { get; set; }
    }
}
