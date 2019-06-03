using Common;
using SitePlugin;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using SitePluginCommon;
using System.Collections.Generic;
using ryu_s.BrowserCookie;

namespace NicoSitePlugin
{
    abstract class CommentProviderInternalBase: INicoCommentProviderInternal
    {
        protected readonly ICommentOptions _options;
        protected readonly INicoSiteOptions _siteOptions;
        protected readonly IUserStoreManager _userStoreManager;
        protected readonly IDataSource _dataSource;
        protected readonly ILogger _logger;
        protected readonly ConcurrentDictionary<string, int> _userCommentCountDict = new ConcurrentDictionary<string, int>();
        /// <summary>
        /// 現在の放送の部屋のThreadIdで一番小さいもの。
        /// 基本的にはアリーナがこれに該当するが、自分の部屋しか取れない場合もあるためそれを考慮してこういう形にした。
        /// </summary>
        protected string _mainRoomThreadId;
        private IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.NicoLive, userId);
        }
        protected void SendSystemInfo(string message, InfoType type)
        {
            var context = InfoMessageContext.Create(new InfoMessage
            {
                CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(message) },
                NameItems = null,
                SiteType = SiteType.NicoLive,
                Type = type,
            }, _options);
            MessageReceived?.Invoke(this, context);
        }
        public async Task<NicoMessageContext> CreateMessageContextAsync(IChat chat, string roomName, bool isInitialComment)
        {
            NicoMessageContext messageContext = null;
            INicoMessageMetadata metadata;

            var userId = chat.UserId;


            INicoMessage message;
            var messageType = Tools.GetMessageType(chat, _mainRoomThreadId);
            switch (messageType)
            {
                case NicoMessageType.Comment:
                    {
                        var user = GetUser(userId);
                        var comment = await Tools.CreateNicoComment(chat, user, _siteOptions, roomName, async userid => await API.GetUserInfo(_dataSource, userid), _logger);

                        bool isFirstComment;
                        if (_userCommentCountDict.ContainsKey(userId))
                        {
                            _userCommentCountDict[userId]++;
                            isFirstComment = false;
                        }
                        else
                        {
                            _userCommentCountDict.AddOrUpdate(userId, 1, (s, n) => n);
                            isFirstComment = true;
                        }
                        message = comment;
                        metadata = new CommentMessageMetadata(comment, _options, _siteOptions, user, _cp, isFirstComment)
                        {
                            IsInitialComment = isInitialComment,
                            SiteContextGuid = SiteContextGuid,
                        };
                    }
                    break;
                case NicoMessageType.Info:
                    {
                        var info = Tools.CreateNicoInfo(chat, roomName, _siteOptions);
                        message = info;
                        metadata = new InfoMessageMetadata(info, _options, _siteOptions);
                    }
                    break;
                case NicoMessageType.Ad:
                    {
                        var ad = Tools.CreateNicoAd(chat, roomName, _siteOptions);
                        message = ad;
                        metadata = new AdMessageMetadata(ad, _options, _siteOptions);
                    }
                    break;
                case NicoMessageType.Item:
                    {
                        var item = Tools.CreateNicoItem(chat, roomName, _siteOptions);
                        message = item;
                        metadata = new ItemMessageMetadata(item, _options, _siteOptions);
                    }
                    break;
                default:
                    message = null;
                    metadata = null;
                    break;
            }
            if (message == null || metadata == null)
            {
                return null;
            }
            else
            {
                var methods = new NicoMessageMethods();
                messageContext = new NicoMessageContext(message, metadata, methods);
                return messageContext;
            }
        }

        public abstract void BeforeConnect();

        public abstract void AfterDisconnected();

        public abstract Task ConnectAsync(string input, CookieContainer cc);

        public abstract void Disconnect();

        public abstract bool IsValidInput(string input);
        protected void RaiseMessageReceived(NicoMessageContext messageContext)
        {
            if (messageContext != null)
            {
                MessageReceived?.Invoke(this, messageContext);
            }
        }
        protected void RaiseConnected(ConnectedEventArgs args)
        {
            Connected?.Invoke(this, args);
        }
        protected void RaiseMetadataUpdated(IMetadata metadata)
        {
            MetadataUpdated?.Invoke(this, metadata);
        }

        public abstract Task PostCommentAsync(string comment, string mail);

        public CommentProviderInternalBase(ICommentOptions options, INicoSiteOptions siteOptions, IUserStoreManager userStoreManager, IDataSource dataSource, ILogger logger)
        {
            _options = options;
            _siteOptions = siteOptions;
            _userStoreManager = userStoreManager;
            _dataSource = dataSource;
            _logger = logger;
        }
        public Guid SiteContextGuid { get; set; }
        public ICommentProvider _cp;

        public event EventHandler<IMessageContext> MessageReceived;
        public event EventHandler<IMetadata> MetadataUpdated;
        public event EventHandler<ConnectedEventArgs> Connected;
    }
}
