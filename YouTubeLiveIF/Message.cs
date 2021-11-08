using SitePlugin;
using System;
using System.Collections.Generic;

namespace YouTubeLiveSitePlugin
{
    public enum YouTubeLiveMessageType
    {
        Unknown,
        Comment,
        Superchat,
        Membership,
        Connected,
        Disconnected,
    }


    public interface IYouTubeLiveMessage : ISiteMessage
    {
        YouTubeLiveMessageType YouTubeLiveMessageType { get; }
    }
    public interface IYouTubeLiveConnected : IYouTubeLiveMessage
    {
        string Text { get; }
    }
    public interface IYouTubeLiveDisconnected : IYouTubeLiveMessage
    {
        string Text { get; }
    }
    public interface IYouTubeLiveComment : IYouTubeLiveMessage
    {
        IEnumerable<IMessagePart> NameItems { get; }
        IEnumerable<IMessagePart> CommentItems { get; }
        IMessageImage UserIcon { get; }
        DateTime PostedAt { get; }
        string Id { get; }
        string UserId { get; }
    }
    public interface IYouTubeLiveSuperchat : IYouTubeLiveMessage
    {
        IEnumerable<IMessagePart> NameItems { get; }
        IEnumerable<IMessagePart> CommentItems { get; }
        IMessageImage UserIcon { get; }
        DateTime PostedAt { get; }
        string Id { get; }
        string UserId { get; }
        string PurchaseAmount { get; }
    }
    public interface IYouTubeLiveMembership : IYouTubeLiveMessage
    {
        IEnumerable<IMessagePart> NameItems { get; }
        IEnumerable<IMessagePart> CommentItems { get; }
        IEnumerable<IMessagePart> HeaderPrimaryTextItems { get; }
        IEnumerable<IMessagePart> HeaderSubTextItems { get; }
        IMessageImage UserIcon { get; }
        DateTime PostedAt { get; }
        string Id { get; }
        string UserId { get; }
    }
}