namespace Mcv.PluginV2.Messages;

public record RequestLoadPluginOptions(string PluginName) : IGetMessageToCoreV2;
public record ReplyPluginOptions(string? RawOptions) : IReplyMessageToPluginV2
{
    public string Raw => "";
}
