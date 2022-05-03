using Mcv.PluginV2;
using System;

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

    public interface IShowRoomMessage : ISiteMessage
    {
        ShowRoomMessageType ShowRoomMessageType { get; }
    }
    public interface IShowRoomConnected : IShowRoomMessage
    {
        string Text { get; }
    }
    public interface IShowRoomDisconnected : IShowRoomMessage
    {
        string Text { get; }
    }
    public interface IShowRoomComment : IShowRoomMessage
    {
        string UserName { get; }
        string Text { get; }
        DateTime PostedAt { get; }
        string UserId { get; }
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