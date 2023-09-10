using Mcv.PluginV2;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System;
using MildomSitePlugin;

namespace Mcv.MainViewPlugin;

class McvMildomCommentViewModel : IMcvCommentViewModel
{
    private readonly IMildomMessage _message;
    private readonly IMainViewPluginOptions _options;
    private readonly MyUser? _user;

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
    private McvMildomCommentViewModel(ConnectionName connectionStatus, IMainViewPluginOptions options, MyUser? user)
    {
        ConnectionName = connectionStatus;
        _options = options;
        ConnectionName.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(ConnectionName.Name):
                    RaisePropertyChanged(nameof(ConnectionName));
                    break;
                case nameof(ConnectionName.BackColor):
                    RaisePropertyChanged(nameof(Background));
                    break;
                case nameof(ConnectionName.ForeColor):
                    RaisePropertyChanged(nameof(Foreground));
                    break;
            }
        };
        options.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(options.IsEnabledSiteConnectionColor):
                    RaisePropertyChanged(nameof(Background));
                    RaisePropertyChanged(nameof(Foreground));
                    break;
            }
        };
        _options.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(_options.BackColor):
                    RaisePropertyChanged(nameof(Background));
                    break;
                case nameof(_options.ForeColor):
                    RaisePropertyChanged(nameof(Foreground));
                    break;
                case nameof(_options.FontFamily):
                    RaisePropertyChanged(nameof(FontFamily));
                    break;
                case nameof(_options.FontStyle):
                    RaisePropertyChanged(nameof(FontStyle));
                    break;
                case nameof(_options.FontWeight):
                    RaisePropertyChanged(nameof(FontWeight));
                    break;
                case nameof(_options.FontSize):
                    RaisePropertyChanged(nameof(FontSize));
                    break;
                case nameof(_options.IsUserNameWrapping):
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
    public McvMildomCommentViewModel(MildomSitePlugin.IMildomComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
             : this(connName, options, user)
    {
        _message = comment;

        _nameItems = MessagePartFactory.CreateMessageItems(comment.UserName);
        MessageItems = comment.CommentItems;
        Thumbnail = null;
        Id = null;
        PostTime = comment.PostedAt.ToString("HH:mm:ss");
    }
    public McvMildomCommentViewModel(MildomSitePlugin.IMildomJoinRoom item, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
: this(connName, options, user)
    {
        var comment = item;
        _message = comment;

        _nameItems = comment.NameItems;
        MessageItems = comment.CommentItems;
        Thumbnail = comment.UserIcon;
        Id = null;
        PostTime = comment.PostedAt.ToString("HH:mm:ss");
    }
    public McvMildomCommentViewModel(MildomSitePlugin.IMildomGift gift, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
: this(connName, options, user)
    {
        _message = gift;
        MessageItems = new List<IMessagePart>
            {
                new MessageImage
                {
                     Alt=gift.GiftName,
                      Url=gift.GiftUrl,
                       Width=40,
                        Height=40,
                },
                MessagePartFactory.CreateMessageText($"を贈りました × {gift.Count}"),
            };
        _nameItems = MessagePartFactory.CreateMessageItems(gift.UserName);
        //MessageItems = comment.CommentItems;
        //Thumbnail = null;
        //Id = null;
        PostTime = gift.PostedAt.ToString("HH:mm:ss");
        Info = gift.GiftName;
    }
    public McvMildomCommentViewModel(MildomSitePlugin.IMildomConnected connected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
             : this(connName, options, user)
    {
        _message = connected;
        MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
    }
    public McvMildomCommentViewModel(MildomSitePlugin.IMildomDisconnected disconnected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
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

    public SolidColorBrush Background
    {
        get
        {
            //if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Site)
            //{
            //    return new SolidColorBrush(_options.MildomBackColor);
            //}
            //else if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection)
            //{
            //    return new SolidColorBrush(ConnectionName.BackColor);
            //}
            //else
            //{
            return new SolidColorBrush(_options.BackColor);
            //}
        }
    }

    public FontFamily FontFamily => _options.FontFamily;

    public int FontSize => _options.FontSize;

    public FontStyle FontStyle => _options.FontStyle;

    public FontWeight FontWeight => _options.FontWeight;

    public SolidColorBrush Foreground
    {
        get
        {
            //if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Site)
            //{
            //    return new SolidColorBrush(_options.MildomForeColor);
            //}
            //else if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection)
            //{
            //    return new SolidColorBrush(ConnectionName.ForeColor);
            //}
            //else
            //{
            return new SolidColorBrush(_options.ForeColor);
            //}
        }
    }

    public string Id { get; private set; }

    public string Info { get; private set; }

    public bool IsVisible => true;

    public string PostTime { get; private set; }

    public IMessageImage Thumbnail { get; private set; }

    public string UserId => _user?.UserId;

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
    private System.ComponentModel.PropertyChangedEventHandler? _propertyChanged;
    private IEnumerable<IMessagePart> _messageItems;

    /// <summary>
    /// 
    /// </summary>
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged
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