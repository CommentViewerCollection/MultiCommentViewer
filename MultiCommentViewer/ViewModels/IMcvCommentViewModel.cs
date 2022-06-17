using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Plugin;
using SitePlugin;

namespace MultiCommentViewer
{
    public interface IMcvCommentViewModel : INotifyPropertyChanged
    {
        SolidColorBrush Background { get; }
        IConnectionStatus ConnectionName { get; }
        FontFamily FontFamily { get; }
        int FontSize { get; }
        FontStyle FontStyle { get; }
        FontWeight FontWeight { get; }
        SolidColorBrush Foreground { get; }
        string Id { get; }
        string Info { get; }
        bool IsVisible { get; }
        IEnumerable<IMessagePart> MessageItems { get; set; }
        IEnumerable<IMessagePart> NameItems { get; }
        string PostTime { get; }
        IMessageImage Thumbnail { get; }
        string UserId { get; }
        TextWrapping UserNameWrapping { get; }
        bool IsTranslated { get; set; }

        Task AfterCommentAdded();
    }
}