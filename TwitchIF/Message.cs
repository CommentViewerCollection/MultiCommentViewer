using SitePlugin;

namespace TwitchSitePlugin
{
    public enum TwitchMessageType
    {
        Unknown,
        Comment,
        //Item,
        Connected,
        Disconnected,
    }


    public interface ITwitchMessage : IMessage
    {
        TwitchMessageType TwitchMessageType { get; }
    }
    public interface ITwitchConnected : ITwitchMessage
    {
    }
    public interface ITwitchDisconnected : ITwitchMessage
    {
    }
    public interface ITwitchComment : ITwitchMessage, IMessageComment
    {
    }
    //public interface ITwitchItem : ITwitchMessage
    //{
    //    string ItemName { get; }
    //    int ItemCount { get; }
    //    //string Comment { get; }
    //    long Id { get; }
    //    //string UserName { get; }
    //    string UserPath { get; }
    //    long UserId { get; }
    //    string AccountName { get; }
    //    long PostedAt { get; }
    //    string UserIconUrl { get; }
    //}
}