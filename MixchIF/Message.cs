using SitePlugin;
using System;
using System.Collections.Generic;

namespace MixchSitePlugin
{
    public enum MixchMessageType
    {
        Unknown,
        Comment,
        //Item,
        Stamp,
        Yell,
        Connected,
        Disconnected,
    }


    public interface IMixchMessage : ISiteMessage
    {
        MixchMessageType MixchMessageType { get; }
    }
    public interface IMixchConnected : IMixchMessage
    {
        string Text { get; }
    }
    public interface IMixchDisconnected : IMixchMessage
    {
        string Text { get; }
    }
    public interface IMixchComment : IMixchMessage
    {
        IEnumerable<IMessagePart> NameItems { get; }
        IEnumerable<IMessagePart> MessageItems { get; }
        string Id { get; }
        DateTime PostTime { get; }
        string UserId { get; }
    }
    public interface IMixchStamp : IMixchMessage
    {
        IMessageImage Stamp { get; }
        string Message { get; }
        IEnumerable<IMessagePart> NameItems { get; set; }
        IMessageImage UserIcon { get; }
        DateTime PostTime { get; }
        string Id { get; }
    }
    public interface IMixchYell : IMixchMessage
    {
        string YellPoints { get; }
        string Message { get; }
        IEnumerable<IMessagePart> NameItems { get; }
        IMessageImage UserIcon { get; }
        DateTime PostTime { get; }
        string Id { get; }
    }
    //public interface IMixchItem : IMixchMessage
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
