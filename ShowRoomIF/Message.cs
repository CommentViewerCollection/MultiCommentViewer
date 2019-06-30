using SitePlugin;

namespace ShowRoomSitePlugin
{
    public enum ShowRoomMessageType
    {
        Unknown,
        Comment,
        Connected,
        Disconnected,
        Join,
        Leave,
    }

    public interface IShowRoomMessage : IMessage
    {
        ShowRoomMessageType ShowRoomMessageType { get; }
    }
    public interface IShowRoomConnected : IShowRoomMessage
    {
    }
    public interface IShowRoomDisconnected : IShowRoomMessage
    {
    }
    public interface IShowRoomComment : IShowRoomMessage, IMessageComment
    {
    }
    public interface IShowRoomJoin : IShowRoomMessage
    {
        string UserId { get; }
    }
    public interface IShowRoomLeave : IShowRoomMessage
    {
        string UserId { get; }
    }
    public interface IShowRoomItem : IShowRoomMessage
    {
        string PostTime { get; }
    }
}