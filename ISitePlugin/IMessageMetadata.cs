using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace SitePlugin
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMessageMetadata:INotifyPropertyChanged
    {
        Color BackColor { get; }
        Color ForeColor { get; }
        FontFamily FontFamily { get; }
        int FontSize { get; }
        FontWeight FontWeight { get; }
        FontStyle FontStyle { get; }
        bool IsNgUser { get; }
        bool IsSiteNgUser { get; }
        bool IsFirstComment { get; }
        bool IsInitialComment { get; }
        bool Is184 { get; }
        IUser User { get; }
        ICommentProvider CommentProvider { get; }
        bool IsVisible { get; }
    }
}
