using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ryu_s.BrowserCookie;
using SitePlugin;
using System.Threading;
namespace YouTubeLiveSitePlugin.Test
{
    public class YouTubeLiveCommentProvider : IYouTubeCommentProvider
    {
        private bool _canConnect;
        public bool CanConnect
        {
            get { return _canConnect; }
            private set
            {
                _canConnect = value;
                CanConnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool _canDisconnect;
        public bool CanDisconnect
        {
            get { return _canDisconnect; }
            private set
            {
                _canDisconnect = value;
                CanDisconnectChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<List<ICommentViewModel>> CommentsReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;
        private CancellationTokenSource _cts;
        public async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            _cts = new CancellationTokenSource();
            CanConnect = false;
            CanDisconnect = true;
            try
            {
                var vid = await GetVid(input);


            }
            finally
            {
                CanConnect = true;
                CanDisconnect = false;
            }
        }

        public Task<string> GetVid(string input)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }

        public List<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        public Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }
    }
    public interface IGetLiveChatResponse
    {
        IContinuationData ContinuationData { get; }
        string Url { get; }
    }
    public class GetLiveChatResponse : IGetLiveChatResponse
    {
        public IContinuationData ContinuationData { get; set; }
        public string Url { get; set; }
    }
    public interface IYtInitialData
    {
        IContinuationData ContinuationData { get; }
    }
    public interface IContinuationData
    {
        long TimeoutMs { get; }
        string Continuation { get; }
        string ClickTrackingParams { get; }
    }
    public interface IInvalidationContinuationData : IContinuationData
    {
        long ObjectSource { get; }
        string ObjectId { get; }
        long ProtoCreationTimestampMs { get; }
    }
    public interface ITimedContinuationData : IContinuationData { }

    public class Photo
    {
        public string Url { get; }
        public int Width { get; }
        public int Height { get; }
        public Photo(GetLiveChatLow.Thumbnail low)
        {
            Url = low.url;
            Width = low.width;
            Height = low.height;
        }
    }
    public class TextMessage
    {
        public string Id { get; }
        public string Message { get; }
        public string Name { get; }
        public Photo AuthorPhoto { get; }
        /// <summary>
        /// 投稿日時（マイクロ秒）
        /// </summary>
        public long TimestampUsec { get; }
        public TextMessage(GetLiveChatLow.LiveChatTextMessageRenderer renderer)
        {
            Id = renderer.id;
            Message = string.Join("", renderer.message.runs.Select(r => r.text));
            TimestampUsec = long.Parse(renderer.timestampUsec);
            Name = renderer.authorName.simpleText;
            var exChannelId = renderer.authorExternalChannelId;
            AuthorPhoto = new Photo(renderer.authorPhoto.thumbnails[0]);
        }
    }
    public class PaidMessage
    {
        public string Id { get; }
        public string Message { get; }
        public string Name { get; }
        public Photo AuthorPhoto { get; }
        /// <summary>
        /// 投稿日時（マイクロ秒）
        /// </summary>
        public long TimestampUsec { get; }
        public long HeaderBackColor { get; }
        public long HeaderTextColor { get; }
        public long BodyBackColor { get; }
        public long BodyTextColor { get; }
        public string AuthorChannelId { get; }
        public string PurchaseAmount { get; }
        public PaidMessage(GetLiveChatLow.LiveChatPaidMessageRenderer renderer)
        {
            Id = renderer.id;
            Message = string.Join("", renderer.message.runs.Select(r => r.text));
            TimestampUsec = long.Parse(renderer.timestampUsec);
            Name = renderer.authorName.ToString();
            var exChannelId = renderer.authorExternalChannelId;
            AuthorPhoto = new Photo(renderer.authorPhoto.thumbnails[0]);
            AuthorChannelId = renderer.authorExternalChannelId;

            HeaderBackColor = renderer.headerBackgroundColor;
            HeaderTextColor = renderer.headerTextColor;
            BodyBackColor = renderer.bodyBackgroundColor;
            BodyTextColor = renderer.bodyTextColor;
            PurchaseAmount = renderer.purchaseAmountText.ToString();
        }
    }

    public class YtInitialData : IYtInitialData
    {
        public IContinuationData ContinuationData { get; set; }
    }
    public class InvalidContinuationData : IInvalidationContinuationData
    {
        public long ObjectSource { get; set; }

        public string ObjectId { get; set; }

        public long ProtoCreationTimestampMs { get; set; }

        public long TimeoutMs { get; set; }

        public string Continuation { get; set; }

        public string ClickTrackingParams { get; set; }
    }
    public class TimedContinualtionData : ITimedContinuationData
    {
        public long TimeoutMs { get; set; }

        public string Continuation { get; set; }

        public string ClickTrackingParams { get; set; }
    }
    public static class Tests
    {
        public static TextMessage ParseAddChatItemAction(string json)
        {
            var lowObject = JsonConvert.DeserializeObject<GetLiveChatLow.LiveChatTextMessageRenderer>(json,new JsonSerializerSettings
            {
                
                 Error= (s, e) =>
                 {
                     e.ErrorContext.Handled = true;
                 },
            });
            return new TextMessage(lowObject);
        }
        public static PaidMessage ParsePaidMessage(string json)
        {
            var lowObject = JsonConvert.DeserializeObject<GetLiveChatLow.LiveChatPaidMessageRenderer>(json);
            return new PaidMessage(lowObject);
        }
        public static IGetLiveChatResponse ParseGetLiveChatResponse(string json)
        {
            var res = new GetLiveChatResponse();
            var lowObject = JsonConvert.DeserializeObject<GetLiveChatLow.RootObject>(json);
            res.Url = lowObject.url;
            var continuations = lowObject.response.continuationContents.liveChatContinuation.continuations;
            if (continuations == null || continuations.Count == 0)
            {
                throw new Exception($"continuationsがnull？？そんな状況見たこと無い。get_live_chat={json}");
            }
            if (continuations[0].invalidationContinuationData != null)
            {
                var icd = continuations[0].invalidationContinuationData;
                res.ContinuationData = new InvalidContinuationData
                {
                    ClickTrackingParams = icd.clickTrackingParams,
                    Continuation = icd.continuation,
                    ObjectId = icd.invalidationId.objectId,
                    ObjectSource = icd.invalidationId.objectSource,
                    ProtoCreationTimestampMs = long.Parse(icd.invalidationId.protoCreationTimestampMs),
                    TimeoutMs = icd.timeoutMs,
                };
            }
            else if (continuations[0].timedContinuationData != null)
            {
                var tcd = continuations[0].timedContinuationData;
                res.ContinuationData = new TimedContinualtionData
                {
                    ClickTrackingParams = tcd.clickTrackingParams,
                    Continuation = tcd.continuation,
                    TimeoutMs = tcd.timeoutMs,
                };
            }
            else
            {
                throw new Exception("第3のcontinuationがあったとしてもデシリアライズするオブジェクトが無いからここには来ない。");
            }
            var actions = lowObject.response.continuationContents.liveChatContinuation.actions;
            
            return res;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ytInitialDataStr">https://www.youtube.com/live_chat?v=bYz14PKovHg&is_popout=1 のレスポンス内にあるYtInitialDataというJSON</param>
        /// <returns></returns>
        public static IYtInitialData Parse(string ytInitialDataStr)
        {
            var ytInitialData = new YtInitialData();
            var lowObject = JsonConvert.DeserializeObject<YtInitialDataLow.RootObject>(ytInitialDataStr);
            var continuations = lowObject.contents.liveChatRenderer.continuations;
            if (continuations == null || continuations.Count == 0)
            {
                throw new Exception($"continuationsがnull？？そんな状況見たこと無い。ytInitialDataStr={ytInitialDataStr}");
            }
            if (continuations[0].invalidationContinuationData != null)
            {
                var icd = continuations[0].invalidationContinuationData;
                ytInitialData.ContinuationData = new InvalidContinuationData
                {
                    ClickTrackingParams = icd.clickTrackingParams,
                    Continuation = icd.continuation,
                    ObjectId = icd.invalidationId.objectId,
                    ObjectSource = icd.invalidationId.objectSource,
                    ProtoCreationTimestampMs = long.Parse(icd.invalidationId.protoCreationTimestampMs),
                    TimeoutMs = icd.timeoutMs,
                };
            }
            else if (continuations[0].timedContinuationData != null)
            {
                var tcd = continuations[0].timedContinuationData;
                ytInitialData.ContinuationData = new TimedContinualtionData
                {
                    ClickTrackingParams = tcd.clickTrackingParams,
                    Continuation = tcd.continuation,
                    TimeoutMs = tcd.timeoutMs,
                };
            }
            else
            {
                throw new Exception("第3のcontinuationがあったとしてもデシリアライズするオブジェクトが無いからここには来ない。");
            }
            return ytInitialData;
        }
    }

    /// <summary>
    /// https://www.youtube.com/live_chat/get_live_chat?continuation=... のレスポンスをデシリアライズするためのクラス群
    /// </summary>
    namespace GetLiveChatLow
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
            public string objectId { get; set; }
            public string protoCreationTimestampMs { get; set; }
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
            public string clickTrackingParams { get; set; }
            public string continuation { get; set; }
            public int timeoutMs { get; set; }
        }

        public class Continuation
        {
            public InvalidationContinuationData invalidationContinuationData { get; set; }
            public TimedContinuationData timedContinuationData { get; set; }
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
            public List<Run> runs { get; set; }
            public override string ToString()
            {
                if(runs != null)
                {
                    return string.Join("", runs.Select(r => r.text));
                }
                else
                {
                    return simpleText;
                }
            }
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
        public class PurchaseAmountText
        {
            public List<Run> runs { get; set; }
            public override string ToString()
            {
                if(runs != null)
                {
                    return string.Join("", runs.Select(r => r.text));
                }
                else
                {
                    return "";
                }
            }
        }
        public class LiveChatTextMessageRenderer
        {
            public Message message { get; set; }
            public AuthorName authorName { get; set; }
            public AuthorPhoto authorPhoto { get; set; }
            public ContextMenuEndpoint contextMenuEndpoint { get; set; }
            public string id { get; set; }
            public string timestampUsec { get; set; }
            public string authorExternalChannelId { get; set; }
            public ContextMenuAccessibility contextMenuAccessibility { get; set; }
        }
        public class LiveChatPaidMessageRenderer
        {
            public string id { get; set; }
            public string timestampUsec { get; set; }
            public AuthorName authorName { get; set; }
            public AuthorPhoto authorPhoto { get; set; }
            public PurchaseAmountText purchaseAmountText { get; set; }
            public long headerBackgroundColor { get; set; }
            public long headerTextColor { get; set; }
            public long bodyBackgroundColor { get; set; }
            public long bodyTextColor { get; set; }
            public string authorExternalChannelId { get; set; }
            public long authorNameTextColor { get; set; }
            public ContextMenuEndpoint contextMenuEndpoint { get; set; }
            public long timestampColor { get; set; }
            public ContextMenuAccessibility contextMenuAccessibility { get; set; }
            public Message message { get; set; }
        }
        public class Item
        {
            public LiveChatPaidMessageRenderer liveChatPaidMessageRenderer { get; set; }
            public LiveChatTextMessageRenderer liveChatTextMessageRenderer { get; set; }
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
            public string xsrf_token { get; set; }
            public Response response { get; set; }
            public Timing timing { get; set; }
            public string csn { get; set; }
            public string url { get; set; }
        }
    }
    /// <summary>
    /// https://www.youtube.com/live_chat?v=bYz14PKovHg&is_popout=1 のレスポンス中にあるYtInitialDataというJSONをデシリアライズするためのクラス群
    /// </summary>
    namespace YtInitialDataLow
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
            public string objectId { get; set; }
            public string protoCreationTimestampMs { get; set; }
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
            public string clickTrackingParams { get; set; }
            public string continuation { get; set; }
            public int timeoutMs { get; set; }
        }

        public class Continuation
        {
            public InvalidationContinuationData invalidationContinuationData { get; set; }
            public TimedContinuationData timedContinuationData { get; set; }
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

        public class Thumbnail2
        {
            public string url { get; set; }
        }

        public class CustomThumbnail
        {
            public List<Thumbnail2> thumbnails { get; set; }
        }

        public class AccessibilityData2
        {
            public string label { get; set; }
        }

        public class Accessibility
        {
            public AccessibilityData2 accessibilityData { get; set; }
        }

        public class LiveChatAuthorBadgeRenderer
        {
            public CustomThumbnail customThumbnail { get; set; }
            public string tooltip { get; set; }
            public Accessibility accessibility { get; set; }
        }

        public class AuthorBadge
        {
            public LiveChatAuthorBadgeRenderer liveChatAuthorBadgeRenderer { get; set; }
        }

        public class LiveChatTextMessageRenderer
        {
            public Message message { get; set; }
            public AuthorName authorName { get; set; }
            public AuthorPhoto authorPhoto { get; set; }
            public ContextMenuEndpoint contextMenuEndpoint { get; set; }
            public string id { get; set; }
            public string timestampUsec { get; set; }
            public string authorExternalChannelId { get; set; }
            public ContextMenuAccessibility contextMenuAccessibility { get; set; }
            public List<AuthorBadge> authorBadges { get; set; }
        }

        public class Item
        {
            public LiveChatTextMessageRenderer liveChatTextMessageRenderer { get; set; }
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

        public class Run11
        {
            public string text { get; set; }
        }

        public class Placeholder
        {
            public List<Run11> runs { get; set; }
        }

        public class LiveChatTextInputFieldRenderer
        {
            public Placeholder placeholder { get; set; }
            public int maxCharacterLimit { get; set; }
            public int emojiCharacterCount { get; set; }
        }

        public class InputField
        {
            public LiveChatTextInputFieldRenderer liveChatTextInputFieldRenderer { get; set; }
        }

        public class Icon2
        {
            public string iconType { get; set; }
        }

        public class Accessibility2
        {
            public string label { get; set; }
        }

        public class ButtonRenderer5
        {
            public Icon2 icon { get; set; }
            public Accessibility2 accessibility { get; set; }
            public string trackingParams { get; set; }
        }

        public class SendButton
        {
            public ButtonRenderer5 buttonRenderer { get; set; }
        }

        public class Title2
        {
            public string simpleText { get; set; }
        }

        public class EmojiPickerCategoryRenderer
        {
            public string categoryId { get; set; }
            public Title2 title { get; set; }
            public List<string> emojiIds { get; set; }
        }

        public class Category
        {
            public EmojiPickerCategoryRenderer emojiPickerCategoryRenderer { get; set; }
        }

        public class Icon3
        {
            public string iconType { get; set; }
        }

        public class AccessibilityData3
        {
            public string label { get; set; }
        }

        public class Accessibility3
        {
            public AccessibilityData3 accessibilityData { get; set; }
        }

        public class EmojiPickerCategoryButtonRenderer
        {
            public string categoryId { get; set; }
            public Icon3 icon { get; set; }
            public string tooltip { get; set; }
            public Accessibility3 accessibility { get; set; }
        }

        public class CategoryButton
        {
            public EmojiPickerCategoryButtonRenderer emojiPickerCategoryButtonRenderer { get; set; }
        }

        public class Run12
        {
            public string text { get; set; }
        }

        public class SearchPlaceholderText
        {
            public List<Run12> runs { get; set; }
        }

        public class Run13
        {
            public string text { get; set; }
        }

        public class SearchNoResultsText
        {
            public List<Run13> runs { get; set; }
        }

        public class Run14
        {
            public string text { get; set; }
        }

        public class PickSkinToneText
        {
            public List<Run14> runs { get; set; }
        }

        public class EmojiPickerRenderer
        {
            public string id { get; set; }
            public List<Category> categories { get; set; }
            public List<CategoryButton> categoryButtons { get; set; }
            public SearchPlaceholderText searchPlaceholderText { get; set; }
            public SearchNoResultsText searchNoResultsText { get; set; }
            public PickSkinToneText pickSkinToneText { get; set; }
            public string trackingParams { get; set; }
            public string clearSearchLabel { get; set; }
            public string skinToneGenericLabel { get; set; }
            public string skinToneLightLabel { get; set; }
            public string skinToneMediumLightLabel { get; set; }
            public string skinToneMediumLabel { get; set; }
            public string skinToneMediumDarkLabel { get; set; }
            public string skinToneDarkLabel { get; set; }
        }

        public class Picker
        {
            public EmojiPickerRenderer emojiPickerRenderer { get; set; }
        }

        public class Icon4
        {
            public string iconType { get; set; }
        }

        public class AccessibilityData4
        {
            public string label { get; set; }
        }

        public class Accessibility4
        {
            public AccessibilityData4 accessibilityData { get; set; }
        }

        public class ToggledIcon
        {
            public string iconType { get; set; }
        }

        public class LiveChatIconToggleButtonRenderer
        {
            public string targetId { get; set; }
            public Icon4 icon { get; set; }
            public string tooltip { get; set; }
            public Accessibility4 accessibility { get; set; }
            public ToggledIcon toggledIcon { get; set; }
            public bool? disabled { get; set; }
        }

        public class PickerButton
        {
            public LiveChatIconToggleButtonRenderer liveChatIconToggleButtonRenderer { get; set; }
        }

        public class Text4
        {
            public string simpleText { get; set; }
        }

        public class WebCommandMetadata3
        {
            public string url { get; set; }
        }

        public class CommandMetadata3
        {
            public WebCommandMetadata3 webCommandMetadata { get; set; }
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

        public class WebNavigationEndpointData3
        {
            public string url { get; set; }
            public string webPageType { get; set; }
        }

        public class NextEndpoint
        {
            public string clickTrackingParams { get; set; }
            public CommandMetadata4 commandMetadata { get; set; }
            public WatchEndpoint watchEndpoint { get; set; }
            public WebNavigationEndpointData3 webNavigationEndpointData { get; set; }
        }

        public class SignInEndpoint
        {
            public NextEndpoint nextEndpoint { get; set; }
        }

        public class WebNavigationEndpointData4
        {
            public string url { get; set; }
        }

        public class NavigationEndpoint3
        {
            public string clickTrackingParams { get; set; }
            public CommandMetadata3 commandMetadata { get; set; }
            public SignInEndpoint signInEndpoint { get; set; }
            public WebNavigationEndpointData4 webNavigationEndpointData { get; set; }
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
            public Text4 text { get; set; }
            public NavigationEndpoint3 navigationEndpoint { get; set; }
            public Accessibility5 accessibility { get; set; }
            public string trackingParams { get; set; }
        }

        public class Button
        {
            public ButtonRenderer6 buttonRenderer { get; set; }
        }

        public class MessageRenderer
        {
            public string trackingParams { get; set; }
            public Button button { get; set; }
        }

        public class InteractionMessage
        {
            public MessageRenderer messageRenderer { get; set; }
        }

        public class LiveChatMessageInputRenderer
        {
            public InputField inputField { get; set; }
            public SendButton sendButton { get; set; }
            public List<Picker> pickers { get; set; }
            public List<PickerButton> pickerButtons { get; set; }
            public InteractionMessage interactionMessage { get; set; }
        }

        public class ActionPanel
        {
            public LiveChatMessageInputRenderer liveChatMessageInputRenderer { get; set; }
        }

        public class Icon5
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
            public Icon5 icon { get; set; }
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

        public class Run15
        {
            public string text { get; set; }
        }

        public class TitleText
        {
            public List<Run15> runs { get; set; }
        }

        public class Run16
        {
            public string text { get; set; }
        }

        public class Text5
        {
            public List<Run16> runs { get; set; }
        }

        public class Icon6
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
            public Text5 text { get; set; }
            public Icon6 icon { get; set; }
            public ServiceEndpoint serviceEndpoint { get; set; }
            public string trackingParams { get; set; }
        }

        public class Run17
        {
            public string text { get; set; }
        }

        public class Text6
        {
            public List<Run17> runs { get; set; }
        }

        public class Icon7
        {
            public string iconType { get; set; }
        }

        public class UserFeedbackEndpoint
        {
            public bool hack { get; set; }
            public string bucketIdentifier { get; set; }
        }

        public class NavigationEndpoint4
        {
            public UserFeedbackEndpoint userFeedbackEndpoint { get; set; }
        }

        public class MenuNavigationItemRenderer
        {
            public Text6 text { get; set; }
            public Icon7 icon { get; set; }
            public NavigationEndpoint4 navigationEndpoint { get; set; }
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

        public class LiveChatHeaderRenderer
        {
            public TitleText titleText { get; set; }
            public OverflowMenu overflowMenu { get; set; }
            public CollapseButton collapseButton { get; set; }
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

        public class Run18
        {
            public string text { get; set; }
        }

        public class Title3
        {
            public List<Run18> runs { get; set; }
        }

        public class Icon8
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
            public Icon8 icon { get; set; }
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

        public class Thumbnail3
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class AuthorPhoto2
        {
            public List<Thumbnail3> thumbnails { get; set; }
        }

        public class Icon9
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
            public Icon9 icon { get; set; }
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
            public Title3 title { get; set; }
            public BackButton backButton { get; set; }
            public List<Participant> participants { get; set; }
        }

        public class ParticipantsList
        {
            public LiveChatParticipantsListRenderer liveChatParticipantsListRenderer { get; set; }
        }

        public class Thumbnail4
        {
            public string url { get; set; }
        }

        public class Image
        {
            public List<Thumbnail4> thumbnails { get; set; }
        }

        public class Emoji
        {
            public string emojiId { get; set; }
            public List<string> shortcuts { get; set; }
            public List<string> searchTerms { get; set; }
            public Image image { get; set; }
            public bool? supportsSkinTone { get; set; }
        }

        public class Run19
        {
            public string text { get; set; }
        }

        public class ReconnectMessage
        {
            public List<Run19> runs { get; set; }
        }

        public class Run20
        {
            public string text { get; set; }
        }

        public class UnableToReconnectMessage
        {
            public List<Run20> runs { get; set; }
        }

        public class Run21
        {
            public string text { get; set; }
        }

        public class FatalError
        {
            public List<Run21> runs { get; set; }
        }

        public class Run22
        {
            public string text { get; set; }
        }

        public class ReconnectedMessage
        {
            public List<Run22> runs { get; set; }
        }

        public class Run23
        {
            public string text { get; set; }
        }

        public class GenericError
        {
            public List<Run23> runs { get; set; }
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
            public List<Emoji> emojis { get; set; }
            public ClientMessages clientMessages { get; set; }
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
}
