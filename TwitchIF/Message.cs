using Mcv.PluginV2;
using System.Collections.Generic;

namespace TwitchSitePlugin
{
    public enum TwitchMessageType
    {
        Unknown,
        Comment,
        //Item,
        Connected,
        Disconnected,
        Notice,
    }


    public interface ITwitchMessage : ISiteMessage
    {
        TwitchMessageType TwitchMessageType { get; }
    }
    public interface ITwitchConnected : ITwitchMessage
    {
        string Text { get; }
    }
    public interface ITwitchDisconnected : ITwitchMessage
    {
        string Text { get; }
    }
    public interface ITwitchComment : ITwitchMessage
    {
        string Id { get; }
        /// <summary>
        /// NameItemsとDisplayNameが同値か
        /// </summary>
        bool IsDisplayNameSame { get; }
        string DisplayName { get; }
        string UserName { get; }
        IEnumerable<IMessagePart> CommentItems { get; }
        string PostTime { get; }
        IMessageImage UserIcon { get; }
    }
    public interface ITwitchNotice : ITwitchMessage
    {
        string Message { get; }
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