using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using SitePluginCommon;
using SitePluginCommon.AutoReconnection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MixerSitePlugin
{
    abstract class ProviderBase<T> : IProvider
    {
        public event EventHandler<T> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        protected void RaiseMessageReceived(T message)
        {
            MessageReceived?.Invoke(this, message);
        }
        protected void RaiseMetadataReceived(IMetadata metadata)
        {
            MetadataUpdated?.Invoke(this, metadata);
        }
        public IProvider Master { get; }
        public bool IsFinished { get; }
        public Task Work { get; protected set; }
        public ProviderFinishReason FinishReason { get; }

        public abstract void Start();
        public abstract void Stop();
        //public abstract void Send(T data);
    }
    class MessageProvider2 : ProviderBase<IInternalMessage>
    {
        private readonly IWebsocket _websocket;
        private readonly ILogger _logger;
        public long ChannelId { get; set; }
        public long? MyUserId { get; set; }
        public string Token { get; set; }
        ConcurrentDictionary<long, MethodBase> _methodReplyDict = new ConcurrentDictionary<long, MethodBase>();
        public override void Start()
        {
            _methodReplyDict.Clear();
            Work = _websocket.ReceiveAsync();
        }

        public override void Stop()
        {
            _websocket.Disconnect();
        }
        public void Send(IInternalMessage message)
        {
            if (message is MethodBase method)
            {
                _methodReplyDict.AddOrUpdate(method.Id, method, (n, t) => t);
            }
            _websocket.Send(message.Raw);
        }
        public MessageProvider2(IWebsocket websocket, ILogger logger)
        {
            _websocket = websocket;
            _logger = logger;
            websocket.Opened += Websocket_Opened;
            websocket.Received += Websocket_Received;
        }

        private void Websocket_Opened(object sender, EventArgs e)
        {
        }

        private void Websocket_Received(object sender, string e)
        {
            var raw = e;
            Debug.WriteLine(raw);
            try
            {
                var internalMessage = InternalMessageParser.Parse(raw, _methodReplyDict);
                if (internalMessage is WelcomeEvent)
                {
                    //    var optOutMethod = new OptOutEventsMethod(0, new string[] { "UserJoin", "UserLeave" });
                    //    Send(optOutMethod);
                    //}
                    //else if(internalMessage is OptOutEventsReply)
                    //{
                    AuthMethod authMethod;
                    if (MyUserId.HasValue)
                    {
                        authMethod = new AuthMethod(1, ChannelId, MyUserId.Value, Token);
                    }
                    else
                    {
                        authMethod = new AuthMethod(1, ChannelId);
                    }
                    Send(authMethod);
                }
                else if (internalMessage is UnknownMessage unknown)
                {
                    _logger.LogException(new ParseException(unknown.Raw));
                }
                //        switch (internalMessage)
                //        {
                //            case ChatMessageData chat:
                //                var context = CreateMessageContext(chat);
                //                RaiseMessageReceived(context);
                //                break;
                //            case UserUpdateEvent userUpdate:
                //                break;
                //            case DeleteMessageEvent deleteMessage:
                //                break;
                //            default:
                //                break;
                //        }
                //var mixerMessage = Tools.Convert(internalMessage);
                //RaiseMessageReceived(mixerMessage);
                RaiseMessageReceived(internalMessage);
            }
            catch (ParseException ex)
            {
                _logger.LogException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
    }
    class DummyImpl : IDummy
    {
        private readonly IDataServer _server;
        private readonly string _input;
        private readonly CookieContainer _cc;
        private readonly IBrowserProfile _browserProfile;
        private readonly MessageProvider2 _p1;


        public Task<bool> CanConnectAsync()
        {
            return Task.FromResult(true);
        }

        public async Task<IEnumerable<IProvider>> GenerateGroupAsync()
        {
            var channelName = Tools.ExtractUserId(_input);
            var channelInfo = await Api.GetChannelInfo(_server, channelName, _cc);
            var userInfo = await Api.GetCurrentUserInfo(_server, _cc);
            long? myUserId;
            string token;
            if (userInfo is CurrentUser loggedin)
            {
                var chatInfo = await Api.GetChatInfo(_server, channelInfo.Id, _cc);
                myUserId = loggedin.Id;
                token = chatInfo.Authkey;
            }
            else
            {
                myUserId = null;
                token = null;
            }
            _p1.ChannelId = channelInfo.Id;
            _p1.Token = token;
            _p1.MyUserId = myUserId;
            return new List<IProvider> { _p1 };
        }
        public DummyImpl(IDataServer server, string input, ILogger logger, CookieContainer cc, IMixerSiteOptions siteOptions, MessageProvider2 p1)
        {
            _server = server;
            _input = input;
            _cc = cc;
            _p1 = p1;
        }
    }
    class MixerCommentProvider2 : CommentProviderBase
    {
        private readonly ILogger _logger;
        private readonly ICommentOptions _options;
        private readonly IMixerSiteOptions _siteOptions;
        private readonly IUserStoreManager _userStoreManager;
        private readonly IDataServer _server;
        FirstCommentDetector _first = new FirstCommentDetector();

        private void P2_MetadataUpdated(object sender, ILiveInfo e)
        {

        }

        private void P1_MetadataUpdated(object sender, IMetadata e)
        {

        }
        private void P1_MessageReceived(object sender, IInternalMessage e)
        {
            var internalMessage = e;
            switch (internalMessage)
            {
                case ChatMessageData chat:
                    var context = CreateMessageContext(chat);
                    RaiseMessageReceived(context);
                    break;
                case UserUpdateEvent userUpdate:
                    break;
                case DeleteMessageEvent deleteMessage:
                    break;
                default:
                    break;
            }
        }
        protected virtual DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
        private MixerMessageContext CreateMessageContext(ChatMessageData chatData)
        {
            var userId = chatData.UserId.ToString();
            var isFirst = _first.IsFirstComment(userId);
            var user = GetUser(userId);
            //var comment = new MirrativComment(message, raw);
            var comment = new MixerComment(chatData, GetCurrentDateTime());
            var metadata = new CommentMessageMetadata(comment, _options, _siteOptions, user, this, isFirst)
            {
                IsInitialComment = false,
                SiteContextGuid = SiteContextGuid,
            };
            var methods = new MixerMessageMethods();
            if (_siteOptions.NeedAutoSubNickname)
            {
                var messageText = comment.CommentItems.ToText();
                var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
                if (!string.IsNullOrEmpty(nick))
                {
                    user.Nickname = nick;
                }
            }
            return new MixerMessageContext(comment, metadata, methods);
        }

        NewAutoReconnector _autoReconnector;
        MessageProvider2 _p1;
        bool _isInitialized;
        public async Task InitAsync()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            var p1 = new MessageProvider2(new Websocket("wss://chat.mixer.com/?version=1.0"), _logger);
            p1.MessageReceived += P1_MessageReceived;
            p1.MetadataUpdated += P1_MetadataUpdated;
            _p1 = p1;
        }
        public async Task ConnectInternalAsync(string input, IBrowserProfile browserProfile)
        {
            var channelName = Tools.ExtractUserId(input);
            if (string.IsNullOrEmpty(channelName))
            {
                SendSystemInfo("入力値からチャンネル名を取得できませんでした", InfoType.Error);
                return;
            }
            var cc = GetCookieContainer(browserProfile, "mixer.com");
            await InitAsync();
            
            //var p2 = new MetadataProvider2(_server, _siteOptions);
            //p2.MetadataUpdated += P2_MetadataUpdated;
            //p2.Master = p1;
            try
            {
                var dummy = new DummyImpl(_server, input, _logger, cc, _siteOptions, _p1);
                var connectionManager = new ConnectionManager(_logger);
                _autoReconnector = new NewAutoReconnector(connectionManager, dummy, new MessageUntara(), _logger);
                await _autoReconnector.AutoReconnectAsync();
            }
            finally
            {
                //p1.MessageReceived -= P1_MessageReceived;
                //p1.MetadataUpdated -= P1_MetadataUpdated;
                //p2.MetadataUpdated -= P2_MetadataUpdated;
            }
        }
        public override async Task ConnectAsync(string input, IBrowserProfile browserProfile)
        {
            BeforeConnect();
            try
            {
                await ConnectInternalAsync(input, browserProfile);
            }
            catch (Exception ex)
            {
                SendSystemInfo(ex.Message, InfoType.Error);
                _logger.LogException(ex, "", $"input={input}");
            }
            finally
            {
                AfterDisconnected();
            }
        }

        public override void Disconnect()
        {
            _autoReconnector?.Disconnect();
        }

        public override async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
        {
            var userInfo = new CurrentUserInfo
            {

            };
            return await Task.FromResult(userInfo);
        }

        public override IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.Mixer, userId);
        }

        public override Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }

        public override void SetMessage(string raw)
        {
            throw new NotImplementedException();
        }

        public MixerCommentProvider2(IDataServer server, ILogger logger, ICommentOptions options, IMixerSiteOptions siteOptions, IUserStoreManager userStoreManager)
            : base(logger, options)
        {
            _logger = logger;
            _options = options;
            _siteOptions = siteOptions;
            _userStoreManager = userStoreManager;
            _server = server;
        }
    }
}
