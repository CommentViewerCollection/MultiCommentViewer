namespace Mcv.PluginV2.Messages;

public record GetConnectionStatus(ConnectionId ConnId) : IGetMessageToCoreV2;
public record ReplyConnectionStatus(IConnectionStatus ConnSt) : IReplyMessageToPluginV2
{
    public string Raw => "";
}
