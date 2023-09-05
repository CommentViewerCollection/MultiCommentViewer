using Mcv.PluginV2;
using System;
using System.Collections.Generic;

namespace MildomSitePlugin
{
    public enum MildomMessageType
    {
        Unknown,
        Comment,
        JoinRoom,
        Connected,
        Disconnected,
        Gift,
    }

    public interface IMildomMessage : ISiteMessage
    {
        MildomMessageType MildomMessageType { get; }
    }
    public interface IMildomConnected : IMildomMessage
    {
        string Text { get; }
    }
    public interface IMildomDisconnected : IMildomMessage
    {
        string Text { get; }
    }
    public interface IMildomComment : IMildomMessage
    {
        string UserName { get; }
        IEnumerable<IMessagePart> CommentItems { get; }
        DateTime PostedAt { get; }
        string UserId { get; }
    }
    public interface IMildomJoinRoom : IMildomMessage
    {
        IEnumerable<IMessagePart> NameItems { get; }
        IEnumerable<IMessagePart> CommentItems { get; }
        //string Id { get; }
        string UserId { get; }
        DateTime PostedAt { get; }
        IMessageImage UserIcon { get; }
    }
    public interface IMildomGift : IMildomMessage
    {
        string GiftName { get; }
        string GiftId { get; }
        string GiftUrl { get; }
        int Coins { get; }
        DateTime PostedAt { get; }
        int Count { get; }
        string UserName { get; }
    }
}