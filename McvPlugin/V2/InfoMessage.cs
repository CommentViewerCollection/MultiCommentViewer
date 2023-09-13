using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Mcv.PluginV2;

public interface IInfoMessage : ISiteMessage
{
    InfoType Type { get; set; }
    string Text { get; }
    DateTime CreatedAt { get; }
}
public class InfoMessage : IInfoMessage
{
    public InfoType Type { get; set; }
    public string Raw { get; }
    public SiteType SiteType { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; } = DateTime.Now;

    public event EventHandler<ValueChangedEventArgs> ValueChanged;
}
public class InfoMessageContext : IMessageContext
{
    public ISiteMessage Message { get; }
    public string? UserId { get; }
    public string? NewNickname { get; }
    public bool IsInitialComment { get; }
    public IEnumerable<IMessagePart>? UsernameItems { get; }

    public InfoMessageContext(IInfoMessage message, string? userId, IEnumerable<IMessagePart>? usernameItems, string? newNickname, bool isInitialComment)
    {
        Message = message;
        UserId = userId;
        UsernameItems = usernameItems;
        NewNickname = newNickname;
        IsInitialComment = isInitialComment;
    }
    public static InfoMessageContext Create(InfoMessage message)
    {
        var context = new InfoMessageContext(message, null, null, null, false);
        return context;

    }
}
