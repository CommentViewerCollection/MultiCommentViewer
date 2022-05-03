using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Mcv.MainViewPlugin
{
    //class ConnectionMetadata : INotifyPropertyChanged
    //{
    //    public Guid Guid { get; }
    //    private string _name;
    //    public string Name
    //    {
    //        get { return _name; }
    //        set
    //        {
    //            _name = value;
    //            RaisePropertyChanged();
    //        }
    //    }
    //    public Color BackColor { get; set; }
    //    public Color ForeColor { get; set; }
    //    public ConnectionMetadata()
    //    {
    //        Guid = Guid.NewGuid();
    //    }


    //}
    interface IMcvCommentViewModel
    {
        string UserId { get; }
    }

    public interface IMessageMetadata : INotifyPropertyChanged
    {
        Color BackColor { get; }
        FontFamily FontFamily { get; }
        int FontSize { get; }
        FontStyle FontStyle { get; }
        FontWeight FontWeight { get; }
        Color ForeColor { get; }
        bool IsVisible { get; }
        bool IsNameWrapping { get; }
    }
    public interface IMessageMethods { }
}