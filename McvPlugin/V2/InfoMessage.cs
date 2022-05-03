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

    public InfoMessageContext(IInfoMessage message, string? userId, string? newNickname)
    {
        Message = message;
        UserId = userId;
        NewNickname = newNickname;
    }
    public static InfoMessageContext Create(InfoMessage message)
    {
        var context = new InfoMessageContext(message, null, null);
        return context;

    }
}
