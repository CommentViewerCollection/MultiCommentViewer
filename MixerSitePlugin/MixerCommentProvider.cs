using Common;
using ryu_s.BrowserCookie;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using SitePluginCommon;
using SitePluginCommon.AutoReconnector;
using System.Net;
using SitePluginCommon.AutoReconnection;

namespace MixerSitePlugin
{
    //class MixerConnector : IConnector
    //{
    //    private readonly IDataServer _server;
    //    private readonly string _channelId;
    //    private readonly CookieContainer _cc;
    //    private readonly IMessageProvider _messageProvider;
    //    private readonly ILogger _logger;
    //    private readonly long? _myUserId;
    //    private readonly string _token;
    //    DisconnectReason _disconnectReason = DisconnectReason.Unknown;
    //    public async Task<DisconnectReason> ConnectAsync()
    //    {
    //        _disconnectReason = DisconnectReason.Unknown;
    //        var channelInfo = await Api.GetChannelInfo(_server, _channelId, _cc);
    //        var userInfo = await Api.GetCurrentUserInfo(_server, _cc);
    //        long? myUserId;
    //        string token;
    //        if (userInfo is CurrentUser loggedin)
    //        {
    //            var chatInfo = await Api.GetChatInfo(_server, channelInfo.Id, _cc);
    //            myUserId = loggedin.Id;
    //            token = chatInfo.Authkey;
    //        }
    //        else
    //        {
    //            myUserId = null;
    //            token = null;
    //        }
    //        try
    //        {
    //            await _messageProvider.ReceiveAsync(channelInfo.Id, myUserId, token);
    //        }
    //        catch (System.IO.IOException) { }
    //        return _disconnectReason;
    //    }
    //    public void Disconnect()
    //    {
    //        _messageProvider.Disconnect();
    //        _disconnectReason = DisconnectReason.User;
    //    }

    //    public Task<bool> IsLivingAsync()
    //    {
    //        //Mixerは配信中でなくてもチャットが可能だから常にtrueで良い
    //        return Task.FromResult(true);
    //    }
    //    public MixerConnector(IDataServer server, string channelId, CookieContainer cc, IMessageProvider messageProvider, ILogger logger, long? myUserId, string token)
    //    {
    //        _server = server;
    //        _channelId = channelId;
    //        _cc = cc;
    //        _messageProvider = messageProvider;
    //        _logger = logger;
    //        _myUserId = myUserId;
    //        _token = token;
    //    }
    //}
    //internal class MixerCommentProvider : CommentProviderBase
    //{
    //    protected override void BeforeConnect()
    //    {
    //        base.BeforeConnect();
    //    }
    //    protected override void AfterDisconnected()
    //    {
    //        base.AfterDisconnected();
    //        _autoReconnector = null;
    //    }
    //    private async Task ConnectInternal2Async(string input, IBrowserProfile browserProfile)
    //    {
    //        var channelName = Tools.ExtractUserId(input);
    //        if (string.IsNullOrEmpty(channelName))
    //        {
    //            SendSystemInfo("入力値からチャンネル名を取得できませんでした", InfoType.Error);
    //            return;
    //        }
    //        var cc = GetCookieContainer(browserProfile, "mixer.com");
    //        var messageProvider = new MessageProvider2(new Websocket("wss://chat.mixer.com/?version=1.0"), _logger);
    //        messageProvider.MessageReceived += MessageProvider_MessageReceived;
    //        try
    //        {
    //            var dummy = new DummyImpl(_server, input, _logger, cc, _siteOptions, messageProvider);//, p2);
    //            var connectionManager = new ConnectionManager(_logger);
    //            _autoReconnector = new NewAutoReconnector(connectionManager, dummy);
    //            await _autoReconnector.AutoReconnectAsync();
    //        }
    //        finally
    //        {
    //            messageProvider.MessageReceived -= MessageProvider_MessageReceived;
    //        }
    //    }

    //    private void MessageProvider_MessageReceived(object sender, IMixerMessage e)
    //    {

    //    }

    //    private void Me_SystemInfoReiceved(object sender, SystemInfoEventArgs e)
    //    {
    //        SendSystemInfo(e.Message, e.Type);
    //    }

    //    NewAutoReconnector _autoReconnector;
    //    public override async Task ConnectAsync(string input, IBrowserProfile browserProfile)
    //    {
    //        BeforeConnect();
    //        try
    //        {
    //            await ConnectInternal2Async(input, browserProfile);
    //        }
    //        catch (Exception ex)
    //        {
    //            SendSystemInfo(ex.Message, InfoType.Error);
    //            _logger.LogException(ex, "", $"input={input}");
    //        }
    //        finally
    //        {
    //            AfterDisconnected();
    //        }
    //    }
    //    FirstCommentDetector _first = new FirstCommentDetector();
    //    private void MessageProvider_Received(object sender, IInternalMessage e)
    //    {
    //        var internalMessage = e;
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
    //    }
    //    private MixerMessageContext CreateMessageContext(ChatMessageData chatData)
    //    {
    //        var userId = chatData.UserId.ToString();
    //        var isFirst = _first.IsFirstComment(userId);
    //        var user = GetUser(userId);
    //        //var comment = new MirrativComment(message, raw);
    //        var comment = new MixerComment(chatData, GetCurrentDateTime());
    //        var metadata = new CommentMessageMetadata(comment, _options, _siteOptions, user, this, isFirst)
    //        {
    //            IsInitialComment = false,
    //            SiteContextGuid = SiteContextGuid,
    //        };
    //        var methods = new MixerMessageMethods();
    //        if (_siteOptions.NeedAutoSubNickname)
    //        {
    //            var messageText = comment.CommentItems.ToText();
    //            var nick = SitePluginCommon.Utils.ExtractNickname(messageText);
    //            if (!string.IsNullOrEmpty(nick))
    //            {
    //                user.Nickname = nick;
    //            }
    //        }
    //        return new MixerMessageContext(comment, metadata, methods);
    //    }
    //    protected virtual DateTime GetCurrentDateTime()
    //    {
    //        return DateTime.Now;
    //    }
    //    public override void Disconnect()
    //    {
    //        _autoReconnector?.Disconnect();
    //    }

    //    public IEnumerable<ICommentViewModel> GetUserComments(IUser user)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override async Task PostCommentAsync(string text)
    //    {
    //        await Task.FromResult<object>(null);
    //    }

    //    public override IUser GetUser(string userId)
    //    {
    //        return _userStoreManager.GetUser(SiteType.Mixer, userId);
    //    }

    //    public override async Task<ICurrentUserInfo> GetCurrentUserInfo(IBrowserProfile browserProfile)
    //    {
    //        var userInfo = new CurrentUserInfo
    //        {

    //        };
    //        return await Task.FromResult(userInfo);
    //    }

    //    private readonly IDataServer _server;
    //    private readonly ILogger _logger;
    //    private readonly ICommentOptions _options;
    //    private readonly MixerSiteOptions _siteOptions;
    //    private readonly IUserStoreManager _userStoreManager;
    //    public MixerCommentProvider(IDataServer server, ILogger logger, ICommentOptions options, MixerSiteOptions siteOptions, IUserStoreManager userStoreManager)
    //        : base(logger, options)
    //    {
    //        _server = server;
    //        _logger = logger;
    //        _options = options;
    //        _siteOptions = siteOptions;
    //        _userStoreManager = userStoreManager;
    //    }
    //}
    class CurrentUserInfo : ICurrentUserInfo
    {
        public string Username { get; set; }
        public string UserId { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
