using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ryu_s.BrowserCookie;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
namespace SitePlugin
{
    public interface ICommentViewModel : INotifyPropertyChanged
    {
        MessageType MessageType { get; }
        IEnumerable<IMessagePart> NameItems { get; }
        IEnumerable<IMessagePart> MessageItems { get; }
        string Info { get; }
        string Id { get; }
        string UserId { get; }
        IUser User { get; }
        ICommentProvider CommentProvider { get; }
        bool Is184 { get; }
        string PostTime { get; }

        /// <summary>
        /// このユーザの最初のコメント
        /// </summary>
        //bool IsFirstComment { get; }

        IMessageImage Thumbnail { get; }

        FontFamily FontFamily { get; }
        FontStyle FontStyle { get; }
        FontWeight FontWeight { get; }
        int FontSize { get; }

        SolidColorBrush Foreground { get; }
        SolidColorBrush Background { get; }

        bool IsVisible { get; }

        bool IsFirstComment { get; }

        Task AfterCommentAdded();
    }
}
