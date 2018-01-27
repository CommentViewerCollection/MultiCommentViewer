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
        string ConnectionName { get;  }
        IEnumerable<IMessagePart> NameItems { get; }
        IEnumerable<IMessagePart> MessageItems { get; }
        string Info { get; }
        string Id { get; }
        string Nickname { get;  }

        bool IsInfo { get;  }

        bool IsFirstComment { get; }

        IEnumerable<IMessagePart> Thumbnail { get; }

        FontFamily FontFamily { get; }
        FontStyle FontStyle { get; }
        FontWeight FontWeight { get; }
        int FontSize { get; }

        SolidColorBrush Foreground { get; }
        SolidColorBrush Background { get; }

        Task AfterCommentAdded();
    }
}
