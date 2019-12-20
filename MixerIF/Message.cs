using SitePlugin;

namespace MixerSitePlugin
{
    public enum MixerMessageType
    {
        Unknown,
        Comment,
        JoinRoom,
        Connected,
        Disconnected,
        Item,
    }

    public interface IMixerMessage : IMessage
    {
        MixerMessageType MixerMessageType { get; }
    }
    public interface IMixerConnected : IMixerMessage
    {
    }
    public interface IMixerDisconnected : IMixerMessage
    {
    }
    public interface IMixerComment : IMixerMessage, IMessageComment
    {
    }
    public interface IMixerJoinRoom : IMixerMessage
    {
        //string Comment { get; }
        string Id { get; }
        //string UserName { get; }
        string UserId { get; }
        string PostTime { get; }
        IMessageImage UserIcon { get; set; }
        int OnlineViewerNum { get; }
    }
    public interface IMixerItem : IMixerMessage
    {
        string Id { get; }
        string UserId { get; }
        string PostTime { get; }
        //IMessageImage UserIcon { get; set; }
    }
    public interface IMixerPhotoGift : IMixerItem
    {
        string GiftTitle { get; }
        string PhotoGiftId { get; }
        string BUrl { get; }
        int Coins { get; }
        string GiftSmallImageUrl { get; }

        string ShareText { get; }
    }
    public interface IMixerGift : IMixerItem
    {
        string GiftTitle { get; }
        string PhotoGiftId { get; }
        string BUrl { get; }
        int Coins { get; }
        string GiftSmallImageUrl { get; }

        int Count { get; }
    }
}