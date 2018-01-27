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
    public interface ICommentProvider
    {
        event EventHandler<List<ICommentViewModel>> CommentsReceived;
        event EventHandler<IMetadata> MetadataUpdated;
        Task PostCommentAsync(string text);
        Task ConnectAsync(string input, IBrowserProfile browserProfile);
        void Disconnect();
        List<ICommentViewModel> GetUserComments(IUser user);
        bool CanConnect { get; }
        bool CanDisconnect { get; }
        event EventHandler CanConnectChanged;
        event EventHandler CanDisconnectChanged;
    }
    public interface IUser
    {
        string UserId { get; }
        string Nickname { get; }
    }


}
