using Mcv.PluginV2;
using System.Collections.Generic;
using System.ComponentModel;
namespace Mcv.MainViewPlugin;

internal class MyUser : ViewModelBase
{
    private string? _nickname;

    public string UserId { get; }
    public string? Nickname
    {
        get => _nickname;
        internal set
        {
            _nickname = value;
            RaisePropertyChanged();
        }
    }
    private string? _foreColorArgb;
    public string? ForeColorArgb
    {
        get
        {
            return _foreColorArgb;
        }
        internal set
        {
            _foreColorArgb = value;
            RaisePropertyChanged();
        }
    }
    private string? _backColorArgb;
    public string? BackColorArgb
    {
        get
        {
            return _backColorArgb;
        }
        internal set
        {
            _backColorArgb = value;
            RaisePropertyChanged();
        }
    }
    private bool _isNgUser;
    public bool IsNgUser
    {
        get
        {
            return _isNgUser;
        }
        internal set
        {
            _isNgUser = value;
            RaisePropertyChanged();
        }
    }
    public IEnumerable<IMessagePart>? Name { get; internal set; }
    public bool IsSiteNgUser { get; internal set; }
    public MyUser(string userId)
    {
        UserId = userId;
    }
}
