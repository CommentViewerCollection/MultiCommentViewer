using System.Collections.Generic;

namespace Mcv.PluginV2.Messages;

public class SetMessage : ISetMessageToCoreV2
{
    public SetMessage(ConnectionId connId, PluginId siteId, ISiteMessage message, string? userId, string? newNickname, bool isInitialComment)
    {
        ConnId = connId;
        SiteId = siteId;
        Message = message;
        UserId = userId;
        NewNickname = newNickname;
        IsInitialComment = isInitialComment;
    }

    public ConnectionId ConnId { get; }
    public PluginId SiteId { get; }
    public ISiteMessage Message { get; }
    public string? UserId { get; }
    public string? NewNickname { get; }
    public bool IsInitialComment { get; }
}
public class NotifyMessageReceived : INotifyMessageV2
{
    public NotifyMessageReceived(ConnectionId connId, PluginId siteId, ISiteMessage message, string? userId, IEnumerable<IMessagePart>? username, string? nickname, bool isNgUser, bool isInitialComment)
    {
        ConnectionId = connId;
        SiteId = siteId;
        Message = message;
        UserId = userId;
        Username = username;
        Nickname = nickname;
        IsNgUser = isNgUser;
        IsInitialComment = isInitialComment;
    }

    public ConnectionId ConnectionId { get; }
    public PluginId SiteId { get; }
    public ISiteMessage Message { get; }
    public string? UserId { get; }
    public IEnumerable<IMessagePart>? Username { get; }
    public string? Nickname { get; }
    public bool IsNgUser { get; }
    public bool IsInitialComment { get; }

    public string Raw
    {
        get
        {
            return "";
        }
    }
}
