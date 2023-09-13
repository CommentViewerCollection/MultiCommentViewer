namespace Mcv.PluginV2;

public interface IMessageContext
{
    ISiteMessage Message { get; }
    string? UserId { get; }
    IEnumerable<IMessagePart>? UsernameItems { get; }
    string? NewNickname { get; }
    bool IsInitialComment { get; }
}
