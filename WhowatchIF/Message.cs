using SitePlugin;

namespace WhowatchSitePlugin
{
    public enum WhowatchMessageType
    {
        Unknown,
        Comment,
        Item,
        Connected,
        Disconnected,
    }


    public interface IWhowatchMessage : IMessage
    {
        WhowatchMessageType WhowatchMessageType { get; }
    }
    public interface IWhowatchConnected : IWhowatchMessage
    {
    }
    public interface IWhowatchDisconnected : IWhowatchMessage
    {
    }
    public interface IWhowatchComment : IWhowatchMessage, IMessageComment
    {
        string UserPath { get; }
        string AccountName { get; }
    }
    public interface IWhowatchNgComment : IWhowatchComment
    {
        string OriginalMessage { get; }
    }
    public interface IWhowatchItem : IWhowatchMessage
    {
        string ItemName { get; }
        int ItemCount { get; }
        //string Comment { get; }
        long Id { get; }
        //string UserName { get; }
        string UserPath { get; }
        long UserId { get; }
        string AccountName { get; }
        long PostedAt { get; }
        string UserIconUrl { get; }
    }
}