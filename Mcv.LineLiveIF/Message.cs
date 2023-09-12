using Mcv.PluginV2;
using System;
using System.Collections.Generic;

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

    public interface ILineLiveMessage : ISiteMessage
    {
        LineLiveMessageType LineLiveMessageType { get; }
    }
    public interface ILineLiveConnected : ILineLiveMessage
    {
        string Text { get; }
    }
    public interface ILineLiveDisconnected : ILineLiveMessage
    {
        string Text { get; }
    }
    public interface ILineLiveComment : ILineLiveMessage
    {
        string Text { get; }
        bool IsNgMessage { get; }
        DateTime PostedAt { get; }
        string UserIconUrl { get; }
        long UserId { get; }
        string DisplayName { get; }
    }
    public interface ILineLiveItem : ILineLiveMessage
    {
        IEnumerable<IMessagePart> CommentItems { get; }
        DateTime PostedAt { get; }
        long UserId { get; }
        string UserIconUrl { get; }
        string DisplayName { get; }
    }
}