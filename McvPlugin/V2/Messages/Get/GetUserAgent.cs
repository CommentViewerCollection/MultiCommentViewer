namespace Mcv.PluginV2.Messages;

public record GetUserAgent : IGetMessageToCoreV2;
public record ReplyUserAgent(string UserAgent) : IReplyMessageToPluginV2
{
    public string Raw => "";
}
