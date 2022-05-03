using System.ComponentModel;

namespace Mcv.MainViewPlugin
{
    interface IUser : INotifyPropertyChanged
    {
        string UserId { get; }

        string Nickname { get; }
    }
}