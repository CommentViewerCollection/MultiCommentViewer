using System.ComponentModel;
using System.Windows.Media;

namespace Plugin
{
    public interface IConnectionStatus : INotifyPropertyChanged
    {
        string Name { get; }
        string Guid { get; }
        //bool CanPostComment{get;}//接続中で且つ大抵のサイトではログイン済みである必要がある
        Color BackColor { get; }
        Color ForeColor { get; }
    }
}
