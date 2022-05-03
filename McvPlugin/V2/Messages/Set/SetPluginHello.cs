namespace Mcv.PluginV2.Messages;

public record SetPluginHello(PluginId PluginId, string PluginName, List<string> PluginRole) : ISetMessageToCoreV2;
public record NotifyPluginAdded(PluginId PluginId, string PluginName, List<string> PluginRole) : INotifyMessageV2
{
    public string Raw { get; } = "";
}
