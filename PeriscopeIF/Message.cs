using SitePlugin;

namespace PeriscopeSitePlugin
{
    public enum PeriscopeMessageType
    {
        Unknown,
        Comment,
        Connected,
        Disconnected,
        Join,
        Leave,
    }

    public interface IPeriscopeMessage : IMessage
    {
        PeriscopeMessageType PeriscopeMessageType { get; }
    }
    public interface IPeriscopeConnected : IPeriscopeMessage
    {
    }
    public interface IPeriscopeDisconnected : IPeriscopeMessage
    {
    }
    public interface IPeriscopeComment : IPeriscopeMessage, IMessageComment
    {
    }
    public interface IPeriscopeJoin : IPeriscopeMessage
    {
        string UserId { get; }
    }
    public interface IPeriscopeLeave : IPeriscopeMessage
    {
        string UserId { get; }
    }
    public interface IPeriscopeItem : IPeriscopeMessage
    {
        string PostTime { get; }
    }
}