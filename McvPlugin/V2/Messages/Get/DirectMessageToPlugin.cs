namespace Mcv.PluginV2.Messages;

/// <summary>
/// プラグインから別のプラグインに直接setメッセージを送る
/// </summary>
/// <param name="Target">対象のプラグイン</param>
/// <param name="Message">メッセージ</param>
public record SetDirectMessage(PluginId Target, ISetMessageToPluginV2 Message) : ISetMessageToCoreV2;
/// <summary>
/// プラグインから別のプラグインに直接Getメッセージを送る
/// </summary>
/// <param name="Target">対象のプラグイン</param>
/// <param name="Message">メッセージ</param>
public record GetDirectMessage(PluginId Target, IGetMessageToPluginV2 Message) : IGetMessageToCoreV2;
public record ReplyDirectMessage(IReplyMessageToPluginV2 Message) : IReplyMessageToPluginV2
{
    public string Raw { get; } = "";
}
