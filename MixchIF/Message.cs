using SitePlugin;
using System;
using System.Collections.Generic;

namespace OpenrecSitePlugin
{
    public enum OpenrecMessageType
    {
        Unknown,
        Comment,
        //Item,
        Stamp,
        Yell,
        Connected,
        Disconnected,
    }


    public interface IOpenrecMessage : ISiteMessage
    {
        OpenrecMessageType OpenrecMessageType { get; }
    }
    public interface IOpenrecConnected : IOpenrecMessage
    {
        string Text { get; }
    }
    public interface IOpenrecDisconnected : IOpenrecMessage
    {
        string Text { get; }
    }
    public interface IOpenrecComment : IOpenrecMessage
    {
        IEnumerable<IMessagePart> NameItems { get; }
        IEnumerable<IMessagePart> MessageItems { get; }
        string Id { get; }
        DateTime PostTime { get; }
        string UserId { get; }
    }
    public interface IOpenrecStamp : IOpenrecMessage
    {
        IMessageImage Stamp { get; }
        string Message { get; }
        IEnumerable<IMessagePart> NameItems { get; set; }
        IMessageImage UserIcon { get; }
        DateTime PostTime { get; }
        string Id { get; }
    }
    public interface IOpenrecYell : IOpenrecMessage
    {
        string YellPoints { get; }
        string Message { get; }
        IEnumerable<IMessagePart> NameItems { get; }
        IMessageImage UserIcon { get; }
        DateTime PostTime { get; }
        string Id { get; }
    }
    //public interface IOpenrecItem : IOpenrecMessage
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