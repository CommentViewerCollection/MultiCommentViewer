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
        CoinBox = 53,
        EnterNewbie = 60,
        EnterLevel = 61,
        Follow = 62,
        EnterFanclub = 63,
    }

    public interface IMixchMessage : ISiteMessage
    {
        MixchMessageType MixchMessageType { get; set; }
        IEnumerable<IMessagePart> NameItems { get; }
        IEnumerable<IMessagePart> MessageItems { get; }
        string Id { get; }
        DateTime PostTime { get; }
        bool IsFirstComment { get; }
        string UserId { get; }
    }
}
