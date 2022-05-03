using Mcv.PluginV2;
using System;

namespace MirrativSitePlugin
{
    public enum MirrativMessageType
    {
        Unknown,
        Comment,
        JoinRoom,
        Connected,
        Disconnected,
        Item,
    }

    public interface IMirrativMessage : ISiteMessage
    {
        MirrativMessageType MirrativMessageType { get; }
    }
    public interface IMirrativConnected : IMirrativMessage
    {
        string Text { get; }
    }
    public interface IMirrativDisconnected : IMirrativMessage
    {
        string Text { get; }
    }
    public interface IMirrativComment : IMirrativMessage
    {
        string Text { get; }
        string UserName { get; }
        string Id { get; }
        string UserId { get; }
        DateTime PostedAt { get; }
    }
    public interface IMirrativJoinRoom : IMirrativMessage
    {
        string Text { get; }
        string Id { get; }
        string UserName { get; }
        string UserId { get; }
        string PostTime { get; }
        IMessageImage UserIcon { get; set; }
        int OnlineViewerNum { get; }
    }
    public interface IMirrativItem : IMirrativMessage
    {
        string Text { get; }
        string UserName { get; }
        string Id { get; }
        string UserId { get; }
        DateTime PostedAt { get; }
        //IMessageImage UserIcon { get; set; }
    }
    //public interface IMirrativPhotoGift : IMirrativItem
    //{
    //    string GiftTitle { get; }
    //    string PhotoGiftId { get; }
    //    string BUrl { get; }
    //    int Coins { get; }
    //    string GiftSmallImageUrl { get; }

    //    string ShareText { get; }
    //}
    //public interface IMirrativGift : IMirrativItem
    //{
    //    string GiftTitle { get; }
    //    string PhotoGiftId { get; }
    //    string BUrl { get; }
    //    int Coins { get; }
    //    string GiftSmallImageUrl { get; }

    //    int Count { get; }
    //}
}