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
    }
}