namespace Mcv.PluginV2.Messages;

public record GetLegacyOptions : IGetMessageToCoreV2;
public record ReplyLegacyOptions(string RawOptions) : IReplyMessageToPluginV2
{
    public string Raw => "";
}
