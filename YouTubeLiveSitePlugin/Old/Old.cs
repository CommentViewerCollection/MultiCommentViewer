using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SitePlugin;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using YoutubeLib;
namespace YouTubeLiveSitePlugin.Old
{
    using MultiCommentViewer.Model.YouTubeLive;
    public class Metadata : IMetadata
    {
        public string Title { get; set; }
        public string Elapsed { get; set; }
        public string CurrentViewers { get; set; }
        public string Active { get; set; }
        public string TotalViewers { get; set; }
        public bool? IsLive { get; set; }
    }
    public class YouTubeCommentProvider : IYouTubeCommentProvider
    {
        #region Fields
        private System.Timers.Timer _500msTimer = new System.Timers.Timer();
        private readonly int default_timeoutMs = 1000;
        CookieContainer _cc;
        private DateTime _startAt;
        private readonly YoutubeLib.IDataSource _dataSource;
        private ChatContext _chatContext;
        private YoutubeLib.AccessContext _accessContext;
        private CancellationTokenSource _cts;
        #endregion //Fields


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
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler CanConnectChanged;
        public event EventHandler CanDisconnectChanged;

        public static async Task<YoutubeLib.LivePageContext> GetLivePageContextAsync(YoutubeLib.IDataSource dataSource, string live_id, CookieContainer cc)
        {
            var url = "https://www.youtube.com/watch?v=" + live_id;
            var headers = new List<KeyValuePair<string, string>>
            {
            };
            var html = await dataSource.GetAsync(url, headers, cc);
            var context = ParseLivePage(html);
            return context;
        }
        private static YoutubeLib.LivePageContext ParseLivePage(string html)
        {
            var context = new YoutubeLib.LivePageContext();
            var match0 = Regex.Match(html, "\"innertube_api_key\":\"(?<api_key>[^\"]+)\"");
            if (match0.Success)
            {
                context.innertube_api_key = match0.Groups["api_key"].Value;
            }
            var match1 = Regex.Match(html, "href=\"\\/watch\\?v=(?<vid>[^\"]+)\"");
            if (match1.Success)
            {
                context.vid = match1.Groups["vid"].Value;
            }
            var match2 = Regex.Match(html, "src=\"(?<chatpageurl>https://www.youtube.com/live_chat\\?continuation=[^\"]+)\"");
            if (match2.Success)
            {
                context.ChatPageUrl = match2.Groups["chatpageurl"].Value;
            }
            return context;
        }
        public static async Task<YoutubeLib.LiveChatContext> GetLiveChatContextAsync(YoutubeLib.IDataSource dataSource, string vid, CookieContainer cc)
        {
            var url = $"https://www.youtube.com/live_chat?v={vid}&is_popout=1";
            var headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36"),
            };
            var html = await dataSource.GetAsync(url, headers, cc);
#if LOGGING
            using (var sr = new System.IO.StreamWriter("before_live_chat_context.txt", false))
            {
                sr.Write(html);
            }
#endif
            var context = new YoutubeLib.LiveChatContext();
            context.Raw = html;
            var match0 = Regex.Match(html, "\"XSRF_TOKEN\":\"(?<xsrf_token>[^\"]+)\"");
            if (match0.Success)
            {
                context.xsrf_token = match0.Groups["xsrf_token"].Value;
            }
            var match3 = Regex.Match(html, "\"ID_TOKEN\":\"(?<id_token>[^\"]+)\"");
            if (match3.Success)
            {
                context.id_token = match3.Groups["id_token"].Value;
            }
            var match1 = Regex.Match(html, "\"VARIANTS_CHECKSUM\":\"(?<variants_checksum>[^\"]+)\"");
            if (match1.Success)
            {
                context.variants_checksum = match1.Groups["variants_checksum"].Value;
            }
            var match2 = Regex.Match(html, "\"continuation\":\"(?<continuation>[^\"]+)\"");
            if (match2.Success)
            {
                context.continuation = match2.Groups["continuation"].Value;
            }


            var match4 = Regex.Match(html, "\"PAGE_CL\":(?<page_cl>\\d+)");
            if (match4.Success)
            {
                context.page_cl = match4.Groups["page_cl"].Value;
            }
            var match5 = Regex.Match(html, "\"PAGE_BUILD_LABEL\":\"(?<page_build_label>[^\"]+)\"");
            if (match5.Success)
            {
                context.page_build_label = match5.Groups["page_build_label"].Value;
            }
            var match6 = Regex.Match(html, "window\\[\"ytInitialData\"\\] = (?<ytinitialdata>{.+});");
            if (match6.Success)
            {
                var ytInitialData = match6.Groups["ytinitialdata"].Value;
                context.YtInitialData = JsonConvert.DeserializeObject<YtInitialData>(ytInitialData);
            }
            var match7 = Regex.Match(html, "\"INNERTUBE_API_KEY\":\"(?<key>[^\"]+)\"");
            if (match7.Success)
            {
                context.inner_api_key = match7.Groups["key"].Value;
            }
            return context;
        }
        
        private IEnumerable<IYouTubeLiveCommentData> Actions2CommentData(IEnumerable<Action> actions)
        {
            foreach (var action in actions)
            {

                if (action is AddChatItemAction addChatItem)
                {
                    //ブラウザでは[0]の方を使用しているっぽいし、[1]では変更前の古いサムネが表示されてしまう問題が報告されたため変更。2017/07/23
                    //var thumbnail = addChatItem.AuthorPhoto.thumbnails.Count >= 2 ? addChatItem.AuthorPhoto.thumbnails[1].url : addChatItem.AuthorPhoto.thumbnails[0].url;
                    var thumbnail = addChatItem.AuthorPhoto.thumbnails[0].url;
                    IYouTubeLiveCommentData data = new YouTubeLiveCommentData(thumbnail, addChatItem.AuthorName, addChatItem.Message, addChatItem.ClientId, addChatItem.Id, addChatItem.TimestampUsec)
                    {
                        PurchaseAmount = addChatItem.IsPaidAction ? addChatItem.PurchaseAmount : null,
                    };
                    yield return data;
                }
            }
        }
        private void _500msTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var elapsed = DateTime.Now - _startAt;
            var elapsedStr = elapsed.ToString(@"hh\:mm\:ss");
            MetadataUpdated?.Invoke(this, new Metadata { Elapsed = elapsedStr });
            //_callback(new MetaCallback(new Metadata
            //{
            //    Elapsed = elapsedStr,
            //}));
        }
        bool canComment = true;
        public async Task ConnectAsync(string input, ryu_s.BrowserCookie.IBrowserProfile browserProfile)
        {
            CanConnect = false;
            CanDisconnect = true;

            var cookies = browserProfile.GetCookieCollection("youtube.com");
            _cc = new CookieContainer();
            _cc.Add(cookies);

            var _vid = await GetVid(input);
            _cts = new CancellationTokenSource();

            var userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
            //400が返ってくることがある
            var liveChatContext = await GetLiveChatContextAsync(_dataSource, _vid, _cc).ConfigureAwait(false);
            if (liveChatContext.YtInitialData == null)
            {
                //放送が終了しているっぽい。とりあえず終了しておく。この辺の作り込みをちゃんとしたい。
                throw new LiveNotFoundException();
            }
            if (liveChatContext.YtInitialData.contents == null)
            {
                //存在しないvidが入力された場合に来るっぽい。
                throw new InvalidUrlException($"live_id={_vid}");
            }
            if (liveChatContext.YtInitialData.contents.liveChatRenderer == null)
            {
                throw new InvalidUrlException("ライブのURLではないかチャットが無効になっています。");
            }
            if (liveChatContext.YtInitialData.contents.liveChatRenderer.actionPanel.liveChatMessageInputRenderer.sendButton.buttonRenderer.serviceEndpoint == null)
            {
                //未ログインかも。
                //throw new NotLoggedinException();
                //未ログインでもおｋ。ただし、コメント出来ない。
                canComment = false;
            }
            _chatContext = new ChatContext(liveChatContext);
            _accessContext = new YoutubeLib.AccessContext
            {
                Referer = $"https://www.youtube.com/live_chat?v={_vid}&is_popout=1",
                UserAgent = userAgent,
                ClientName = "1",
                ClientVersion = liveChatContext.YtInitialData.responseContext.Cver,
                //不要っぽい
                Identity_Token = liveChatContext.id_token,
                Page_CL = liveChatContext.page_cl,
                Page_Label = liveChatContext.page_build_label,
                Variants_Checksum = liveChatContext.variants_checksum,
            };
            try
            {
                var comments = Actions2CommentData(ChatData.ConvertAction(liveChatContext.YtInitialData.contents.liveChatRenderer.actions)).Cast<ICommentData>().ToList();                
                foreach(var comment in comments)
                {
                    var cvm = new YouTubeCommentViewModel(ConnectionName, _options, SiteOptions)
                    {
                         MessageItems = comment.MessageItems,
                         NameItems = comment.NameItems,
                         Id = comment.Id,
                    };
                    CommentReceived?.Invoke(this, cvm);
                }
            }
            catch (ParseException ex)
            {
                throw new SpecChangedException(liveChatContext.Raw, ex);
            }
            var liveContext = await YoutubeLib.API.GetLiveContextAsync(_dataSource, _vid, _cc);
            if (liveContext.StartDate.HasValue)
            {
                _startAt = liveContext.StartDate.Value;
                _500msTimer.Interval = 500;
                _500msTimer.Elapsed += _500msTimer_Elapsed;
                _500msTimer.Enabled = true;
            }
            var t1 = DoChatLoop(liveChatContext, _cc, _cts.Token);
            var t2 = DoUpdatedMetadataLoop(liveChatContext.inner_api_key, _vid, _cc, _cts.Token);
            try
            {
                while (true)
                {
                    var t = await Task.WhenAny(t1, t2);
                    if (t == t1)
                    {
                        _cts.Cancel();
                        try
                        {
                            await t1;//TODO:ここで例外が発生した場合にt2を終わらせなければいけない
                        }
                        catch (Exception)
                        {
                            //ryu_s.Common.ExceptionLogger.Logging(ryu_s.Common.LogLevel.error, new Exception("error=k1"));
                            throw;
                        }
                        break;
                    }
                    else if (t == t2 && (t1.IsCanceled || t1.IsCompleted || t1.IsFaulted))
                    {
                        try
                        {
                            await t1;
                        }
                        catch (Exception)
                        {
                            //ryu_s.Common.ExceptionLogger.Logging(ryu_s.Common.LogLevel.error, new Exception("error=k2"));
                        }
                        break;
                    }
                    else
                    {
                        t2 = DoUpdatedMetadataLoop(liveChatContext.inner_api_key, _vid, _cc, _cts.Token);
                    }
                }
            }
            //            catch(Exception ex)
            //            {
            //                await _callback(new InfoCallback("問題が発生したため、サーバとの接続が切れました"));
            //#if DEBUG
            //                await _callback(new InfoCallback(ex.Message));
            //#endif
            //                throw;
            //            }
            finally
            {
                _500msTimer.Enabled = false;
                _cts.Dispose();
                _cts = new CancellationTokenSource();

                CanConnect = true;
                CanDisconnect = false;
            }


        }
        private IMetadata ConvertToMetadata(YoutubeLib.UpdatedMetadata.UpdatedMetadata from)
        {
            var metadata = new Metadata() { TotalViewers = "-" };
            foreach (var action in from.actions)
            {
                if (action.updateViewershipAction != null)
                {
                    if (action.updateViewershipAction.viewership.videoViewCountRenderer.viewCount != null)
                    {
                        //"1,665 人が視聴中"
                        var str = action.updateViewershipAction.viewership.videoViewCountRenderer.viewCount.ToString();
                        var match = Regex.Match(str, "^(?<count>[\\d,]+)");
                        if (match.Success)
                        {
                            var count = int.Parse(match.Groups["count"].Value.Replace(",", ""));
                            metadata.CurrentViewers = count.ToString();
                        }
                        else
                        {
                            throw new YoutubeLib.SpecChangedException("リアルタイム視聴者数の表示形式が変更され、人数の抽出が出来なくなった。=>" + str);
                        }
                        metadata.IsLive = action.updateViewershipAction.viewership.videoViewCountRenderer.isLive;
                    }
                }
                else if (action.updateSentimentAction != null)
                {
                    var likeCount = action.updateSentimentAction.sentimentRenderer.likeButtonRenderer.likeCount;
                    var dislikeCount = action.updateSentimentAction.sentimentRenderer.likeButtonRenderer.dislikeCount;
                }
                else if (action.updateTitleAction != null)
                {
                    var title = action.updateTitleAction.title.ToString();
                    metadata.Title = title;
                }
                //Descriptionは未実装。。。
            }
            return metadata;
        }
        private async Task DoUpdatedMetadataLoop(string inner_api_key, string vid, CookieContainer cc, CancellationToken ct)
        {
            Debug.WriteLine("DoUpdatedMetadataLoop開始");
            int timeoutMs = default_timeoutMs;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    Debug.WriteLine($"metadata timeoutMs={timeoutMs}");
                    await Task.Delay(timeoutMs, ct).ConfigureAwait(false);

                    //400,500
                    var metadata = await YoutubeLib.API.PostUpdatedMetadata(_dataSource, inner_api_key, vid, cc).ConfigureAwait(false);
                    if (metadata.continuation.timedContinuationData == null)
                        break;
                    timeoutMs = metadata.continuation.timedContinuationData.timeoutMs;

                    var meta = ConvertToMetadata(metadata);
                    if (meta.IsLive == false)
                    {
                        Disconnect();
                        break;
                    }
                    //await _callback(new MetaCallback(meta));
                    MetadataUpdated?.Invoke(this, meta);
                }
            }
            catch (TaskCanceledException) { }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
            {
                var res = (HttpWebResponse)ex.Response;
                if (res.StatusCode == HttpStatusCode.BadRequest)
                {
                    //400。これが来たら放送が終了したと判断しちゃって良いかも。
                    //放送終了後もコメビュをつけっぱなしにしておくと、UpdatedMetadataを取得しようとした時に400が投げられていた。

                    //2017/08/14　直接的な原因はvidに"KO9rTWo9AOc&feature=share"のように&以降も含まれてしまっていたために400BadRequestが返されたのだが、
                    //ここを通ってしまっていたために発見に時間が掛かってしまったことがあった。
                    Disconnect();
                }
                else if (res.StatusCode == HttpStatusCode.InternalServerError)
                {
                    //500
                    //ryu_s.Common.ExceptionLogger.Logging(ryu_s.Common.LogLevel.error, ex);
                    await Task.Delay(5000);
                }
                else
                {
                    //ryu_s.Common.ExceptionLogger.Logging(ryu_s.Common.LogLevel.error, ex);
                }
            }
            catch (Exception ex)
            {
                //ryu_s.Common.ExceptionLogger.Logging(ryu_s.Common.LogLevel.error, ex);
            }
            finally
            {
                Debug.WriteLine("DoUpdatedMetadataLoop終了");
            }
        }
        private async Task DoChatLoop(YoutubeLib.LiveChatContext liveChatContext, CookieContainer cc, CancellationToken ct)
        {
            Debug.WriteLine("DoChatLoop開始");

            try
            {
                int timeoutMs = default_timeoutMs;
                string continuation;
                if (liveChatContext.YtInitialData.contents.liveChatRenderer.continuations[0].timedContinuationData != null)
                {
                    continuation = liveChatContext.YtInitialData.contents.liveChatRenderer.continuations[0].timedContinuationData.continuation;
                }
                else
                {
                    continuation = liveChatContext.YtInitialData.contents.liveChatRenderer.continuations[0].invalidationContinuationData.continuation;
                }

                do
                {
                    string raw = "";
                    try
                    {
                        //                   throw new WebException("", WebExceptionStatus.NameResolutionFailure);
                        var (getLiveChat, raw_) = await YoutubeLib.API.GetLiveChat(_dataSource, continuation, _accessContext, cc).ConfigureAwait(false);
                        raw = raw_;
                        if (getLiveChat.response.continuationContents == null)
                        {
                            throw new YtException("原因不明のエラーが発生しました（continuationContents無し）", new Exception(raw_));
                        }
                        //サーバから送られてくるデータがかなり複雑であるため、必要なデータだけを整理して独自のクラスに格納する。
                        var chatData = new ChatData(getLiveChat);
                        continuation = chatData.Continuation;

                        timeoutMs = chatData.TimeoutMs;

                        Debug.WriteLine($"actions.Count={chatData.Actions.Count}");
                        if (chatData.Actions.Count > 0)
                        {
                            var interval = timeoutMs / chatData.Actions.Count;
                            foreach (var commentData in Actions2CommentData(chatData.Actions))
                            {
                                //await _callback(new ReceiveCallback(new List<ICommentData> { commentData })).ConfigureAwait(false);
                                var cvm = new YouTubeCommentViewModel(ConnectionName, _options, SiteOptions)
                                {
                                     NameItems = commentData.NameItems,
                                     MessageItems = commentData.MessageItems,
                                };
                                CommentReceived?.Invoke(this, cvm);
                                await Task.Delay(interval, ct).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            var interval = timeoutMs > default_timeoutMs ? timeoutMs : default_timeoutMs;
                            interval = 1000;
                            if (interval < default_timeoutMs)
                            {
                                interval = default_timeoutMs;
                            }
                            await Task.Delay(interval, ct).ConfigureAwait(false);
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (NullReferenceException ex)
                    {
                        throw new SpecChangedException(raw, ex);
                    }
                    catch (ParseException ex)
                    {
                        throw new SpecChangedException(raw, ex);
                    }
                    catch (WebException ex) when (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
                    {
                        var res = (HttpWebResponse)ex.Response;
                        if (res.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        //ryu_s.Common.ExceptionLogger.Logging(ryu_s.Common.LogLevel.error, new Exception("sss=" + raw, ex));
                        throw;
                    }
                } while (!ct.IsCancellationRequested);
            }
            finally
            {
                Debug.WriteLine("DoChatLoop終了");
            }
        }
        private Task<string> GetVid(string input)
        {
            return YoutubeLib.API.GetVid(_dataSource, input);
        }
        public void Disconnect()
        {
            if(_cts != null)
            {
                _cts.Cancel();
            }
        }

        public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
        {
            throw new NotImplementedException();
        }

        public Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }
        

        public ConnectionName ConnectionName { get; set; }
        private readonly IOptions _options;
        public YouTubeSiteOptions SiteOptions { get; set; }
        public YouTubeCommentProvider(ConnectionName connectionName, IOptions options, YouTubeSiteOptions siteOptions)
        {
            ConnectionName = connectionName;
            _options = options;
            SiteOptions = siteOptions;
            _dataSource = new YoutubeLib.YoutubeServer();
            CanConnect = true;
            CanDisconnect = false;
        }
    }
    public class YouTubeSiteOptions : IYouTubeSiteOptions
    {
        public YouTubeSiteOptions()
        {

        }
        internal YouTubeSiteOptions Clone()
        {
            return (YouTubeSiteOptions)this.MemberwiseClone();
        }
        internal void Set(YouTubeSiteOptions changedOptions)
        {
            var properties = changedOptions.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.SetMethod != null)
                {
                    property.SetValue(this, property.GetValue(changedOptions));
                }
            }
        }
    }



    [Serializable]
    public class CVException : Exception
    {
        public CVException()
        { }

        public CVException(string message)
            : base(message)
        { }

        public CVException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected CVException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
    [Serializable]
    public class LiveNotFoundException : CVException
    {
        public LiveNotFoundException()
        { }

        public LiveNotFoundException(string message)
            : base(message)
        { }

        public LiveNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected LiveNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }


    public interface ICommentData
    {
        string ThumbnailUrl { get; }
        /// <summary>
        /// ユーザ名
        /// </summary>
        IEnumerable<IMessagePart> NameItems { get; }
        /// <summary>
        /// メッセージ
        /// </summary>
        IEnumerable<IMessagePart> MessageItems { get; }
        //情報
        //ニコ生 "C P"
        string Info { get; }
        //
        //コメントID
        string Id { get; }
        //投稿日時
        //投稿経過時間

        string UserId { get; }
    }
    public interface IYouTubeLiveCommentData : ICommentData
    {
        bool IsPaid { get; }
        string PurchaseAmount { get; }
        DateTime PostTime { get; }
        string ChannelId { get; }
    }
    public class YouTubeLiveCommentData : IYouTubeLiveCommentData
    {
        public string ThumbnailUrl { get; }
        public IEnumerable<IMessagePart> NameItems { get; }
        public IEnumerable<IMessagePart> MessageItems { get; }

        public string Info { get; }
        public string Id { get; }
        public string UserId { get; }
        public bool IsPaid { get { return !string.IsNullOrEmpty(PurchaseAmount); } }
        public string PurchaseAmount { get; set; }
        public DateTime PostTime { get; }
        public string ChannelId { get; }

        public YouTubeLiveCommentData(string thumbnailUrl, string name, string message, string user_id, string id, long timestampUsec)
        {
            ThumbnailUrl = thumbnailUrl;
            NameItems = new List<IMessagePart> { new MessageText(name) };
            MessageItems = new List<IMessagePart> { new MessageText(message) };
            UserId = user_id;
            Id = id;
            PostTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestampUsec / 1000);
        }
        public YouTubeLiveCommentData(string thumbnailUrl, IEnumerable<IMessagePart> nameItems, IEnumerable<IMessagePart> messageItems, string user_id, string id, DateTime postDate)
        {
            ThumbnailUrl = thumbnailUrl;
            NameItems = nameItems;
            MessageItems = messageItems;
            UserId = user_id;
            Id = id;
            PostTime = postDate;
        }
    }
    public class MessageText : IMessageText
    {
        public string Text { get; }
        public MessageText(string text)
        {
            Text = text;
        }
    }


}

namespace MultiCommentViewer.Model.YouTubeLive
{


    /// <summary>
    /// 主にコメント投稿時に必要な情報
    /// </summary>
    class ChatContext
    {
        public int NumSentMessages { get; set; }
        public bool IsLoggedIn { get; private set; }
        public string SessionToken { get; private set; }
        public string ClientIdPrefix { get; }

        public string Base64EncodedServiceEndpoint { get; }
        public string Sej { get; }

        public ChatContext(YoutubeLib.LiveChatContext a)
        {
            SessionToken = a.xsrf_token;
            IsLoggedIn = a.YtInitialData.responseContext.Logged_in == "1";
            var serviceEndpoint = a.YtInitialData.contents.liveChatRenderer.actionPanel.liveChatMessageInputRenderer.sendButton.buttonRenderer.serviceEndpoint;
            if (serviceEndpoint != null)
            {
                //2018/01/27
                //IsLoggedInがfalseの時、serviceEndpointがnullになっている。これはコメント投稿時に必要なだけで、コメントを取得するだけなら本来は支障がない。
                //しかし、これまでは未ログインが分かった時点で例外を投げてしまっていた。
                ClientIdPrefix = a.YtInitialData.contents.liveChatRenderer.actionPanel.liveChatMessageInputRenderer.sendButton.buttonRenderer.serviceEndpoint.sendLiveChatMessageEndpoint.clientIdPrefix;

                //前はコメント投稿時にbase64EncodedServiceEndpointが必要だったのだが、2017/03/23にこの値がhttps://www.youtube.com/live_chat?v={vid}から無くなった。
                //また、se={base64EncodedServiceEndpoint}というパラメータを使ってコメントを投稿していたのだが、
                //sej={serviceEndpointのJSON形式}という形に変更された。
                if (a.YtInitialData.contents.liveChatRenderer.actionPanel.liveChatMessageInputRenderer.sendButton.buttonRenderer.serviceEndpoint.webSerializedServiceEndpointExtension != null)
                {
                    Base64EncodedServiceEndpoint = a.YtInitialData.contents.liveChatRenderer.actionPanel.liveChatMessageInputRenderer.sendButton.buttonRenderer.serviceEndpoint.webSerializedServiceEndpointExtension.base64EncodedServiceEndpoint;
                }
                else
                {
                    var sendButton = ExtractSej(a.Raw, "\"sendButton\":");//他のButtonにもserviceEndpointがあるため、まずこれだけ切り出す
                    Sej = ExtractSej(sendButton, "\"serviceEndpoint\":");
                }
            }
        }
        public void Set(YoutubeLib.RootObject a)
        {
            SessionToken = a.xsrf_token;
            IsLoggedIn = a.response.responseContext.Logged_in == "1";
        }
        /// <summary>
        /// 文字列から指定した名前をもつJSONだけを抜き出す
        /// </summary>
        /// <param name="s"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private string ExtractSej(string s, string prefix)
        {
            var startPos = s.IndexOf(prefix) + prefix.Length;

            int n = 0;//閉じていない'{'の個数

            int i = 0;
            for (; ; i++)
            {
                var c = s[startPos + i];
                if (c == '{')
                    n++;
                else if (c == '}')
                    n--;

                if (n == 0)
                {
                    i++;
                    break;
                }
            }
            var sej = s.Substring(startPos, i);
            return sej;
        }
    }
    public class ChatData
    {
        public List<Action> Actions { get; }
        public string Xsrf_Token { get; }
        public int TimeoutMs { get; }
        public string Continuation { get; }
        public static List<Action> ConvertAction(List<YoutubeLib.Action> actions)
        {
            var list = new List<Action>();
            if (actions != null)
            {
                foreach (var action in actions)
                {
                    if (action.addChatItemAction != null && action.addChatItemAction.item.liveChatPlaceholderItemRenderer != null)
                    {
                        //何の意味があるのか分からない
                    }
                    else if (action.addChatItemAction != null && action.addChatItemAction.item.liveChatTextMessageRenderer != null)
                    {
                        list.Add(new AddChatItemAction(action.addChatItemAction));
                    }
                    else if (action.addChatItemAction != null && action.addChatItemAction.item.liveChatPaidMessageRenderer != null)
                    {
                        //投げ銭
                        list.Add(new AddChatItemAction(action.addChatItemAction));
                    }
                    else if (action.addChatItemAction != null)
                    {
                        list.Add(new AddChatItemByModeratorAction(action.addChatItemAction));
                    }
                    else if (action.markChatItemAsDeletedAction != null)
                    {
                        list.Add(new MarkChatItemAsDeletedAction(action.markChatItemAsDeletedAction));
                    }
                    else if (action.markChatItemsByAuthorAsDeletedAction != null)
                    {
                        list.Add(new MarkChatItemsByAuthorAsDeletedAction(action.markChatItemsByAuthorAsDeletedAction));
                    }
                    else if (action.addLiveChatTickerItemAction != null)
                    {
                        list.Add(new AddLiveChatTickerItemAction(action.addLiveChatTickerItemAction));
                    }
                    else if (action.replaceChatItemAction != null) { }
                    else
                    {
                        throw new ParseException();
                    }
                }
            }
            return list;
        }
        public ChatData(YoutubeLib.RootObject getLiveChat)
        {
            //コメントが無いとnullになる。また、continuationContentsがnullになることがある。配信終了後？
            if (getLiveChat.response.continuationContents.liveChatContinuation.actions != null)
                this.Actions = ConvertAction(getLiveChat.response.continuationContents.liveChatContinuation.actions);
            else
                this.Actions = new List<Model.YouTubeLive.Action>();
            Xsrf_Token = getLiveChat.xsrf_token;

            if (getLiveChat.response.continuationContents.liveChatContinuation.continuations[0].timedContinuationData != null)
            {
                Continuation = getLiveChat.response.continuationContents.liveChatContinuation.continuations[0].timedContinuationData.continuation;
                TimeoutMs = getLiveChat.response.continuationContents.liveChatContinuation.continuations[0].timedContinuationData.timeoutMs;
                Debug.WriteLine($"ChatLoop continuationType=timedContinuation, timeoutMs={TimeoutMs}");
            }
            else
            {
                Continuation = getLiveChat.response.continuationContents.liveChatContinuation.continuations[0].invalidationContinuationData.continuation;
                //TimeoutMs = 3000;//サーバから来る値は30000ms。でも明らかに公式ページの更新間隔は数秒。この値を使うわけではなさそう。とりあえず自前の値を設定。
                TimeoutMs = getLiveChat.response.continuationContents.liveChatContinuation.continuations[0].invalidationContinuationData.timeoutMs;
                Debug.WriteLine($"ChatLoop continuationType=invalidationContinuation, timeoutMs={TimeoutMs}");
            }
        }
    }
    public abstract class Action { }
    public class AddChatItemAction : Action
    {
        public YoutubeLib.AuthorPhoto AuthorPhoto { get; }
        public string ClientId { get; }
        public string Id { get; }
        public string Message { get; }
        public string AuthorExternalChannelId { get; }
        public string AuthorName { get; }
        public long TimestampUsec { get; }
        public bool IsPaidAction { get; }
        public string PurchaseAmount { get; }
        public AddChatItemAction(YoutubeLib.AddChatItemAction a)
        {
            if (a.item.liveChatPaidMessageRenderer != null)
            {
                IsPaidAction = true;
                var renderer = a.item.liveChatPaidMessageRenderer;
                this.AuthorPhoto = renderer.authorPhoto;
                this.Id = renderer.id;
                //投げ銭の際にコメントも付加できるのだろうか。あるとしたら多分renderer.messageに入る
                this.Message = renderer.message != null ? renderer.message.ToString() : "";
                this.AuthorExternalChannelId = renderer.authorExternalChannelId;
                this.AuthorName = renderer.authorName.ToString();
                this.PurchaseAmount = renderer.purchaseAmountText.ToString();
                this.TimestampUsec = long.Parse(renderer.timestampUsec);
            }
            else
            {
                this.ClientId = a.clientId;

                var renderer = a.item.liveChatTextMessageRenderer;
                this.AuthorPhoto = renderer.authorPhoto;
                this.Id = renderer.id;
                this.Message = renderer.Message.ToString();
#if DEBUG
                if (renderer.Message.runs.Count > 1)
                {

                }
#endif
                this.AuthorExternalChannelId = renderer.authorExternalChannelId;
                this.AuthorName = renderer.authorName.ToString();
                this.TimestampUsec = long.Parse(renderer.timestampUsec);
            }
        }
        public override string ToString()
        {
            return Message;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>モデレータによる警告等のメッセージにはclientId等が含まれていない。よって別クラスにする。</remarks>
    public class AddChatItemByModeratorAction : Action
    {
        public AddChatItemByModeratorAction(YoutubeLib.AddChatItemAction a)
        {

        }
    }
    public class MarkChatItemsByAuthorAsDeletedAction : Action
    {
        public MarkChatItemsByAuthorAsDeletedAction(YoutubeLib.MarkChatItemsByAuthorAsDeletedAction a)
        {

        }
    }
    public class MarkChatItemAsDeletedAction : Action
    {
        public MarkChatItemAsDeletedAction(YoutubeLib.MarkChatItemAsDeletedAction a)
        {

        }
    }
    public class AddLiveChatTickerItemAction : Action
    {
        public AddLiveChatTickerItemAction(YoutubeLib.AddLiveChatTickerItemAction a)
        {

        }
    }
}

namespace YoutubeLib
{
    public class LiveContext
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class LiveChatContext
    {
        public string Raw { get; set; }
        public string variants_checksum { get; set; }
        public string xsrf_token { get; set; }
        public string continuation { get; set; }
        public string id_token { get; set; }
        public string page_cl { get; set; }
        public string page_build_label { get; set; }
        public string cver { get; set; }
        public string inner_api_key { get; set; }
        public YtInitialData YtInitialData { get; set; }
    }
    public class AccessContext
    {
        public string Referer { get; set; }
        public string UserAgent { get; set; }
        public string ClientName { get; set; }
        public string ClientVersion { get; set; }
        public string Identity_Token { get; set; }
        public string Page_CL { get; set; }
        public string Page_Label { get; set; }
        public string Variants_Checksum { get; set; }
    }
}
namespace YoutubeLib
{
    public class Thumbnail
    {
        public string url { get; set; }
    }
    public class Thumbnail2
    {
        public int height { get; set; }
        public int width { get; set; }
        public string url { get; set; }
    }
    public class Image
    {
        public List<Thumbnail> thumbnails { get; set; }
        public Accessibility accessibility { get; set; }
    }

    public class Emoji
    {
        public List<string> shortcuts { get; set; }
        public Image image { get; set; }
        public List<string> searchTerms { get; set; }
        public string emojiId { get; set; }
        public bool? supportsSkinTone { get; set; }
        public bool? isCustomEmoji { get; set; }
    }


    public class Accessibility4
    {
        public string label { get; set; }
    }

    public class WebSerializedServiceEndpointExtension2
    {
        public string base64EncodedServiceEndpoint { get; set; }
    }




    public class LiveChatTextMessageRenderer2
    {
        public Text authorName { get; set; }
        public AuthorPhoto authorPhoto { get; set; }
        public string authorExternalChannelId { get; set; }
    }

    public class Template
    {
        public LiveChatTextMessageRenderer2 liveChatTextMessageRenderer { get; set; }
    }

    public class AddLiveChatTextMessageFromTemplateAction
    {
        public Template template { get; set; }
    }


    public class SendLiveChatMessageEndpoint
    {
        public string clientIdPrefix { get; set; }
        public string @params { get; set; }
        public List<ActionForPostComment> actions { get; set; }
    }

    public class ServiceEndpoint
    {
        public WebSerializedServiceEndpointExtension2 webSerializedServiceEndpointExtension { get; set; }
        public SendLiveChatMessageEndpoint sendLiveChatMessageEndpoint { get; set; }
        public string clickTrackingParams { get; set; }
    }

    public class ButtonRenderer6
    {
        public string trackingParams { get; set; }
        public Accessibility4 accessibility { get; set; }
        public ServiceEndpoint serviceEndpoint { get; set; }
        public Icon icon { get; set; }
    }

    public class SendButton
    {
        public ButtonRenderer6 buttonRenderer { get; set; }
    }










    public class LiveChatMessageInputRenderer
    {
        public SendButton sendButton { get; set; }
    }

    public class ActionPanel
    {
        public LiveChatMessageInputRenderer liveChatMessageInputRenderer { get; set; }
    }

    public class Run21
    {
        public string text { get; set; }
    }

    public class Amount
    {
        public List<Run21> runs { get; set; }
    }


    public class LiveChatPaidMessageRenderer
    {
        public string id { get; set; }
        public Accessibility contextMenuAccessibility { get; set; }
        public long timestampColor { get; set; }
        public ContextMenuEndpoint contextMenuEndpoint { get; set; }
        public long headerTextColor { get; set; }
        public string timestampUsec { get; set; }
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

    public class Renderer
    {
        public LiveChatPaidMessageRenderer liveChatPaidMessageRenderer { get; set; }
    }

    public class ShowLiveChatItemEndpoint
    {
        public Renderer renderer { get; set; }
    }

    public class ShowItemEndpoint
    {
        public ShowLiveChatItemEndpoint showLiveChatItemEndpoint { get; set; }
        public string clickTrackingParams { get; set; }
    }


    public class LiveChatTickerPaidMessageItemRenderer
    {
        public Amount amount { get; set; }
        public long amountTextColor { get; set; }
        public long endBackgroundColor { get; set; }
        public int durationSec { get; set; }
        public ShowItemEndpoint showItemEndpoint { get; set; }
        public int fullDurationSec { get; set; }
        public AuthorPhoto authorPhoto { get; set; }
        public string authorExternalChannelId { get; set; }
        public long startBackgroundColor { get; set; }
        public string id { get; set; }
    }

    public class Item2
    {
        public LiveChatTickerPaidMessageItemRenderer liveChatTickerPaidMessageItemRenderer { get; set; }
    }

    public class AddLiveChatTickerItemAction
    {
        public Item2 item { get; set; }
        public string durationSec { get; set; }
    }

    public class AccessibilityData6
    {
        public string label { get; set; }
    }

    public class ContextMenuAccessibility2
    {
        public AccessibilityData6 accessibilityData { get; set; }
    }


    public class LiveChatItemContextMenuEndpoint2
    {
        public string @params { get; set; }
    }


    public class Action2
    {
        public AddLiveChatTickerItemAction addLiveChatTickerItemAction { get; set; }
        public AddChatItemAction addChatItemAction { get; set; }
    }

    public class Icon7
    {
        public string iconType { get; set; }
    }

    public class ButtonRenderer9
    {
        public Accessibility accessibilityData { get; set; }
        public string style { get; set; }
        public string trackingParams { get; set; }
        public Icon7 icon { get; set; }
    }

    public class MoreCommentsBelowButton
    {
        public ButtonRenderer9 buttonRenderer { get; set; }
    }

    public class LiveChatItemListRenderer
    {
        public MoreCommentsBelowButton moreCommentsBelowButton { get; set; }
        public int maxItemsToDisplay { get; set; }
    }

    public class ItemList
    {
        public LiveChatItemListRenderer liveChatItemListRenderer { get; set; }
    }






    public class Icon8
    {
        public string iconType { get; set; }
    }

    public class LiveChatAuthorBadgeRenderer2
    {
        public Accessibility accessibility { get; set; }
        public string tooltip { get; set; }
        public Icon8 icon { get; set; }
    }

    public class AuthorBadge2
    {
        public LiveChatAuthorBadgeRenderer2 liveChatAuthorBadgeRenderer { get; set; }
    }

    public class LiveChatParticipantRenderer
    {
        public Text authorName { get; set; }
        public AuthorPhoto authorPhoto { get; set; }
        public List<AuthorBadge2> authorBadges { get; set; }
    }

    public class Participant
    {
        public LiveChatParticipantRenderer liveChatParticipantRenderer { get; set; }
    }



    public class ButtonRenderer10
    {
        public Accessibility accessibilityData { get; set; }
        public string trackingParams { get; set; }
        public Icon icon { get; set; }
    }

    public class BackButton
    {
        public ButtonRenderer10 buttonRenderer { get; set; }
    }

    public class Run28
    {
        public string text { get; set; }
    }

    public class Title3
    {
        public List<Run28> runs { get; set; }
    }

    public class LiveChatParticipantsListRenderer
    {
        public List<Participant> participants { get; set; }
        public BackButton backButton { get; set; }
        public Title3 title { get; set; }
    }

    public class ParticipantsList
    {
        public LiveChatParticipantsListRenderer liveChatParticipantsListRenderer { get; set; }
    }

    public class LiveChatTickerRenderer
    {
        public bool sentinel { get; set; }
    }

    public class Ticker
    {
        public LiveChatTickerRenderer liveChatTickerRenderer { get; set; }
    }

    public class Run29
    {
        public string text { get; set; }
    }

    public class ReconnectedMessage
    {
        public List<Run29> runs { get; set; }
    }

    public class Run30
    {
        public string text { get; set; }
    }

    public class ReconnectMessage
    {
        public List<Run30> runs { get; set; }
    }

    public class Run31
    {
        public string text { get; set; }
    }

    public class FatalError
    {
        public List<Run31> runs { get; set; }
    }

    public class Run32
    {
        public string text { get; set; }
    }

    public class UnableToReconnectMessage
    {
        public List<Run32> runs { get; set; }
    }

    public class Run33
    {
        public string text { get; set; }
    }

    public class GenericError
    {
        public List<Run33> runs { get; set; }
    }

    public class ClientMessages
    {
        public ReconnectedMessage reconnectedMessage { get; set; }
        public ReconnectMessage reconnectMessage { get; set; }
        public FatalError fatalError { get; set; }
        public UnableToReconnectMessage unableToReconnectMessage { get; set; }
        public GenericError genericError { get; set; }
    }

    public class LiveChatRenderer
    {
        public List<Continuation> continuations { get; set; }
        public List<Emoji> emojis { get; set; }
        //不要
        //public Header header { get; set; }
        public ActionPanel actionPanel { get; set; }
        public List<Action> actions { get; set; }
        public ItemList itemList { get; set; }
        public string viewerName { get; set; }
        public ParticipantsList participantsList { get; set; }
        public string trackingParams { get; set; }
        public Ticker ticker { get; set; }
        public ClientMessages clientMessages { get; set; }
    }

    public class Contents
    {
        public LiveChatRenderer liveChatRenderer { get; set; }
    }

    public class YtInitialData
    {
        public string trackingParams { get; set; }
        public ResponseContext responseContext { get; set; }
        public Contents contents { get; set; }
    }

}
namespace YoutubeLib
{
    /// <summary>
    /// base class
    /// </summary>
    [Serializable]
    public class YtException : Exception
    {
        public YtException()
        { }

        public YtException(string message)
            : base(message)
        { }

        public YtException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected YtException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
    [Serializable]
    public class InvalidUrlException : YtException
    {
        public InvalidUrlException(string message)
            : base(message)
        { }

        public InvalidUrlException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected InvalidUrlException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
    /// <summary>
    /// 仕様変更
    /// </summary>
    [Serializable]
    public class SpecChangedException : YtException
    {
        public SpecChangedException()
        { }

        public SpecChangedException(string message)
            : base(message)
        { }

        public SpecChangedException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected SpecChangedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
    [Serializable]
    public class NotLoggedinException : YtException
    {
        public NotLoggedinException()
        { }

        public NotLoggedinException(string message)
            : base(message)
        { }

        public NotLoggedinException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected NotLoggedinException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
    [Serializable]
    public class ParseException : YtException
    {
        public ParseException()
        { }

        public ParseException(string message)
            : base(message)
        { }

        public ParseException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected ParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
    public class Run
    {
        public string text { get; set; }
        public override string ToString()
        {
            return text;
        }
    }
    public class Text
    {
        public List<Run> runs { get; set; }
        public string simpleText { get; set; }
        public override string ToString()
        {
            if (runs != null)
                return string.Join("", runs);//2017/08/05区切り文字無しが妥当。一文字ずつ分割されて来る場合があった。
            else
                return simpleText;
        }
    }
    public class Rung
    {
        public string text { get; set; }
        public NavigationEndpoint navigationEndpoint { get; set; }
        public override string ToString()
        {
            return text;
        }
    }


    public class AuthorPhoto
    {
        public List<Thumbnail2> thumbnails { get; set; }
    }



    public class Message
    {
        public List<Run> runs { get; set; }
        public string simpleText { get; set; }
    }

    public class AccessibilityData
    {
        public string label { get; set; }
    }

    public class Accessibility
    {
        public AccessibilityData accessibilityData { get; set; }
    }


    public class AuthorName
    {
        public List<Run> runs { get; set; }
    }

    public class LiveChatItemContextMenuEndpoint
    {
        public string @params { get; set; }
    }

    public class ContextMenuEndpoint
    {
        public string clickTrackingParams { get; set; }
        public LiveChatItemContextMenuEndpoint liveChatItemContextMenuEndpoint { get; set; }
    }

    public class Icon
    {
        public string iconType { get; set; }
    }

    public class LiveChatAuthorBadgeRenderer
    {
        public Icon icon { get; set; }
        public string tooltip { get; set; }
        public Accessibility accessibility { get; set; }
    }

    public class AuthorBadge
    {
        public LiveChatAuthorBadgeRenderer liveChatAuthorBadgeRenderer { get; set; }
    }

    public class LiveChatTextMessageRenderer
    {
        public string id { get; set; }
        public AuthorPhoto authorPhoto { get; set; }
        [JsonProperty("message")]
        public Text Message { get; set; }
        public Accessibility contextMenuAccessibility { get; set; }
        public Text authorName { get; set; }
        public string timestampUsec { get; set; }
        public string authorExternalChannelId { get; set; }
        public ContextMenuEndpoint contextMenuEndpoint { get; set; }
        public List<AuthorBadge> authorBadges { get; set; }
    }
    public class LiveChatPlaceholderItemRenderer
    {
        public string id { get; set; }
        public string timestampUsec { get; set; }
    }
    public class Item
    {
        public LiveChatTextMessageRenderer liveChatTextMessageRenderer { get; set; }
        public LiveChatPaidMessageRenderer liveChatPaidMessageRenderer { get; set; }
        public LiveChatPlaceholderItemRenderer liveChatPlaceholderItemRenderer { get; set; }
    }

    public class AddChatItemAction
    {
        public string clientId { get; set; }
        public Item item { get; set; }
    }

    public class DeletedStateMessage
    {
        public List<Run> runs { get; set; }
    }

    public class MarkChatItemsByAuthorAsDeletedAction
    {
        public DeletedStateMessage deletedStateMessage { get; set; }
        public string externalChannelId { get; set; }
    }


    public class DeletedStateMessage2
    {
        public List<Run> runs { get; set; }
    }

    public class MarkChatItemAsDeletedAction
    {
        public DeletedStateMessage2 deletedStateMessage { get; set; }
        public string targetItemId { get; set; }
    }
    public class ReplacementItem
    {
        public LiveChatPlaceholderItemRenderer liveChatPlaceholderItemRenderer { get; set; }
    }
    public class ReplaceChatItemAction
    {
        public string targetItemId { get; set; }
        public ReplacementItem replacementItem { get; set; }
    }
    public class Action
    {
        public AddChatItemAction addChatItemAction { get; set; }
        public MarkChatItemsByAuthorAsDeletedAction markChatItemsByAuthorAsDeletedAction { get; set; }
        public MarkChatItemAsDeletedAction markChatItemAsDeletedAction { get; set; }
        public AddLiveChatTickerItemAction addLiveChatTickerItemAction { get; set; }
        public AddLiveChatTextMessageFromTemplateAction addLiveChatTextMessageFromTemplateAction { get; set; }
        public ReplaceChatItemAction replaceChatItemAction { get; set; }
    }
    /// <summary>
    /// コメント投稿時に必要なActionだけにした。Serialize時に余計なものが入るとコメント投稿に失敗する
    /// </summary>
    public class ActionForPostComment
    {
        public AddLiveChatTextMessageFromTemplateAction addLiveChatTextMessageFromTemplateAction { get; set; }
    }
    public class TimedContinuationData
    {
        public string clickTrackingParams { get; set; }
        public string continuation { get; set; }
        public int timeoutMs { get; set; }
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
    public class Continuation
    {
        public TimedContinuationData timedContinuationData { get; set; }
        public InvalidationContinuationData invalidationContinuationData { get; set; }
    }

    public class LiveChatContinuation
    {
        public List<Action> actions { get; set; }
        public string trackingParams { get; set; }
        public List<Continuation> continuations { get; set; }
    }

    public class ContinuationContents
    {
        public LiveChatContinuation liveChatContinuation { get; set; }
    }

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

    public class WebNavigationEndpointData
    {
        public string url { get; set; }
    }

    public class UrlEndpoint
    {
        public string url { get; set; }
    }

    public class NavigationEndpoint
    {
        public WebNavigationEndpointData webNavigationEndpointData { get; set; }
        public UrlEndpoint urlEndpoint { get; set; }
    }



    public class Disclaimer
    {
        public List<Rung> runs { get; set; }
    }

    public class ButtonRenderer
    {
        public string size { get; set; }
        public string style { get; set; }
        public Text text { get; set; }
        public bool isDisabled { get; set; }
    }

    public class SubmitButton
    {
        public ButtonRenderer buttonRenderer { get; set; }
    }

    public class DismissButton
    {
        public ButtonRenderer buttonRenderer { get; set; }
    }

    public class ButtonRenderer3
    {
        public string size { get; set; }
        public Icon icon { get; set; }
        public string style { get; set; }
        public bool isDisabled { get; set; }
    }

    public class CloseButton
    {
        public ButtonRenderer3 buttonRenderer { get; set; }
    }

    public class ButtonRenderer4
    {
        public string size { get; set; }
        public string style { get; set; }
        public Text text { get; set; }
        public bool isDisabled { get; set; }
    }

    public class CancelButton
    {
        public ButtonRenderer4 buttonRenderer { get; set; }
    }

    public class YtConfigData
    {
        public string visitorData { get; set; }
        public int sessionIndex { get; set; }
        public string csn { get; set; }
        public string delegatedSessionId { get; set; }
    }

    public class WebResponseContextExtensionData
    {
        public YtConfigData ytConfigData { get; set; }
        //不要
        //public FeedbackDialog feedbackDialog { get; set; }
    }
    public class Error
    {
        public string domain { get; set; }
        public string externalErrorMessage { get; set; }
        public string debugInfo { get; set; }
        public string code { get; set; }
    }
    public class Errors
    {
        public List<Error> errors { get; set; }
    }
    public class ResponseContext
    {
        public Errors errors { get; set; }
        public WebResponseContextExtensionData webResponseContextExtensionData { get; set; }
        public List<ServiceTrackingParam> serviceTrackingParams { get; set; }
        public string Cver
        {
            get
            {
                return GetValue("CSI", "cver");
            }
        }
        public string Logged_in
        {
            get { return GetValue("GUIDED_HELP", "logged_in"); }
        }
        public string Creator_channel_id
        {
            get { return GetValue("GUIDED_HELP", "creator_channel_id"); }
        }
        private Dictionary<Tuple<string, string>, string> _dict;
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>今のところ被っているキーが無いため、service毎の分割はしていない</remarks>
        public IDictionary<Tuple<string, string>, string> Dict
        {
            get
            {
                if (_dict == null)
                {
                    _dict = new Dictionary<Tuple<string, string>, string>();
                    foreach (var param in serviceTrackingParams)
                    {
                        foreach (var m in param.@params)
                        {
                            _dict.Add(new Tuple<string, string>(param.service, m.key), m.value);
                        }
                    }
                }
                return _dict;
            }
        }

        private string GetValue(string service, string key)
        {
            foreach (var param in serviceTrackingParams)
            {
                if (param.service == service)
                {
                    foreach (var m in param.@params)
                    {
                        if (m.key == key)
                        {
                            return m.value;
                        }
                    }
                }
            }
            throw new SpecChangedException();
        }
    }

    public class Response
    {
        public ContinuationContents continuationContents { get; set; }
        public ResponseContext responseContext { get; set; }
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
    public class LivePageContext
    {
        public string ChatPageUrl { get; internal set; }
        public string innertube_api_key { get; set; }

        public string vid { get; set; }

    }

    public class Data
    {
        public ResponseContext responseContext { get; set; }
        public List<Action> actions { get; set; }
    }

    public class CommentPostResponse
    {
        public Data data { get; set; }
        public string code { get; set; }
    }
    public static class API
    {
        private static LivePageContext ParseLivePage(string html)
        {
            var context = new LivePageContext();
            var match0 = Regex.Match(html, "\"innertube_api_key\":\"(?<api_key>[^\"]+)\"");
            if (match0.Success)
            {
                context.innertube_api_key = match0.Groups["api_key"].Value;
            }
            var match1 = Regex.Match(html, "href=\"\\/watch\\?v=(?<vid>[^\"]+)\"");
            if (match1.Success)
            {
                context.vid = match1.Groups["vid"].Value;
            }
            var match2 = Regex.Match(html, "src=\"(?<chatpageurl>https://www.youtube.com/live_chat\\?continuation=[^\"]+)\"");
            if (match2.Success)
            {
                context.ChatPageUrl = match2.Groups["chatpageurl"].Value;
            }
            return context;
        }
        public static async Task<LiveContext> GetLiveContextAsync(IDataSource dataSource, string vid, CookieContainer cc)
        {
            //2017/09/14 放送開始時刻が取れなくなってしまっている。
            var url = $"https://www.youtube.com/watch?v={vid}";
            var headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36"),
            };
            var html = await dataSource.GetAsync(url, headers, cc);
            var context = new LiveContext();
            var match0 = Regex.Match(html, "meta itemprop=\"startDate\" content=\"(?<start>[\\d-:+A-Z]+)\"");
            if (match0.Success)
            {
                var start = match0.Groups["start"].Value;
                context.StartDate = DateTime.Parse(start);
            }
            return context;
        }
        public static async Task<(RootObject, string)> GetLiveChat(IDataSource dataSource, string continuation, AccessContext context, CookieContainer cc)
        {
            if (string.IsNullOrWhiteSpace(continuation))
                throw new ArgumentNullException(nameof(continuation));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var url = $"https://www.youtube.com/live_chat/get_live_chat?continuation={continuation}&pbj=1";
            var headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("User-Agent", context.UserAgent),//User-Agentが無いとお使いのブラウザは古いようですというHTMLが返って来てしまう。
                new KeyValuePair<string, string>("X-YouTube-Client-Name", "1"),//必須
                new KeyValuePair<string, string>("X-YouTube-Client-Version", context.ClientVersion),//必須
                new KeyValuePair<string, string>("X-YouTube-Identity-Token", context.Identity_Token),//必須
                new KeyValuePair<string, string>("X-YouTube-Page-CL", context.Page_CL),//必須
                //new KeyValuePair<string, string>("X-YouTube-Page-Label", context.Page_Label),
                //new KeyValuePair<string, string>("X-YouTube-Variants-Checksum", context.Variants_Checksum),
            };
            //必要なヘッダが無いと送られてくる
            //{"reload":"now"}
            var str = await dataSource.GetAsync(url, headers, cc);
#if LOGGING
            using (var sr = new System.IO.StreamWriter("before_get_live_chat.txt", false))
            {
                sr.Write(str);
            }
#endif
#if LOGGING
            if (dataSource is YoutubeServer)
            {
                await JsonSQLiteLogger.LogToSQLite(logDir + "log_GetLiveChat.db", str).ConfigureAwait(false);
            }
#endif
            var getLiveChat = JsonConvert.DeserializeObject<RootObject>(str);
            return (getLiveChat, str);
        }
        public static async Task<UpdatedMetadata.UpdatedMetadata> PostUpdatedMetadata(IDataSource dataSource, string innertube_api_key, string vid, CookieContainer cc)
        {
            var url = $"https://www.youtube.com/youtubei/v1/updated_metadata?alt=json&key={innertube_api_key}";
            var headers = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Accept-Language","ja"),
                new KeyValuePair<string, string>("Accept-Encoding","gzip, deflate, br"),
                new KeyValuePair<string, string>("Content-Type","application/json"),
            };
            var data = $"{{\"context\":{{\"client\":{{\"hl\":\"ja\",\"gl\":\"JP\",\"clientName\":1,\"clientVersion\":\"1.20170427\"}}}},\"videoId\":\"{vid}\"}}";
            var str = await dataSource.PostAsync(url, headers, data, cc);
#if LOGGING
            if (dataSource is YoutubeServer)
            {
                await JsonSQLiteLogger.LogToSQLite(logDir + "log_PostUpdatedMetadata.db", str).ConfigureAwait(false);
            }
#endif
            var res = JsonConvert.DeserializeObject<UpdatedMetadata.UpdatedMetadata>(str);
            return res;
        }
        public static async Task<string> GetVid(IDataSource dataSource, string input)
        {
            //37zAMOpy6QE
            //https://www.youtube.com/watch?v=37zAMOpy6QE
            //https://www.youtube.com/c/YukaYuka2525
            //https://www.youtube.com/channel/UCqXtqB7BTY25tZfJ0AFSnmQ

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new InvalidUrlException("");
            }
            string channelId = "";
            if (!input.Contains("/"))
            {
                //URLでは無さそうなら入力値はvidであると見做す。
                return input;
            }
            var match0 = Regex.Match(input, "watch\\?v=(?<vid>[^/\"?&]+)");
            if (match0.Success)
            {
                return match0.Groups["vid"].Value;
            }
            var match1 = Regex.Match(input, "/c/(?<userid>[^/\"?]+)");
            if (match1.Success)
            {
                var userId = match1.Groups["userid"].Value;
                var html = await dataSource.GetAsync($"https://www.youtube.com/c/{userId}", null, null);
                var match2 = Regex.Match(html, "property=\"og:url\" content=\"https://www\\.youtube\\.com/channel/(?<channelid>[^/\"?]+)\">");
                if (match2.Success)
                {
                    channelId = match2.Groups["channelid"].Value;
                }
            }
            if (string.IsNullOrEmpty(channelId))
            {
                var match3 = Regex.Match(input, "channel/(?<channelid>[^/\"?]+)");
                if (match3.Success)
                {
                    channelId = match3.Groups["channelid"].Value;
                }
                else
                {
                    throw new InvalidUrlException("");
                }
            }
            return await GetLiveIdFromChannelId(dataSource, channelId);
        }
        public static async Task<string> GetLiveIdFromChannelId(IDataSource dataSource, string channelId)
        {
            var url = $"https://www.youtube.com/channel/{channelId}/videos?flow=list&view=2";
            string html;
            try
            {
                html = await dataSource.GetAsync(url, null, null);
            }
            catch (WebException)
            {
                throw new YtException("入力されたchannelIdは存在しない");
            }
            var match = Regex.Match(html, "href=\"\\/watch\\?v=(?<vid>[^\"]+)\"");
            if (match.Success)
            {
                return match.Groups["vid"].Value;
            }
            throw new YtException("放送IDが見つからなかった");//放送中ではないもしくは仕様変更
        }
    }
    public abstract class IDataSource
    {
        internal abstract Task<string> GetAsync(string url, List<KeyValuePair<string, string>> headers, CookieContainer cc);
        internal abstract Task<string> PostAsync(string url, List<KeyValuePair<string, string>> headers,
            List<KeyValuePair<string, string>> data, CookieContainer cc);
        internal abstract Task<string> PostAsync(string url, List<KeyValuePair<string, string>> headers, string data, CookieContainer cc);
    }
    public class YoutubeServer : IDataSource
    {
        internal override async Task<string> GetAsync(string url, List<KeyValuePair<string, string>> headers, CookieContainer cc)
        {
            var res = await ryu_s.Net.Http.GetAsync(url, headers, null, cc);
            string str;
            using (var sr = new StreamReader(res.Value.GetResponseStream()))
            {
                str = sr.ReadToEnd();
            }
            return str;
        }
        internal override async Task<string> PostAsync(string url, List<KeyValuePair<string, string>> headers,
            List<KeyValuePair<string, string>> data, CookieContainer cc)
        {
            var res = await ryu_s.Net.Http.PostAsync(url, headers, data, cc);
            string str;
            using (var sr = new StreamReader(res.Value.GetResponseStream()))
            {
                str = sr.ReadToEnd();
            }
            return str;
        }

        internal override async Task<string> PostAsync(string url, List<KeyValuePair<string, string>> headers, string data, CookieContainer cc)
        {
            var res = await ryu_s.Net.Http.PostAsync(url, headers, data, cc);
            string str;
            using (var sr = new StreamReader(res.Value.GetResponseStream()))
            {
                str = sr.ReadToEnd();
            }
            return str;
        }
    }
}
namespace YoutubeLib.UpdatedMetadata
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

    public class ResponseContext
    {
        public List<ServiceTrackingParam> serviceTrackingParams { get; set; }
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
    }

    public class Run
    {
        public string text { get; set; }
        public override string ToString()
        {
            return text;
        }
    }

    public class ViewCount
    {
        public List<Run> runs { get; set; }
        public override string ToString()
        {
            try
            {
                return string.Join(" ", runs.Select(r => r.text.ToString()));
            }
            catch (Exception ex)
            {
                var b = runs == null;
                //ryu_s.MyCommon.ExceptionLogger.Logging(ryu_s.MyCommon.LogLevel.error, ex, $"runs is null: {b}");
            }
            return "";
        }
    }

    public class VideoViewCountRenderer
    {
        public ViewCount viewCount { get; set; }
        public bool isLive { get; set; }
    }

    public class Viewership
    {
        public VideoViewCountRenderer videoViewCountRenderer { get; set; }
    }

    public class UpdateViewershipAction
    {
        public Viewership viewership { get; set; }
    }

    public class Target
    {
        public string videoId { get; set; }
    }

    public class LikeCountText
    {
        public List<Run> runs { get; set; }
    }



    public class LikeCountWithLikeText
    {
        public List<Run> runs { get; set; }
    }



    public class DislikeCountText
    {
        public List<Run> runs { get; set; }
    }



    public class DislikeCountWithDislikeText
    {
        public List<Run> runs { get; set; }
    }

    public class LikeButtonRenderer
    {
        public Target target { get; set; }
        public string likeStatus { get; set; }
        public int likeCount { get; set; }
        public LikeCountText likeCountText { get; set; }
        public LikeCountWithLikeText likeCountWithLikeText { get; set; }
        public int dislikeCount { get; set; }
        public DislikeCountText dislikeCountText { get; set; }
        public DislikeCountWithDislikeText dislikeCountWithDislikeText { get; set; }
        public string trackingParams { get; set; }
        public bool likesAllowed { get; set; }
    }

    public class SentimentRenderer
    {
        public LikeButtonRenderer likeButtonRenderer { get; set; }
    }

    public class UpdateSentimentAction
    {
        public SentimentRenderer sentimentRenderer { get; set; }
    }

    public class Title
    {
        public string simpleText { get; set; }
        public List<Run> runs { get; set; }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(simpleText))
            {
                return simpleText;
            }
            if (runs == null)
            {
                return "";
            }
            else
            {
                return string.Join(" ", runs);
            }
        }
    }

    public class UpdateTitleAction
    {
        public Title title { get; set; }
    }

    public class Run7
    {
        public string text { get; set; }
    }

    public class DateText
    {
        public List<Run7> runs { get; set; }
    }

    public class UpdateDateTextAction
    {
        public DateText dateText { get; set; }
    }

    public class UrlEndpoint
    {
        public string url { get; set; }
        public string target { get; set; }
    }

    public class NavigationEndpoint
    {
        public string clickTrackingParams { get; set; }
        public UrlEndpoint urlEndpoint { get; set; }
    }

    public class Run8
    {
        public string text { get; set; }
        public NavigationEndpoint navigationEndpoint { get; set; }
    }

    public class Description
    {
        public List<Run8> runs { get; set; }
    }

    public class UpdateDescriptionAction
    {
        public Description description { get; set; }
    }

    public class Action
    {
        public UpdateViewershipAction updateViewershipAction { get; set; }
        public UpdateSentimentAction updateSentimentAction { get; set; }
        public UpdateTitleAction updateTitleAction { get; set; }
        public UpdateDateTextAction updateDateTextAction { get; set; }
        public UpdateDescriptionAction updateDescriptionAction { get; set; }
    }

    public class UpdatedMetadata
    {
        public ResponseContext responseContext { get; set; }
        public Continuation continuation { get; set; }
        public List<Action> actions { get; set; }
    }
}
namespace ryu_s.Net
{
    public class HttpResponse<T>
    {
        public T Value { get; private set; }
        public CookieContainer CC { get; private set; }
        public HttpResponse(T value, CookieContainer cc)
        {
            Value = value;
            CC = cc;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public static class Http
    {
        private enum Method
        {
            GET,
            POST,
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="data"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        /// <exception cref="System.OperationCanceledException"></exception>
        public static async Task<ryu_s.Net.HttpResponse<HttpWebResponse>> GetAsync(string url, IEnumerable<KeyValuePair<string, string>> headers,
            IEnumerable<KeyValuePair<string, string>> data, CookieContainer cc, CancellationToken ct = default(CancellationToken))
        {
            return await HttpBaseAsync(Method.GET, url, headers, data, cc, ct);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="data"></param>
        /// <param name="cc"></param>
        /// <param name="enc">サーバからのレスポンスの文字コード</param>
        /// <returns></returns>
        public static async Task<ryu_s.Net.HttpResponse<string>> GetAsync(string url, IEnumerable<KeyValuePair<string, string>> headers,
            IEnumerable<KeyValuePair<string, string>> data, CookieContainer cc, Encoding enc, CancellationToken ct = default(CancellationToken))
        {
            var res = await GetAsync(url, headers, data, cc, ct);
            using (var sr = new StreamReader(res.Value.GetResponseStream(), enc))
            {
                return new HttpResponse<string>(await sr.ReadToEndAsync(), res.CC);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="data"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        /// <exception cref="System.OperationCanceledException"></exception>
        public static async Task<ryu_s.Net.HttpResponse<HttpWebResponse>> PostAsync(string url, IEnumerable<KeyValuePair<string, string>> headers,
            IEnumerable<KeyValuePair<string, string>> data, CookieContainer cc, CancellationToken ct = default(CancellationToken))
        {
            return await HttpBaseAsync(Method.POST, url, headers, data, cc, ct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="data"></param>
        /// <param name="cc"></param>
        /// <param name="enc">サーバからのレスポンスの文字コード</param>
        /// <returns></returns>
        /// <exception cref="System.OperationCanceledException"></exception>
        public static async Task<ryu_s.Net.HttpResponse<string>> PostAsync(string url, IEnumerable<KeyValuePair<string, string>> headers,
            IEnumerable<KeyValuePair<string, string>> data, CookieContainer cc, Encoding enc, CancellationToken ct = default(CancellationToken))
        {
            var res = await PostAsync(url, headers, data, cc, ct);
            using (var sr = new StreamReader(res.Value.GetResponseStream(), enc))
            {
                return new HttpResponse<string>(await sr.ReadToEndAsync(), res.CC);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="headers">null可</param>
        /// <param name="data">null可</param>
        /// <param name="cc">null可</param>
        /// <returns></returns>
        /// <exception cref="System.Net.WebException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="System.OperationCanceledException"></exception>
        private static async Task<ryu_s.Net.HttpResponse<HttpWebResponse>> HttpBaseAsync(Method method, string url, IEnumerable<KeyValuePair<string, string>> headers,
            IEnumerable<KeyValuePair<string, string>> data, CookieContainer cc, CancellationToken ct = default(CancellationToken))
        {
            var dataStr = string.Empty;
            if (data != null)
                dataStr = string.Join("&", data.Select(pair => $"{pair.Key}={pair.Value}"));
            if (method == Method.GET)
            {
                if (!string.IsNullOrEmpty(dataStr))
                    url += "?" + dataStr;
            }
            var req = WebRequest.Create(url) as HttpWebRequest;
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            //req.ServicePoint.Expect100Continue = false;
            //req.Timeout = 100000;
            //req.ReadWriteTimeout = 100000;
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (header.Key == "Content-Type")
                    {
                        req.ContentType = header.Value;
                    }
                    else if (header.Key == "User-Agent")
                    {
                        req.UserAgent = header.Value;
                    }
                    else if (header.Key == "Referer")
                    {
                        req.Referer = header.Value;
                    }
                    else if (header.Key == "KeepAlive")
                    {
                        req.KeepAlive = true;
                    }
                    else if (header.Key == "Accept")
                    {
                        req.Accept = header.Value;
                    }
                    else
                    {
                        req.Headers[header.Key] = header.Value;
                    }
                }
            }
            var reqCc = new CookieContainer();
            if (cc != null)
            {
                var reqCookies = cc.GetCookies(new Uri(url));
                reqCc.Add(reqCookies);
            }
            req.CookieContainer = reqCc;

            if (method == Method.POST)
            {
                var postDataBytes = Encoding.ASCII.GetBytes(dataStr);
                req.Method = "POST";
                if (string.IsNullOrEmpty(req.ContentType))
                    req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = postDataBytes.Length;
                using (var requestStream = await Task<Stream>.Factory.FromAsync(req.BeginGetRequestStream, req.EndGetRequestStream, req))
                {
                    await requestStream.WriteAsync(postDataBytes, 0, postDataBytes.Length);
                }
            }
            var cts = new CancellationTokenSource();
            var t2 = Task.Factory.StartNew(() =>
            {
                while (!ct.IsCancellationRequested && !cts.Token.IsCancellationRequested)
                {
                    Task.Delay(100, ct).Wait();
                }
            }, cts.Token);

            var t1 = Task<WebResponse>.Factory.FromAsync(req.BeginGetResponse, req.EndGetResponse, req);
            var t = await Task.WhenAny(t1, t2);
            if (t == t1)
            {
                cts.Cancel();
                var res = await t1 as HttpWebResponse;
                CookieContainer resCc;
                if (cc != null)
                {
                    cc.Add(res.Cookies);
                    resCc = cc;
                }
                else
                {
                    resCc = new CookieContainer();
                    resCc.Add(res.Cookies);
                }
                var myResponse = new ryu_s.Net.HttpResponse<HttpWebResponse>(res, cc);
                return myResponse;
            }
            else
            {
                cts.Cancel();
                //ここに来る時はct.IsCancellationRequested == true であると想定
                ct.ThrowIfCancellationRequested();
            }
            throw new NotImplementedException("ここに来たらバグ");
        }

        /// <summary>
        /// 
        /// JsonをPostしたい時があったため作った。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="data"></param>
        /// <param name="cc"></param>
        /// <returns></returns>
        /// <exception cref="System.OperationCanceledException"></exception>
        public static async Task<ryu_s.Net.HttpResponse<HttpWebResponse>> PostAsync(string url, IEnumerable<KeyValuePair<string, string>> headers,
            string data, CookieContainer cc, CancellationToken ct = default(CancellationToken))
        {
            return await HttpBaseAsync(Method.POST, url, headers, data, cc, ct);
        }
        /// <summary>
        /// 
        /// JsonをPostしたい時があったため作った。こっちの方が汎用性が高いため、こっちをベースにすべきだろう。
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <param name="headers">null可</param>
        /// <param name="data">null可</param>
        /// <param name="cc">null可</param>
        /// <returns></returns>
        /// <exception cref="System.Net.WebException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="System.OperationCanceledException"></exception>
        private static async Task<ryu_s.Net.HttpResponse<HttpWebResponse>> HttpBaseAsync(Method method, string url, IEnumerable<KeyValuePair<string, string>> headers,
            string dataStr, CookieContainer cc, CancellationToken ct = default(CancellationToken))
        {
            if (method == Method.GET)
            {
                if (!string.IsNullOrEmpty(dataStr))
                    url += "?" + dataStr;
            }
            var req = WebRequest.Create(url) as HttpWebRequest;
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            //req.ServicePoint.Expect100Continue = false;
            //req.Timeout = 100000;
            //req.ReadWriteTimeout = 100000;
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (header.Key == "Content-Type")
                    {
                        req.ContentType = header.Value;
                    }
                    else if (header.Key == "User-Agent")
                    {
                        req.UserAgent = header.Value;
                    }
                    else if (header.Key == "Referer")
                    {
                        req.Referer = header.Value;
                    }
                    else if (header.Key == "KeepAlive")
                    {
                        req.KeepAlive = true;
                    }
                    else if (header.Key == "Accept")
                    {
                        req.Accept = header.Value;
                    }
                    else
                    {
                        req.Headers[header.Key] = header.Value;
                    }
                }
            }
            var reqCc = new CookieContainer();
            if (cc != null)
            {
                var reqCookies = cc.GetCookies(new Uri(url));
                reqCc.Add(reqCookies);
            }
            req.CookieContainer = reqCc;

            if (method == Method.POST)
            {
                var postDataBytes = Encoding.ASCII.GetBytes(dataStr);
                req.Method = "POST";
                if (string.IsNullOrEmpty(req.ContentType))
                    req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = postDataBytes.Length;
                using (var requestStream = await Task<Stream>.Factory.FromAsync(req.BeginGetRequestStream, req.EndGetRequestStream, req))
                {
                    await requestStream.WriteAsync(postDataBytes, 0, postDataBytes.Length);
                }
            }
            var cts = new CancellationTokenSource();
            var t2 = Task.Factory.StartNew(() =>
            {
                while (!ct.IsCancellationRequested && !cts.Token.IsCancellationRequested)
                {
                    Task.Delay(100, ct).Wait();
                }
            }, cts.Token);

            var t1 = Task<WebResponse>.Factory.FromAsync(req.BeginGetResponse, req.EndGetResponse, req);
            var t = await Task.WhenAny(t1, t2);
            if (t == t1)
            {
                cts.Cancel();
                var res = await t1 as HttpWebResponse;
                CookieContainer resCc;
                if (cc != null)
                {
                    cc.Add(res.Cookies);
                    resCc = cc;
                }
                else
                {
                    resCc = new CookieContainer();
                    resCc.Add(res.Cookies);
                }
                var myResponse = new ryu_s.Net.HttpResponse<HttpWebResponse>(res, cc);
                return myResponse;
            }
            else
            {
                cts.Cancel();
                //ここに来る時はct.IsCancellationRequested == true であると想定
                ct.ThrowIfCancellationRequested();
            }
            throw new NotImplementedException("ここに来たらバグ");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cc"></param>
        /// <param name="filePath"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="System.OperationCanceledException"></exception>
        public static async Task GetImageAsync(string url, IEnumerable<KeyValuePair<string, string>> headers, CookieContainer cc, string filePath, CancellationToken ct)
        {
            var res = await GetAsync(url, headers, null, cc, ct);
            using (var br = new BinaryReader(res.Value.GetResponseStream()))
            {
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    const int bufSize = 1024;
                    var bytes = new byte[bufSize];
                    int totalread = 0;
                    int numread;
                    do
                    {
                        numread = br.Read(bytes, 0, bufSize);
                        totalread += numread;
                        fs.Write(bytes, 0, numread);
                    } while (numread != 0);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="outFilePath">ダウンロードしたファイルを保存するパス</param>
        /// <param name="progress"></param>
        /// <param name="timeOutMin">指定された時間の間に新たなProgress.Reportが発生しなかった場合にTimeoutExceptionを起こす。0以下の数値にすればタイムアウトしない</param>
        /// <param name="cancelToken">allow null</param>
        /// <returns></returns>        
        /// <exception cref="System.OperationCanceledException"></exception>
        public static Task DownloadFileAsync(Uri uri, string outFilePath, IProgress<FileDownloadProgress> progress, int timeOutMin, CancellationToken cancelToken)
        {
            var tcs = new TaskCompletionSource<object>();
            var client = new WebClient();
            System.Timers.Timer timeOutTimer = null;
            System.Timers.Timer cancelTimer = null;
            const int cancelTokenWatchingInterval = 500;
            bool enableToRaiseTimeout = (timeOutMin > 0);

            if (enableToRaiseTimeout)
            {
                timeOutTimer = new System.Timers.Timer();
                timeOutTimer.Interval = timeOutMin * 60 * 1000;
                timeOutTimer.AutoReset = false;//raises the Elapsed event only once
                timeOutTimer.Elapsed += (sender, e) =>
                {
                    client.CancelAsync();
                    tcs.TrySetException(new TimeoutException());
                };
            }
            if (cancelToken != CancellationToken.None)
            {
                cancelTimer = new System.Timers.Timer();
                cancelTimer.Interval = cancelTokenWatchingInterval;
                cancelTimer.Elapsed += (sender, e) =>
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        client.CancelAsync();
                        tcs.TrySetCanceled();
                    }
                };
                cancelTimer.Start();
            }
            client.DownloadProgressChanged += (sender, e) =>
            {
                if (enableToRaiseTimeout)
                {
                    //reset timer
                    timeOutTimer.Stop();
                    timeOutTimer.Start();
                }
                progress.Report(new FileDownloadProgress()
                {
                    ProgressPercentage = e.ProgressPercentage,
                    TotalBytesToReceive = e.TotalBytesToReceive,
                    BytesReceived = e.BytesReceived,
                });
            };
            client.DownloadFileCompleted += (sender, e) =>
            {
                if (enableToRaiseTimeout)
                {
                    timeOutTimer.Stop();
                }
                if (e.Cancelled)
                {
                    tcs.TrySetCanceled();
                }
                else if (e.Error != null)
                {
                    tcs.TrySetException(e.Error);
                }
                else
                {
                    tcs.TrySetResult(null);
                }
            };
            if (enableToRaiseTimeout)
            {
                timeOutTimer.Start();
            }
            //start downloading
            client.DownloadFileAsync(uri, outFilePath);

            return tcs.Task;
        }
        /// <summary>
        /// 
        /// </summary>
        public class FileDownloadProgress
        {
            public int ProgressPercentage;
            public long TotalBytesToReceive;
            public long BytesReceived;
        }
    }
}
