namespace Mcv.PluginV2.Messages;

public record GetAppName : IGetMessageToCoreV2;
public record ReplyAppName(string AppName) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"ans\",\"ans\":\"appname\",\"appname\":\"{AppName}\"}}";
}
