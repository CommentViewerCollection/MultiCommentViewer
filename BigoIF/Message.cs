using SitePlugin;
using System;
using System.Collections.Generic;

namespace BigoSitePlugin
{
    public enum BigoMessageType
    {
        Unknown,
        Comment,
        Superchat,
        Connected,
        Disconnected,
    }


    public interface IBigoMessage : ISiteMessage
    {
        BigoMessageType BigoMessageType { get; }
    }
    public interface IBigoConnected : IBigoMessage
    {
        string Text { get; }
    }
    public interface IBigoDisconnected : IBigoMessage
    {
        string Text { get; }
    }
    public interface IBigoComment : IBigoMessage
    {
        string Name { get; }
        string Message { get; }
        DateTime PostedAt { get; }
        //string Id { get; }
        //string UserId { get; }
    }
}