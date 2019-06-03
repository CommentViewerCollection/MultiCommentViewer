using SitePlugin;

namespace NicoSitePlugin
{
    public enum NicoMessageType
    {
        Unknown,
        Comment,
        Connected,
        Disconnected,
        Ad,
        Item,
        Kick,
        Info,
        Ignored,
    }

    public interface INicoMessage : IMessage
    {
        NicoMessageType NicoMessageType { get; }
    }
    public interface INicoConnected : INicoMessage
    {
    }
    public interface INicoDisconnected : INicoMessage
    {
    }
    public interface INicoComment : INicoMessage, IMessageComment
    {
        bool Is184 { get; }
        string RoomName { get; }
        int? ChatNo { get; }
    }
    public interface INicoAd : INicoMessage
    {
        string RoomName { get; }
        int? ChatNo { get; }
        string PostTime { get; }
    }
    public interface INicoItem : INicoMessage
    {
        string RoomName { get; }
        int? ChatNo { get; }
        string PostTime { get; }
        string ItemName { get; }
        int ItemCount { get; }
    }
    public interface INicoKickCommand : INicoMessage
    {

    }
    public interface INicoInfo : INicoMessage
    {
        int No { get; }
        string PostTime { get; }
    }
}