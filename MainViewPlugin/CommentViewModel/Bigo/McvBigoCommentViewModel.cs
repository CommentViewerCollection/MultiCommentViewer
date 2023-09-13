using BigoSitePlugin;
using Mcv.PluginV2;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;

class McvBigoCommentViewModel : McvBigoCommentViewModelBase
{
    public McvBigoCommentViewModel(IBigoComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
       : base(connName, options, user)
    {
        MessageItems = Common.MessagePartFactory.CreateMessageItems(comment.Message);
        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.Name);
        //Id = comment.Id;
        PostTime = comment.PostedAt.ToString("HH:mm:ss");
    }
}
class McvBigoGiftViewModel : McvBigoCommentViewModelBase
{
    public McvBigoGiftViewModel(IBigoGift comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
       : base(connName, options, user)
    {
        MessageItems = new List<IMessagePart>
            {
                new MessageImage
                {
                     Alt = comment.GiftName,
                      Height=40,
                       Width=40,
                        Url = comment.GiftImgUrl
                },
                Common.MessagePartFactory.CreateMessageText($"×{comment.GiftCount}")
            };
        _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.Username);
        //Id = comment.Id;
        //PostTime = comment.PostedAt.ToString("HH:mm:ss");
    }
}
class McvBigoCommentViewModelBase : IMcvCommentViewModel
{
    private readonly IBigoMessage _message;
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
    public McvBigoCommentViewModelBase(ConnectionName connectionStatus, IMainViewPluginOptions options, MyUser? user)
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
    }

    public ConnectionName ConnectionName { get; }

    private IEnumerable<IMessagePart> _nickItems { get; set; }
    protected IEnumerable<IMessagePart> _nameItems { get; set; }
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

    public IEnumerable<IMessagePart> MessageItems { get; set; }

    public SolidColorBrush Background
    {
        get
        {
            if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Site)
            {
                return new SolidColorBrush(_options.YouTubeLiveBackColor);
            }
            else if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection)
            {
                return new SolidColorBrush(ConnectionName.BackColor);
            }
            else
            {
                return new SolidColorBrush(_options.BackColor);
            }
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
            if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Site)
            {
                return new SolidColorBrush(_options.YouTubeLiveForeColor);
            }
            else if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection)
            {
                return new SolidColorBrush(ConnectionName.ForeColor);
            }
            else
            {
                return new SolidColorBrush(_options.ForeColor);
            }
        }
    }

    public string Id { get; protected set; }

    public string Info { get; protected set; }

    public bool IsVisible => true;

    public string PostTime { get; protected set; }

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

    #region INotifyPropertyChanged
    [NonSerialized]
    private System.ComponentModel.PropertyChangedEventHandler? _propertyChanged;
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