using System.Collections.Generic;

namespace SitePlugin
{
    public interface IMessage : IValueChanged
    {
        string Raw { get; }
        SiteType SiteType { get; }
        IEnumerable<IMessagePart> NameItems { get; }
        IEnumerable<IMessagePart> CommentItems { get; }
    }
    public interface IMessageComment : IMessage
    {
        string Id { get; }
        string UserId { get; }
        string PostTime { get; }
        IMessageImage UserIcon { get; set; }
    }
}
