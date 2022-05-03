namespace Mcv.PluginV2.Messages;

public record SetDisconnectSite(ConnectionId ConnId) : ISetMessageToPluginV2;
public record NotifySiteDisconnected(ConnectionId ConnId) : INotifyMessageV2
{
    public string Raw { get; } = "";
}
public record SetLoading : ISetMessageToPluginV2;
public record SetLoaded : ISetMessageToPluginV2;
public record SetClosing : ISetMessageToPluginV2;
