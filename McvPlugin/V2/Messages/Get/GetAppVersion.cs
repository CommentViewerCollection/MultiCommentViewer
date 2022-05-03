namespace Mcv.PluginV2.Messages;

public record GetAppVersion : IGetMessageToCoreV2;
public record ReplyAppVersion(string AppVersion) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"ans\",\"ans\":\"appversion\",\"appversion\":\"{AppVersion}\"}}";
}
