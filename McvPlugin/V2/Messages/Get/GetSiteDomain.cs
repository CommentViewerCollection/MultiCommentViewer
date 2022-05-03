namespace Mcv.PluginV2.Messages;

public record GetSiteDomain(ConnectionId ConnectionId) : IGetMessageToPluginV2;
public record ReplySiteDomain(string Domain) : IReplyMessageToPluginV2
{
    public string Raw { get; } = "";
}
