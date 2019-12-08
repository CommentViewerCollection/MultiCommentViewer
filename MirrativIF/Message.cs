using SitePlugin;

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

    public interface IMirrativMessage : IMessage
    {
        MirrativMessageType MirrativMessageType { get; }
    }
    public interface IMirrativConnected : IMirrativMessage
    {
    }
    public interface IMirrativDisconnected : IMirrativMessage
    {
    }
    public interface IMirrativComment : IMirrativMessage, IMessageComment
    {
    }
    public interface IMirrativJoinRoom : IMirrativMessage
    {
        //string Comment { get; }
        string Id { get; }
        //string UserName { get; }
        string UserId { get; }
        string PostTime { get; }
        IMessageImage UserIcon { get; set; }
        int OnlineViewerNum { get; }
    }
    public interface IMirrativItem : IMirrativMessage
    {
        string Id { get; }
        string UserId { get; }
        string PostTime { get; }
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