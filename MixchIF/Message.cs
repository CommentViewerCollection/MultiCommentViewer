using SitePlugin;
using System;
using System.Collections.Generic;

namespace MixchSitePlugin
{
    public enum MixchMessageType : int
    {
        Comment = 0,
        Status = 10,
        SuperComment = 42,
        Stamp = 45,
        PoiPoi = 46,
        Item = 48,
        Share = 50,
        EnterNewbie = 60,
        EnterLevel = 61,
        Follow = 62,
        EnterFanclub = 63,
    }

    public interface IMixchMessage : ISiteMessage
    {
        MixchMessageType MixchMessageType { get; set; }
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
}
