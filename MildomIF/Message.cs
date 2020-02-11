using SitePlugin;
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
        Item,
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
    public interface IMildomItem : IMildomMessage
    {
        IEnumerable<IMessagePart> NameItems { get; }
        IEnumerable<IMessagePart> CommentItems { get; }
        string Id { get; }
        string UserId { get; }
        string PostTime { get; }
        //IMessageImage UserIcon { get; set; }
    }
    public interface IMildomPhotoGift : IMildomItem
    {
        string GiftTitle { get; }
        string PhotoGiftId { get; }
        string BUrl { get; }
        int Coins { get; }
        string GiftSmallImageUrl { get; }

        string ShareText { get; }
    }
    public interface IMildomGift : IMildomItem
    {
        string GiftTitle { get; }
        string PhotoGiftId { get; }
        string BUrl { get; }
        int Coins { get; }
        string GiftSmallImageUrl { get; }

        int Count { get; }
    }
}