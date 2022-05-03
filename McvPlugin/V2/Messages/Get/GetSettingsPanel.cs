namespace Mcv.PluginV2.Messages;

public record GetSettingsPanel : IGetMessageToPluginV2;
public record AnswerSettingsPanel(IOptionsTabPage Panel) : IReplyMessageToPluginV2
{
    public string Raw => "{}";
}
