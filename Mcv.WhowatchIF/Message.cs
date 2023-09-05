using Mcv.PluginV2;

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


    public interface IWhowatchMessage : ISiteMessage
    {
        WhowatchMessageType WhowatchMessageType { get; }
    }
    public interface IWhowatchConnected : IWhowatchMessage
    {
        string Text { get; }
    }
    public interface IWhowatchDisconnected : IWhowatchMessage
    {
        string Text { get; }
    }
    public interface IWhowatchComment : IWhowatchMessage
    {
        string UserName { get; }
        string Comment { get; }
        string UserPath { get; }
        string UserId { get; }
        string Id { get; }
        string PostTime { get; }
        IMessageImage UserIcon { get; }
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
        string Comment { get; }
        long Id { get; }
        string UserName { get; }
        string UserPath { get; }
        long UserId { get; }
        string AccountName { get; }
        long PostedAt { get; }
        string UserIconUrl { get; }
    }
}