using SitePlugin;

namespace LineLiveSitePlugin
{
    public enum LineLiveMessageType
    {
        Unknown,
        Comment,
        Connected,
        Disconnected,
        Item,
    }

    public interface ILineLiveMessage : IMessage
    {
        LineLiveMessageType LineLiveMessageType { get; }
    }
    public interface ILineLiveConnected : ILineLiveMessage
    {
    }
    public interface ILineLiveDisconnected : ILineLiveMessage
    {
    }
    public interface ILineLiveComment : ILineLiveMessage, IMessageComment
    {
    }
    public interface ILineLiveItem : ILineLiveMessage
    {
        string PostTime { get; }
    }
}