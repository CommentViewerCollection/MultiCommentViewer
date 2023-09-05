using Mcv.PluginV2;
using System;

namespace BigoSitePlugin
{
    public enum BigoMessageType
    {
        Unknown,
        Comment,
        Gift,
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
    public interface IBigoGift : IBigoMessage
    {
        string Username { get; }
        string GiftName { get; }
        int GiftCount { get; }
        string GiftImgUrl { get; }
    }
}