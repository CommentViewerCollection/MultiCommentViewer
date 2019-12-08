using SitePlugin;

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

    public interface IMildomMessage : IMessage
    {
        MildomMessageType MildomMessageType { get; }
    }
    public interface IMildomConnected : IMildomMessage
    {
    }
    public interface IMildomDisconnected : IMildomMessage
    {
    }
    public interface IMildomComment : IMildomMessage, IMessageComment
    {
    }
    public interface IMildomJoinRoom : IMildomMessage
    {
        //string Comment { get; }
        string Id { get; }
        //string UserName { get; }
        string UserId { get; }
        string PostTime { get; }
        IMessageImage UserIcon { get; set; }
        int OnlineViewerNum { get; }
    }
    public interface IMildomItem : IMildomMessage
    {
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