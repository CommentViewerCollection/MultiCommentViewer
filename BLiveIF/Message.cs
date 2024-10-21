using SitePlugin;
using System;
using System.Collections.Generic;

namespace BLiveSitePlugin
{
    public enum BLiveMessageType
    {
        Unknown,
        Comment,
        //Item,
        Stamp,
        Yell,
        Connected,
        Disconnected,
    }


    public interface IBLiveMessage : ISiteMessage
    {
        BLiveMessageType BLiveMessageType { get; }
    }
    public interface IBLiveConnected : IBLiveMessage
    {
        string Text { get; }
    }
    public interface IBLiveDisconnected : IBLiveMessage
    {
        string Text { get; }
    }
    public interface IBLiveComment : IBLiveMessage
    {
        IEnumerable<IMessagePart> NameItems { get; }
        IEnumerable<IMessagePart> MessageItems { get; }
        string Id { get; }
        DateTime PostTime { get; }
        string UserId { get; }
    }
    public interface IBLiveStamp : IBLiveMessage
    {
        IMessageImage Stamp { get; }
        string Message { get; }
        IEnumerable<IMessagePart> NameItems { get; set; }
        IMessageImage UserIcon { get; }
        DateTime PostTime { get; }
        string Id { get; }
    }
    public interface IBLiveYell : IBLiveMessage
    {
        string YellPoints { get; }
        string Message { get; }
        IEnumerable<IMessagePart> NameItems { get; }
        IMessageImage UserIcon { get; }
        DateTime PostTime { get; }
        string Id { get; }
    }
    //public interface IBLiveItem : IBLiveMessage
    //{
    //    string ItemName { get; }
    //    int ItemCount { get; }
    //    //string Comment { get; }
    //    long Id { get; }
    //    //string UserName { get; }
    //    string UserPath { get; }
    //    long UserId { get; }
    //    string AccountName { get; }
    //    long PostedAt { get; }
    //    string UserIconUrl { get; }
    //}
}