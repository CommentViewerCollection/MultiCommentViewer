using SitePlugin;

namespace TwicasSitePlugin
{
    public enum TwicasMessageType
    {
        Unknown,
        Comment,
        Item,
        Connected,
        Disconnected,
    }

    public interface ITwicasMessage : IMessage
    {
        TwicasMessageType TwicasMessageType { get; }
    }
    public interface ITwicasConnected : ITwicasMessage
    {
    }
    public interface ITwicasDisconnected : ITwicasMessage
    {
    }
    public interface ITwicasComment : ITwicasMessage, IMessageComment
    {
    }
    public interface ITwicasItem : ITwicasMessage
    {
        string ItemName { get; }
        string ItemId { get; }
        //    int ItemCount { get; }
        //    long Id { get; }
        //    //string UserName { get; }
        //    string UserPath { get; }
        //    long UserId { get; }
        //    string AccountName { get; }
        //    long PostedAt { get; }
        IMessageImage UserIcon { get; }
        string UserId { get; }
    }
    public interface ITwicasKiitos: ITwicasItem
    {
    }
}