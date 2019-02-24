using SitePlugin;

namespace OpenrecSitePlugin
{
    public enum OpenrecMessageType
    {
        Unknown,
        Comment,
        //Item,
        Stamp,
        Yell,
        Connected,
        Disconnected,
    }


    public interface IOpenrecMessage : IMessage
    {
        OpenrecMessageType OpenrecMessageType { get; }
    }
    public interface IOpenrecConnected : IOpenrecMessage
    {
    }
    public interface IOpenrecDisconnected : IOpenrecMessage
    {
    }
    public interface IOpenrecComment : IOpenrecMessage, IMessageComment
    {
    }
    public interface IOpenrecStamp : IOpenrecMessage, IMessageComment
    {
    }
    public interface IOpenrecYell : IOpenrecMessage, IMessageComment
    {
    }
    //public interface IOpenrecItem : IOpenrecMessage
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