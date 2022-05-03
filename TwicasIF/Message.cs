using Mcv.PluginV2;
using System.Collections.Generic;

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

    public interface ITwicasMessage : ISiteMessage
    {
        TwicasMessageType TwicasMessageType { get; }
    }
    public interface ITwicasConnected : ITwicasMessage
    {
        string Text { get; }
    }
    public interface ITwicasDisconnected : ITwicasMessage
    {
        string Text { get; }
    }
    public interface ITwicasComment : ITwicasMessage
    {
        string UserName { get; }
        IEnumerable<IMessagePart> CommentItems { get; }
        string Id { get; }
        string UserId { get; }
        string PostTime { get; }
        IMessageImage UserIcon { get; }
    }
    public interface ITwicasItem : ITwicasMessage
    {
        string UserName { get; }
        IEnumerable<IMessagePart> CommentItems { get; }
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
    public interface ITwicasKiitos : ITwicasItem
    {
    }
}