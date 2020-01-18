using SitePlugin;
using System;
using System.Collections.Generic;

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

    public interface IPeriscopeMessage : ISiteMessage
    {
        PeriscopeMessageType PeriscopeMessageType { get; }
    }
    public interface IPeriscopeConnected : IPeriscopeMessage
    {
        string Text { get; }
    }
    public interface IPeriscopeDisconnected : IPeriscopeMessage
    {
        string Text { get; }
    }
    public interface IPeriscopeComment : IPeriscopeMessage
    {
        string Id { get; }
        string UserId { get; }
        DateTime? PostedAt { get; }
        string Text { get; }
        string DisplayName { get; }
    }
    public interface IPeriscopeJoin : IPeriscopeMessage
    {
        string DisplayName { get; }
        string Text { get; }
        string UserId { get; }
    }
    public interface IPeriscopeLeave : IPeriscopeMessage
    {
        string DisplayName { get; }
        string Text { get; }
        string UserId { get; }
    }
    public interface IPeriscopeItem : IPeriscopeMessage
    {
        string PostTime { get; }
    }
}