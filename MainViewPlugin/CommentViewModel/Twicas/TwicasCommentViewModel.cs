using Mcv.PluginV2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TwicasSitePlugin;

namespace Mcv.MainViewPlugin;
class TwicasCommentViewModel : IMcvCommentViewModel
{
    private readonly ITwicasMessage _message;
    private readonly IMainViewPluginOptions _options;
    public MyUser? User { get; }
    private void SetNickname(MyUser user)
    {
        if (!string.IsNullOrEmpty(user.Nickname))
        {
            _nickItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(user.Nickname) };
        }
        else
        {
            _nickItems = null;
        }
    }
    private TwicasCommentViewModel(ConnectionName connectionStatus, IMainViewPluginOptions options, MyUser? user)
    {
        ConnectionName = connectionStatus;
        ConnectionName.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(ConnectionName.Name):
                    RaisePropertyChanged(nameof(ConnectionName));
                    break;
            }
        };
        options.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(options.BackColor):
                    RaisePropertyChanged(nameof(Background));
                    break;
                case nameof(options.ForeColor):
                    RaisePropertyChanged(nameof(Foreground));
                    break;
                case nameof(options.FontFamily):
                    RaisePropertyChanged(nameof(FontFamily));
                    break;
                case nameof(options.FontStyle):
                    RaisePropertyChanged(nameof(FontStyle));
                    break;
                case nameof(options.FontWeight):
                    RaisePropertyChanged(nameof(FontWeight));
                    break;
                case nameof(options.FontSize):
                    RaisePropertyChanged(nameof(FontSize));
                    break;
                case nameof(options.IsUserNameWrapping):
                    RaisePropertyChanged(nameof(UserNameWrapping));
                    break;
            }
        };
        if (user != null)
        {
            user.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(user.Nickname):
                        SetNickname(user);
                        RaisePropertyChanged(nameof(NameItems));
                        break;
                }
            };
            SetNickname(user);
        }
    }
    public TwicasCommentViewModel(ITwicasComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
         : this(connName, options, user)
    {
        _message = comment;

        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.UserName);
        MessageItems = comment.CommentItems;
        Thumbnail = comment.UserIcon;
        Id = comment.Id?.ToString();
        PostTime = comment.PostTime;
    }
    public TwicasCommentViewModel(ITwicasItem item, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
          : this(connName, options, user)
    {
        _message = item;

        _nameItems = Common.MessagePartFactory.CreateMessageItems(item.UserName);
        MessageItems = item.CommentItems;
        Thumbnail = item.UserIcon;
        Id = null;
        PostTime = null;// comment.PostTime;
        Info = item.ItemName;
    }
    public TwicasCommentViewModel(ITwicasConnected connected, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
        : this(connName, options, user)
    {
        _message = connected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
    }
    public TwicasCommentViewModel(ITwicasDisconnected disconnected, ConnectionName connName, IMainViewPluginOptions options, MyUser user)
            : this(connName, options, user)
    {
        _message = disconnected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(disconnected.Text);
    }

    public ConnectionName ConnectionName { get; }

    private IEnumerable<IMessagePart> _nickItems { get; set; }
    private IEnumerable<IMessagePart> _nameItems { get; set; }
    public IEnumerable<IMessagePart> NameItems
    {
        get
        {
            if (_nickItems != null)
            {
                return _nickItems;
            }
            else
            {
                return _nameItems;
            }
        }
    }

    public IEnumerable<IMessagePart> MessageItems
    {
        get
        {
            return _messageItems;
        }
        set
        {
            _messageItems = value;
            RaisePropertyChanged();
        }
    }

    public SolidColorBrush Background => new SolidColorBrush(_options.BackColor);

    public FontFamily FontFamily => _options.FontFamily;

    public int FontSize => _options.FontSize;

    public FontStyle FontStyle => _options.FontStyle;

    public FontWeight FontWeight => _options.FontWeight;

    public SolidColorBrush Foreground => new SolidColorBrush(_options.ForeColor);

    public string Id { get; private set; }

    public string Info { get; private set; }

    public bool IsVisible => true;

    public string PostTime { get; private set; }

    public IMessageImage Thumbnail { get; private set; }

    public string UserId => User?.UserId;

    public TextWrapping UserNameWrapping
    {
        get
        {
            if (_options.IsUserNameWrapping)
            {
                return TextWrapping.Wrap;
            }
            else
            {
                return TextWrapping.NoWrap;
            }
        }
    }

    public bool IsTranslated { get; set; }

    public Task AfterCommentAdded()
    {
        return Task.CompletedTask;
    }
    #region INotifyPropertyChanged
    [NonSerialized]
    private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
    private IEnumerable<IMessagePart> _messageItems;

    /// <summary>
    /// 
    /// </summary>
    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
    {
        add { _propertyChanged += value; }
        remove { _propertyChanged -= value; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
