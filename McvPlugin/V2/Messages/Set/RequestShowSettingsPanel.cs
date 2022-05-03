namespace Mcv.PluginV2.Messages;

public record RequestShowSettingsPanel(PluginId PluginId) : ISetMessageToCoreV2;
public class RequestShowSettingsPanelToPlugin : ISetMessageToPluginV2 { }
