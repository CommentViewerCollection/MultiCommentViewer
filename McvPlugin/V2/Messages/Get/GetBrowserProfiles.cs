namespace Mcv.PluginV2.Messages;

public record GetBrowserProfiles : IGetMessageToPluginV2;
public record ReplyBrowserProfiles(IList<ProfileInfo> Profiles) : IReplyMessageToPluginV2
{
    public string Raw { get; } = "";
}
public record ProfileInfo(PluginId PluginId, string BrowserName, string? ProfileName, BrowserProfileId ProfileId);
