using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Mcv.PluginV2;
using Mcv.PluginV2.AutoReconnection;

namespace MildomSitePlugin
{
    interface IMyUserInfo
    {
        string UserId { get; }
        string Loginname { get; }
    }
    public class LoggedinUserInfo : IMyUserInfo
    {
        public string MyId { get; }
        public string UserId { get; }
        public string Loginname { get; }
        public Guid AccessToken { get; }
        public string Gid { get; }

        public LoggedinUserInfo(Low.UserInfo.RootObject low, string gid)
        {
            MyId = low.MyId.ToString();
            UserId = low.UserId.ToString();
            Loginname = low.Loginname;
            AccessToken = low.AccessToken;
            Gid = gid;
        }
    }
    public class AnonymousUserInfo : IMyUserInfo
    {
        public string GuestId { get; }
        public string GuestName { get; }
        string IMyUserInfo.UserId => GuestId;
        string IMyUserInfo.Loginname => GuestName;
        public AnonymousUserInfo(string guestId, string guestName)
        {
            GuestId = guestId;
            GuestName = guestName;
        }
    }
    class MildomCommentProvider : CommentProviderBase
    {
        FirstCommentDetector _first = new FirstCommentDetector();
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly IMildomSiteOptions _siteOptions;

        Dictionary<long, GiftItem> _giftDict;
        public override async Task ConnectAsync(string input, List<Cookie> cookies)
        {
            BeforeConnect();
            try
            {
                await ConnectInternalAsync(input, cookies);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "", $"input={input}");
            }
            finally
            {
                AfterDisconnected();
            }
        }
        NewAutoReconnector _autoReconnector;
        Dictionary<int, string> _imageDict;
        private async Task ConnectInternalAsync(string input, List<Cookie> cookies)
        {
            _isBeeingSentInitialComments = true;
            var mayBeRoomId = Tools.ExtractRoomId(input);
            if (!mayBeRoomId.HasValue)
            {
                //不正なURL
                return;
            }
            var roomId = mayBeRoomId.Value;
            var cc = GetCookieContainer(cookies);
            _imageDict = await Api.GetImageDictionary(_server, roomId, cc);
            _giftDict = await Tools.GetGiftDict(_server);

            //TODO:websocketUrlをAPI経由で取得する
            //https://im.mildom.com/?room_id=10045175&type=chat&call=get_server&cluster=aws_japan
            var websocketUrl = "wss://jp-room1.mildom.com/?roomId=" + roomId;
            var p1 = new MessageProvider(new SytemNetWebSockets(websocketUrl), _logger);
            p1.MessageReceived += P1_MessageReceived;
            p1.BinaryMessageReceived += P1_BinaryMessageReceived;
            p1.MetadataUpdated += P1_MetadataUpdated;
            //var p2 = new MetadataProvider2(_server, _siteOptions);
            //p2.MetadataUpdated += P2_MetadataUpdated;
            //p2.Master = p1;
            try
            {
                if (cookies.Count == 0)
                {
                    //何らかの理由でブラウザからCookieが取れなかった場合にCookieを自前で用意する。
                    //Cookieが全くないとコメントが取れない。恐らくguest_idが空欄になるから。
                    var _ = await _server.GetAsync($"https://cloudac-cf-jp.mildom.com/nonolive/gappserv/guest/h5init?timestamp={DateTime.UtcNow:yyyy-MM-ddTHH%3Amm%3Ass.fffZ}&__guest_id=&__location=Japan%7CTokyo&__country=Japan&__cluster=aws_japan&__pcv=v4.9.68&__platform=web&__la=ja&sfr=pc&accessToken=", new Dictionary<string, string>(), cc);
                    var ccc = Tools.ExtractCookies(cc);
                    cookies.AddRange(ccc);
                }
                var dummy = new DummyImpl(_server, input, cookies, _logger, _siteOptions, p1);//, p2);
                var connectionManager = new ConnectionManager(_logger);
                _autoReconnector = new NewAutoReconnector(connectionManager, dummy, new MessageUntara(), _logger);
                await _autoReconnector.AutoReconnectAsync();
            }
            finally
            {
                p1.MessageReceived -= P1_MessageReceived;
                p1.BinaryMessageReceived -= P1_BinaryMessageReceived;
                p1.MetadataUpdated -= P1_MetadataUpdated;
                //p2.MetadataUpdated -= P2_MetadataUpdated;
            }
        }

        private void P1_BinaryMessageReceived(object sender, byte[] e)
        {
            var raw = e;
            var a = InternalMessage.InternalMessageParser.DecryptMessage(raw);
            //var b = InternalMessage.InternalMessageParser.DecryptMessageWithBase64(raw);
            SetMessage(a);
            return;
        }

        private void P2_MetadataUpdated(object sender, ILiveInfo e)
        {

        }

        private void P1_MetadataUpdated(object sender, IMetadata e)
        {

        }
        public void SetMessage(IInternalMessage internalMessage)
        {
            if (internalMessage is EnterRoom)
            {
                _isBeeingSentInitialComments = true;
            }
            else if (_isBeeingSentInitialComments && !(internalMessage is OnChatMessage))
            {
                _isBeeingSentInitialComments = false;
            }
            var messageContext = CreateMessageContext(internalMessage);
            if (messageContext != null)
            {
                RaiseMessageReceived(messageContext);
            }
        }
        public override async void SetMessage(string rawMessage)
        {
            IInternalMessage internalMessage = null;
            try
            {
                internalMessage = MessageParser.Parse(rawMessage, _imageDict);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "", $"raw={rawMessage}");
            }
            if (internalMessage == null)
            {
                SendSystemInfo($"ParseError: {rawMessage}", InfoType.Error);
                return;
            }
            SetMessage(internalMessage);
        }
        private void P1_MessageReceived(object sender, string e)
        {
            var raw = e;
            SetMessage(raw);
        }
        /// <summary>
        /// 初期コメントが送られてきているか。
        /// 接続直後にEnterRoomが送られてきて、その次に初期コメント(OnChatMessage)が送られてくる。その後、OnChatMessage以外のメッセージが来たら初期コメントは終了。
        /// 自分名義のOnAddは確実に来るからこのロジックで問題無いと思う。
        /// </summary>
        bool _isBeeingSentInitialComments;
        private MildomMessageContext CreateMessageContext(IInternalMessage message)
        {
            if (message is OnChatMessage chat)
            {
                var userId = chat.UserId.ToString();
                var isFirst = _first.IsFirstComment(userId);
                var comment = new MildomComment(chat, chat.Raw);
                string? newNickname = null;
                if (_siteOptions.NeedAutoSubNickname)
                {
                    var messageText = chat.MessageItems.ToText();
                    var nick = Utils.ExtractNickname(messageText);
                    if (!string.IsNullOrEmpty(nick))
                    {
                        newNickname = nick;
                    }
                }
                return new MildomMessageContext(comment, userId, newNickname);
            }
            else if (message is OnGiftMessage internalGift)
            {
                var userId = internalGift.UserId.ToString();
                //var isFirst = _first.IsFirstComment(userId);

                if (!_giftDict.TryGetValue(internalGift.GiftId, out var item))
                {
                    item = new GiftItem("(未知のギフト)");
                }
                var gift = new MildomGift(internalGift, item);

                return new MildomMessageContext(gift, userId, null);
            }
            //if (message is IMildomComment comment)
            //{
            //    var userId = comment.UserId;
            //    var isFirst = _first.IsFirstComment(userId);
            //    var user = GetUser(userId);
            //    //var comment = new MildomComment(message, raw);
            //    var metadata = new CommentMessageMetadata(comment, _options, _siteOptions, user, this, isFirst)
            //    {
            //        IsInitialComment = false,
            //        SiteContextGuid = SiteContextGuid,
            //    };
            //    var methods = new MildomMessageMethods();
            //    if (_siteOptions.NeedAutoSubNickname)
            //    {
            //        var messageText = message.CommentItems.ToText();
            //        var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
            //        if (!string.IsNullOrEmpty(nick))
            //        {
            //            user.Nickname = nick;
            //        }
            //    }
            //    return new MildomMessageContext(comment, metadata, methods);
            //}
            else if (message is OnAddMessage add && _siteOptions.IsShowJoinMessage)
            {
                var userId = add.UserId.ToString();
                var join = new MildomJoinRoom(add);
                return new MildomMessageContext(join, userId, null);
            }
            ////else if (message is IMildomItem item)
            ////{
            ////    var userId = item.UserId;
            ////    var isFirst = false;
            ////    var user = GetUser(userId);
            ////    var metadata = new  MildomMessageMetadata(item, _options, _siteOptions, user, this, isFirst)
            ////    {
            ////        IsInitialComment = false,
            ////        SiteContextGuid = SiteContextGuid,
            ////    };
            ////    var methods = new MildomMessageMethods();
            ////    return new MildomMessageContext(item, metadata, methods);
            ////}
            //else if (message is IMildomConnected connected)
            //{
            //    var metadata = new ConnectedMessageMetadata(connected, _options, _siteOptions)
            //    {
            //        IsInitialComment = false,
            //        SiteContextGuid = SiteContextGuid,
            //    };
            //    var methods = new MildomMessageMethods();
            //    return new MildomMessageContext(connected, metadata, methods);
            //}
            //else if (message is IMildomDisconnected disconnected)
            //{
            //    var metadata = new DisconnectedMessageMetadata(disconnected, _options, _siteOptions)
            //    {
            //        IsInitialComment = false,
            //        SiteContextGuid = SiteContextGuid,
            //    };
            //    var methods = new MildomMessageMethods();
            //    return new MildomMessageContext(disconnected, metadata, methods);
            //}
            else
            {
                return null;
            }
        }
        public override void Disconnect()
        {
            _autoReconnector?.Disconnect();
        }


        protected virtual CookieContainer GetCookieContainer(List<Cookie> cookies)
        {
            var cc = new CookieContainer();

            try
            {
                foreach (var cookie in cookies)
                {
                    cc.Add(cookie);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            return cc;
        }
        public override async Task<ICurrentUserInfo> GetCurrentUserInfo(List<Cookie> cookies)
        {
            await Task.Yield();
            var userInfo = Tools.GetUserInfoFromCookie(cookies);
            if (userInfo != null)
            {

                return new CurrentUserInfo
                {
                    IsLoggedIn = true,
                    UserId = userInfo.UserId.ToString(),
                    Username = userInfo.Loginname,
                };
            }
            else
            {
                return new CurrentUserInfo
                {
                    IsLoggedIn = false,
                    UserId = "",
                    Username = "",
                };
            }
        }

        public override Task PostCommentAsync(string text)
        {
            throw new NotImplementedException();
        }
        public MildomCommentProvider(IDataServer server, ILogger logger, IMildomSiteOptions siteOptions)
            : base(logger)
        {
            _server = server;
            _logger = logger;
            _siteOptions = siteOptions;
        }
    }
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
