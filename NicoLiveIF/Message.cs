using SitePlugin;
using System;
using System.Collections.Generic;

namespace NicoSitePlugin
{
    public enum NicoMessageType
    {
        Unknown,
        Comment,
        Connected,
        Disconnected,
        Ad,
        Item,
        Spi,
        Emotion,
        Kick,
        Info,
        Ignored,
    }

    public interface INicoMessage : ISiteMessage
    {
        NicoMessageType NicoMessageType { get; }
    }
    public interface INicoConnected : INicoMessage
    {
        string Text { get; }
    }
    public interface INicoDisconnected : INicoMessage
    {
        string Text { get; }
    }
    public interface INicoComment : INicoMessage
    {
        string UserName { get; }
        string Text { get; }
        string Id { get; }
        bool Is184 { get; }
        [Obsolete]
        string RoomName { get; }
        int? ChatNo { get; }
        DateTime PostedAt { get; }
        string UserId { get; }
        string ThumbnailUrl { get; set; }
        void SetUserName(string userName);
    }
    public interface INicoAd : INicoMessage
    {
        string Text { get; }
        DateTime PostedAt { get; }
        string UserId { get; }
        string RoomName { get; }
    }
    public interface INicoGift : INicoMessage
    {
        string UserId { get; }
        DateTime PostedAt { get; }
        string Text { get; }
        [Obsolete]
        string RoomName { get; }
        string ItemName { get; }
        int ItemCount { get; }
    }
    public interface INicoSpi : INicoMessage
    {
        string Text { get; }
        DateTime PostedAt { get; }
        string UserId { get; }
    }
    public interface INicoEmotion : INicoMessage
    {
        string Content { get; }
        DateTime PostedAt { get; }
        int? ChatNo { get; }
        int Vpos { get; }
        string UserId { get; }
        int Premium { get; }
        int Anonymity { get; }
    }
    public interface INicoKickCommand : INicoMessage
    {

    }
    public interface INicoInfo : INicoMessage
    {
        string Text { get; }
        DateTime PostedAt { get; }
        string UserId { get; }
        string RoomName { get; }
        int No { get; }
    }
}