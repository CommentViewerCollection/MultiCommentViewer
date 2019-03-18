using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using SitePlugin;
using Common;
using System.Linq;
using Codeplex.Data;
using SitePluginCommon;
using ryu_s.BrowserCookie;
using System.Net.Http;
using System.Diagnostics;

namespace YouTubeLiveSitePlugin.Test2
{
    /// <summary>
    /// 接続が切れた理由
    /// </summary>
    enum DisconnectReason
    {
        /// <summary>
        /// 原因不明
        /// </summary>
        Unknown,
        /// <summary>
        /// 配信終了
        /// </summary>
        Finished,
        /// <summary>
        /// ユーザによる切断
        /// </summary>
        ByUser,
        /// <summary>
        /// リロードが必要
        /// </summary>
        Reload,
        /// <summary>
        /// チャットが無効
        /// </summary>
        ChatUnavailable,
        /// <summary>
        /// YtInitialDataが無かった
        /// </summary>
        YtInitialDataNotFound,
    }
    class EachConnection
    {
        private readonly ILogger _logger;
        private readonly CookieContainer _cc;
        private readonly ICommentOptions _options;
        private readonly IYouTubeLibeServer _server;
        private readonly YouTubeLiveSiteOptions _siteOptions;
        private readonly Dictionary<string, int> _userCommentCountDict;
        private readonly SynchronizedCollection<string> _receivedCommentIds;
        private readonly ICommentProvider _cp;
        private readonly IUserStore _userStore;
        ChatProvider _chatProvider;
        DisconnectReason _disconnectReason;

        public event EventHandler<IMessageContext> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler Connected;
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<DisconnectReason> ReceiveAsync(string vid, BrowserType browserType)
        {
            _disconnectReason = DisconnectReason.Unknown;
            string liveChatHtml = await GetLiveChatHtml(vid);
            string ytInitialData = ExtractYtInitialData(liveChatHtml);
            if (string.IsNullOrEmpty(ytInitialData))
            {
                //これが無いとコメントが取れないから終了
                //SendInfo("ytInitialDataの取得に失敗しました", InfoType.Error);
                return DisconnectReason.YtInitialDataNotFound;
            }
            IContinuation initialContinuation;
            List<CommentData> initialCommentData;
            try
            {
                (initialContinuation, initialCommentData) = Tools.ParseYtInitialData(ytInitialData);
            }
            catch (ContinuationNotExistsException)
            {
                //放送終了
                return DisconnectReason.Finished;
            }
            catch (ChatUnavailableException)
            {
                //SendInfo("この配信ではチャットが無効になっているようです", InfoType.Error);
                return DisconnectReason.ChatUnavailable;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "未知の例外", $"ytInitialData={ytInitialData},vid={vid}");
                return DisconnectReason.Unknown;
            }

            Connected?.Invoke(this, EventArgs.Empty);

            //直近の過去コメントを送る。
            foreach (var data in initialCommentData)
            {
                if (_receivedCommentIds.Contains(data.Id))
                {
                    continue;
                }
                else

                {
                    _receivedCommentIds.Add(data.Id);
                }
                var messageContext = CreateMessageContext(data, true);
                MessageReceived?.Invoke(this, messageContext);
            }
            //コメント投稿に必要なものの準備
            PrepareForPostingComments(liveChatHtml, ytInitialData);

            var tasks = new List<Task>();
            Task activeCounterTask = null;
            IActiveCounter<string> activeCounter = null;
            if (_options.IsActiveCountEnabled)
            {
                activeCounter = new ActiveCounter<string>()
                {
                    CountIntervalSec = _options.ActiveCountIntervalSec,
                    MeasureSpanMin = _options.ActiveMeasureSpanMin,
                };
                activeCounter.Updated += (s, e) =>
                {
                    MetadataUpdated?.Invoke(this, new Metadata { Active = e.ToString() });
                };
                activeCounterTask = activeCounter.Start();
                tasks.Add(activeCounterTask);
            }

            IMetadataProvider metaProvider = null;
            var metaTask = CreateMetadataReceivingTask(ref metaProvider, browserType, vid, liveChatHtml);
            if (metaTask != null)
            {
                tasks.Add(metaTask);
            }

            _chatProvider = new ChatProvider(_server, _logger);
            _chatProvider.ActionsReceived += (s, e) =>
            {
                foreach (var action in e)
                {
                    if (_receivedCommentIds.Contains(action.Id))
                    {
                        continue;
                    }
                    else
                    {
                        activeCounter?.Add(action.Id);
                        _receivedCommentIds.Add(action.Id);
                    }

                    var messageContext = CreateMessageContext(action, false);
                    MessageReceived?.Invoke(this, messageContext);
                }
            };
            _chatProvider.InfoReceived += (s, e) =>
            {
                SendInfo(e.Comment, e.Type);
            };
            var chatTask = _chatProvider.ReceiveAsync(vid, initialContinuation, _cc);
            tasks.Add(chatTask);

            _disconnectReason = DisconnectReason.Finished;
            while (tasks.Count > 0)
            {
                var t = await Task.WhenAny(tasks);
                if (t == metaTask)
                {
                    try
                    {
                        await metaTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex, "metaTaskが終了した原因");
                    }
                    //metaTask内でParseExceptionもしくはDisconnect()
                    //metaTaskは終わっても良い。
                    tasks.Remove(metaTask);
                    metaProvider = null;
                }
                else if (t == activeCounterTask)
                {
                    try
                    {
                        await activeCounterTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex, "activeCounterTaskが終了した原因");
                    }
                    tasks.Remove(activeCounterTask);
                    activeCounter = null;
                }
                else //chatTask
                {
                    tasks.Remove(chatTask);
                    try
                    {
                        await chatTask;
                    }
                    catch (ReloadException ex)
                    {
                        _logger.LogException(ex, "", $"vid={vid}");
                        _disconnectReason = DisconnectReason.Reload;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                        _disconnectReason = DisconnectReason.Unknown;
                    }
                    _chatProvider = null;

                    //chatTaskが終わったらmetaTaskも終了させる
                    metaProvider.Disconnect();
                    try
                    {
                        await metaTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex, "metaTaskが終了した原因");
                    }
                    tasks.Remove(metaTask);
                    metaProvider = null;

                    activeCounter?.Stop();
                    try
                    {
                        await activeCounterTask;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex, "activeCounterTaskが終了した原因");
                    }
                    tasks.Remove(activeCounterTask);
                    activeCounter = null;
                }
            }
            return _disconnectReason;
        }

        public void Disconnect()
        {
            _chatProvider?.Disconnect();
            _disconnectReason = DisconnectReason.ByUser;
        }

        private string ExtractYtInitialData(string liveChatHtml)
        {
            string ytInitialData = null;
            try
            {
                ytInitialData = Tools.ExtractYtInitialData(liveChatHtml);
            }
            catch (ParseException ex)
            {
                _logger.LogException(ex, "live_chatからのytInitialDataの抜き出しに失敗", liveChatHtml);
            }

            return ytInitialData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vid"></param>
        /// <param name="liveChatHtml"></param>
        /// <returns></returns>
        /// <exception cref="ReloadException"></exception>
        private async Task<string> GetLiveChatHtml(string vid)
        {
            string liveChatHtml;
            try
            {
                //live_chatを取得する。この中にこれから必要なytInitialDataとytcfgがある
                var liveChatUrl = $"https://www.youtube.com/live_chat?v={vid}&is_popout=1";
                liveChatHtml = await _server.GetAsync(liveChatUrl, _cc);
            }
            catch (Exception ex)
            {
                throw new ReloadException(ex);
            }

            return liveChatHtml;
        }

        public event EventHandler LoggedInStateChanged;
        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                if (_isLoggedIn == value) return;
                _isLoggedIn = value;
                LoggedInStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        bool CanPostComment => PostCommentContext != null;
        protected virtual PostCommentContext PostCommentContext { get; set; }
        int _commentPostCount;
        public async Task<bool> PostCommentAsync(string text)
        {
            var ret = false;
            if (CanPostComment)
            {
                try
                {
                    var clientMessageId = PostCommentContext.ClientIdPrefix + _commentPostCount;
                    var s = "{\"text_segments\":[{\"text\":\"" + text + "\"}]}";
                    var sej = PostCommentContext.Sej.Replace("\r\n", "").Replace("\t", "").Replace(" ", "");
                    var sessionToken = PostCommentContext.SessionToken;
                    var data = new Dictionary<string, string>
                    {
                        { "client_message_id",clientMessageId},
                        { "rich_message",s},
                        { "sej",sej},
                        { "session_token",sessionToken},
                    };
                    var url = "https://www.youtube.com/service_ajax?name=sendLiveChatMessageEndpoint";
                    var res = await _server.PostAsync(url, data, _cc);
                    Debug.WriteLine(res);
                    var json = DynamicJson.Parse(res);
                    //if (json.IsDefined("errors"))
                    //{
                    //    var k = string.Join("&", data.Select(kv => kv.Key + "=" + kv.Value));
                    //    throw new PostingCommentFailedException("コメント投稿に失敗しました（" + json.errors[0] + "）",$"data={k}, res={res}");
                    //}
                    if (json.IsDefined("code") && json.code == "SUCCESS")
                    {
                        if (json.IsDefined("data") && json.data.IsDefined("errorMessage"))
                        {
                            var k = string.Join("&", data.Select(kv => kv.Key + "=" + kv.Value));
                            var errorText = json.data.errorMessage.liveChatTextActionsErrorMessageRenderer.errorText.simpleText;
                            throw new PostingCommentFailedException("コメント投稿に失敗しました（" + errorText + "）", $"data={k}, res={res}");
                        }
                        else if (json.IsDefined("data") && json.data.IsDefined("actions"))
                        {
                            //多分成功
                            _commentPostCount++;
                            ret = true;
                            goto CommentPostsucceeded;
                        }
                    }
                    var k0 = string.Join("&", data.Select(kv => kv.Key + "=" + kv.Value));
                    throw new UnknownResponseReceivedException($"data={k0}, res={res}");
                }
                catch (UnknownResponseReceivedException ex)
                {
                    _logger.LogException(ex);
                    SendInfo(ex.Message, InfoType.Error);
                }
                catch (PostingCommentFailedException ex)
                {
                    _logger.LogException(ex);
                    SendInfo(ex.Message, InfoType.Error);
                }
                //catch (HttpException ex)
                //{
                //    //{\"errors\":[\"検証中にエラーが発生しました。\"]} statuscodeは分からないけど200以外
                //    _logger.LogException(ex, "", $"text={text},statuscode={ex.StatusCode}");
                //    SendInfo("コメント投稿時にエラーが発生", InfoType.Error);
                //}
                catch (HttpRequestException ex)
                {
                    if (ex.InnerException is WebException webEx)
                    {
                        string response;
                        using (var sr = new System.IO.StreamReader(webEx.Response.GetResponseStream()))
                        {
                            response = sr.ReadToEnd();
                        }
                        var statuscode = (int)((HttpWebResponse)webEx.Response).StatusCode;
                        //{\"errors\":[\"検証中にエラーが発生しました。\"]} statuscodeは分からないけど200以外
                        _logger.LogException(ex, "", $"text={text},statuscode={statuscode}");
                        SendInfo("コメント投稿時にエラーが発生", InfoType.Error);
                    }
                    else
                    {
                        _logger.LogException(ex, "", $"text={text}");
                        SendInfo("コメント投稿時にエラーが発生", InfoType.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                    SendInfo("コメント投稿時にエラーが発生", InfoType.Error);
                }
            }
        CommentPostsucceeded:
            return ret;
        }
        private void SendInfo(string v, InfoType error)
        {
            var message = new InfoMessage
            {
                Type = error,
                CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(v) },
                NameItems = null,
                SiteType = SiteType.YouTubeLive,
            };
            var metadata = new InfoMessageMetadata(message, _options);
            var methods = new InfoMessageMethods();
            var context = new InfoMessageContext(message, metadata, methods);
            MessageReceived?.Invoke(this, context);
        }
        private Task CreateMetadataReceivingTask(ref IMetadataProvider metaProvider, BrowserType browserType, string vid, string liveChatHtml)
        {
            Task metaTask = null;
            string ytCfg = null;
            try
            {
                ytCfg = Tools.ExtractYtcfg(liveChatHtml);
            }
            catch (ParseException ex)
            {
                _logger.LogException(ex, "live_chatからのytcfgの抜き出しに失敗", liveChatHtml);
            }
            if (!string.IsNullOrEmpty(ytCfg))
            {
                //"service_ajax?name=updatedMetadataEndpoint"はIEには対応していないらしく、400が返って来てしまう。
                //そこで、IEの場合のみ旧版の"youtubei"を使うようにした。
                if (browserType == BrowserType.IE)
                {
                    metaProvider = new MetaDataYoutubeiProvider(_server, _logger);
                }
                else
                {
                    metaProvider = new MetadataProvider(_server, _logger);
                }
                metaProvider.MetadataReceived += (s,e)=>
                {
                    MetadataUpdated?.Invoke(this, e);
                };
                metaProvider.InfoReceived += (s,e)=>
                {
                    SendInfo(e.Comment, e.Type);
                };
                metaTask = metaProvider.ReceiveAsync(ytCfg: ytCfg, vid: vid, cc: _cc);
            }

            return metaTask;
        }
        private void PrepareForPostingComments(string liveChatHtml, string ytInitialData)
        {
            var liveChatContext = Tools.GetLiveChatContext(liveChatHtml);
            IsLoggedIn = liveChatContext.IsLoggedIn;
            if (Tools.TryExtractSendButtonServiceEndpoint(ytInitialData, out string serviceEndPoint))
            {
                try
                {
                    var json = DynamicJson.Parse(serviceEndPoint);
                    PostCommentContext = new PostCommentContext
                    {
                        ClientIdPrefix = json.sendLiveChatMessageEndpoint.clientIdPrefix,
                        SessionToken = liveChatContext.XsrfToken,
                        Sej = serviceEndPoint,
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex, "", $"serviceEndPoint={serviceEndPoint}");
                }
            }
        }
        private YouTubeLiveMessageContext CreateMessageContext(CommentData commentData, bool isInitialComment)
        {
            var message = CreateMessage(commentData);
            var metadata = CreateMetadata(message, isInitialComment);
            var methods = new YouTubeLiveMessageMethods();
            if (_siteOptions.IsAutoSetNickname)
            {
                var user = metadata.User;
                var messageText = Common.MessagePartsTools.ToText(message.CommentItems);
                var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            return new YouTubeLiveMessageContext(message, metadata, methods);
        }
        private IYouTubeLiveMessage CreateMessage(CommentData data)
        {
            IYouTubeLiveMessage message;
            if (data.IsPaidMessage)
            {
                message = new YouTubeLiveSuperchat(data);
            }
            else
            {
                message = new YouTubeLiveComment(data);
            }

            return message;
        }
        private YouTubeLiveMessageMetadata CreateMetadata(IYouTubeLiveMessage message, bool isInitialComment)
        {
            string userId = null;
            if (message is IYouTubeLiveComment comment)
            {
                userId = comment.UserId;
            }
            else if (message is IYouTubeLiveSuperchat superchat)
            {
                userId = superchat.UserId;
            }
            bool isFirstComment;
            IUser user;
            if (userId != null)
            {
                if (_userCommentCountDict.ContainsKey(userId))
                {
                    _userCommentCountDict[userId]++;
                    isFirstComment = false;
                }
                else
                {
                    _userCommentCountDict.Add(userId, 1);
                    isFirstComment = true;
                }
                user = _userStore.GetUser(userId);
            }
            else
            {
                isFirstComment = false;
                user = null;
            }
            var metadata = new YouTubeLiveMessageMetadata(message, _options, _siteOptions, user, _cp, isFirstComment)
            {
                IsInitialComment = isInitialComment,
            };
            return metadata;
        }
        public EachConnection(ILogger logger, CookieContainer cc, ICommentOptions options, IYouTubeLibeServer server, 
            YouTubeLiveSiteOptions siteOptions, Dictionary<string, int> userCommentCountDict, SynchronizedCollection<string> receivedCommentIds,
            ICommentProvider cp, IUserStore userStore)
        {
            _logger = logger;
            _cc = cc;
            _options = options;
            _server = server;
            _siteOptions = siteOptions;
            _userCommentCountDict = userCommentCountDict;
            _receivedCommentIds = receivedCommentIds;
            _cp = cp;
            _userStore = userStore;
        }
    }
}
