using System.ComponentModel;
namespace Mcv.MainViewPlugin;

internal class MyUser : INotifyPropertyChanged
{
    public string? UserId { get; }
    public string? Nickname { get; internal set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    public MyUser(string? userId)
    {
        UserId = userId;
    }
}
