namespace Mcv.PluginV2.Messages;

public record GetAppSolutionConfiguration : IGetMessageToCoreV2;
public record ReplyAppSolutionConfiguration(string AppSolutionConfiguration) : IReplyMessageToPluginV2
{
    public string Raw => $"{{\"type\":\"ans\",\"ans\":\"appSolutionConfiguration\",\"appSolutionConfiguration\":\"{AppSolutionConfiguration}\"}}";
}
