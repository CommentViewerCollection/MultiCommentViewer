namespace Mcv.PluginV2.Messages;

public record GetIsValidSiteUrl(string Input) : IGetMessageToPluginV2;
public record ReplyIsValidSiteUrl(bool IsValid) : IReplyMessageToPluginV2
{
    public string Raw { get; } = "";
}
