using System.Net;

namespace Mcv.PluginV2.Messages;

public record SetConnectSite(ConnectionId ConnId, string Input, List<Cookie> Cookies) : ISetMessageToPluginV2;
public record NotifySiteConnected(ConnectionId ConnId) : INotifyMessageV2
{
    public string Raw { get; } = "";
}
