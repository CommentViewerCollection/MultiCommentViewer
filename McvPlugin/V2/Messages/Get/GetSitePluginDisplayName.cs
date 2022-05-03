namespace Mcv.PluginV2.Messages;

public record GetSitePluginDisplayName : IGetMessageToPluginV2;
public record ReplySitePluginDisplayName(string DisplayName) : IReplyMessageToPluginV2
{
    public string Raw => "";
}
