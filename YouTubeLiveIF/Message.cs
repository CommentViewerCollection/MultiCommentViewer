using SitePlugin;

namespace YouTubeLiveSitePlugin
{
    public enum YouTubeLiveMessageType
    {
        Unknown,
        Comment,
        Superchat,
        Connected,
        Disconnected,
    }


    public interface IYouTubeLiveMessage : IMessage
    {
        YouTubeLiveMessageType YouTubeLiveMessageType { get; }
    }
    public interface IYouTubeLiveConnected : IYouTubeLiveMessage
    {
    }
    public interface IYouTubeLiveDisconnected : IYouTubeLiveMessage
    {
    }
    public interface IYouTubeLiveComment : IYouTubeLiveMessage, IMessageComment
    {
    }
    public interface IYouTubeLiveSuperchat : IYouTubeLiveMessage, IMessageComment
    {
    }
}