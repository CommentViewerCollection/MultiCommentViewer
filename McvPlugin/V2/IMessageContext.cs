namespace Mcv.PluginV2;

public interface IMessageContext
{
    ISiteMessage Message { get; }
    string? UserId { get; }
    string? NewNickname { get; }
}
