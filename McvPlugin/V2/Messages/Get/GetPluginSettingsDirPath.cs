namespace Mcv.PluginV2.Messages;

public record GetPluginSettingsDirPath(string FilePath) : IGetMessageToCoreV2;
public record ReplyPluginSettingsDirPath(string PluginSettingsDirPath) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"ans\",\"ans\":\"pluginsettingsdirpath\",\"pluginsettingsdirpath\":\"{PluginSettingsDirPath}\"}}";
}
