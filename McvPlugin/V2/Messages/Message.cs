namespace Mcv.PluginV2.Messages;

public interface IMessage { }
/// <summary>
/// Coreに対して状態の変更を求める
/// </summary>
public interface ISetMessageToCoreV2 : IMessage { }
/// <summary>
/// Pluginに対して状態の変更を求める
/// </summary>
public interface ISetMessageToPluginV2 { }
/// <summary>
/// 状態の変更があった場合の通知
/// </summary>
public interface INotifyMessageV2
{
    string Raw { get; }
}
/// <summary>
/// 現在の状態を取得する要求
/// </summary>
public interface IGetMessageToCoreV2 : IMessage { }
/// <summary>
/// Getメッセージに対する返答
/// </summary>
public interface IReplyMessageToCoreV2 : IMessage
{
    string Raw { get; }
}
/// <summary>
/// 現在の状態を取得する要求
/// </summary>
public interface IGetMessageToPluginV2 : IMessage { }
/// <summary>
/// Getメッセージに対する返答
/// </summary>
public interface IReplyMessageToPluginV2 : IMessage
{
    string Raw { get; }
}

