using Mcv.PluginV2;
using Mcv.YouTubeLiveSitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Mcv.MainViewPlugin;
abstract class CommentViewModelBase : ViewModelBase, IMcvCommentViewModel
{
    public string? UserId => User?.UserId;
    public ConnectionName ConnectionName { get; }
    protected readonly IMainViewPluginOptions _options;
    public MyUser? User { get; }

    public CommentViewModelBase(ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
    {
        ConnectionName = connName;
        _options = options;
        User = user;
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
                    //case nameof(options.IsVisible):
                    //    RaisePropertyChanged(nameof(IsVisible));
                    //    break;
            }
        };
        if (user is not null)
        {
            user.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(user.Nickname):
                        SetNickname(user);
                        RaisePropertyChanged(nameof(NameItems));
                        break;
                    case nameof(user.BackColorArgb):
                        RaisePropertyChanged(nameof(Background));
                        break;
                    case nameof(user.ForeColorArgb):
                        RaisePropertyChanged(nameof(Foreground));
                        break;
                }
            };
        }
    }
    private void SetNickname(MyUser user)
    {
        if (!string.IsNullOrEmpty(user.Nickname))
        {
            _nickItems = new List<IMessagePart> { MessagePartFactory.CreateMessageText(user.Nickname) };
        }
        else
        {
            _nickItems = null;
        }
    }
    protected abstract SolidColorBrush CreateSiteForeground();
    public SolidColorBrush Foreground
    {
        get
        {
            if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Site)
            {
                return CreateSiteForeground();
            }
            else if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection)
            {
                return new SolidColorBrush(ConnectionName.ForeColor);
            }
            else if (User?.ForeColorArgb is not null)
            {
                return new SolidColorBrush(Utils.ColorFromArgb(User.ForeColorArgb));
            }
            else
            {
                return new SolidColorBrush(_options.ForeColor);
            }
        }
    }
    protected abstract SolidColorBrush CreateSiteBackground();
    public SolidColorBrush Background
    {
        get
        {
            if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Site)
            {
                return CreateSiteBackground();
            }
            else if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Connection)
            {
                return new SolidColorBrush(ConnectionName.BackColor);
            }
            else if (User?.BackColorArgb is not null)
            {
                return new SolidColorBrush(Utils.ColorFromArgb(User.BackColorArgb));
            }
            else
            {
                return new SolidColorBrush(_options.BackColor);
            }
        }
    }
    private IEnumerable<IMessagePart>? _nickItems { get; set; }
    protected IEnumerable<IMessagePart>? _nameItems;
    public IEnumerable<IMessagePart>? NameItems
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
    private IEnumerable<IMessagePart> _messageItems;
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
    public FontFamily FontFamily => _options.FontFamily;

    public int FontSize => _options.FontSize;

    public FontStyle FontStyle => _options.FontStyle;

    public FontWeight FontWeight => _options.FontWeight;

    public string Id { get; protected set; }

    public string Info { get; protected set; }

    public bool IsVisible => true;

    public string PostTime { get; protected set; }
    public IMessageImage Thumbnail { get; protected set; }
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
}
class McvYouTubeLiveCommentViewModel : CommentViewModelBase
{
    private McvYouTubeLiveCommentViewModel(ConnectionName connectionStatus, IMainViewPluginOptions options, MyUser? user)
        : base(connectionStatus, options, user)
    {
    }
    public McvYouTubeLiveCommentViewModel(IYouTubeLiveComment comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
        : this(connName, options, user)
    {
        _nameItems = comment.NameItems;
        MessageItems = comment.CommentItems;
        Thumbnail = comment.UserIcon;
        Id = comment.Id.ToString();
        PostTime = comment.PostedAt.ToString("HH:mm:ss");

        connName.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(connName.BackColor):
                    RaisePropertyChanged(nameof(Background));
                    break;
                case nameof(connName.ForeColor):
                    RaisePropertyChanged(nameof(Foreground));
                    break;
            }
        };
    }
    public McvYouTubeLiveCommentViewModel(IYouTubeLiveSuperchat item, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
        : this(connName, options, user)
    {
        _nameItems = item.NameItems;
        //MessageItems = comment.CommentItems;

        var list = new List<IMessagePart>();
        var s = item.PurchaseAmount;
        if (item.CommentItems.Count() > 0)
            s += Environment.NewLine;
        list.Add(MessagePartFactory.CreateMessageText(s));
        list.AddRange(item.CommentItems);
        MessageItems = list;

        Thumbnail = item.UserIcon;
        Id = item.Id;
        PostTime = item.PostedAt.ToString("HH:mm:ss");
    }
    public McvYouTubeLiveCommentViewModel(IYouTubeLivePaidSticker sticker, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
        : this(connName, options, user)
    {
        _nameItems = sticker.NameItems;

        var list = new List<IMessagePart>();
        list.Add(MessagePartFactory.CreateMessageText(sticker.PurchaseAmount + Environment.NewLine));
        list.Add(new MessageImage
        {
            Url = sticker.StickerUrl,
            Alt = sticker.StickerTooltip,
            Width = sticker.StickerWidth,
            Height = sticker.StickerHeight,
        });
        MessageItems = list;

        Thumbnail = sticker.UserIcon;
        Id = sticker.Id;
        PostTime = sticker.PostedAt.ToString("HH:mm:ss");
    }
    public McvYouTubeLiveCommentViewModel(IYouTubeLiveSponsorshipsGiftPurchaseAnnouncement sticker, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
        : this(connName, options, user)
    {
        _nameItems = sticker.NameItems;

        var list = new List<IMessagePart>();
        list.AddRange(sticker.MessageItems);
        MessageItems = list;

        Thumbnail = sticker.UserIcon;
        Id = sticker.Id;
        PostTime = sticker.PostedAt.ToString("HH:mm:ss");
    }
    public McvYouTubeLiveCommentViewModel(IYouTubeLiveMembership comment, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
        : this(connName, options, user)
    {
        _nameItems = comment.NameItems;

        if (comment.HeaderPrimaryTextItems == null || !comment.CommentItems.Any())
        {
            //メンバーシップ登録
            var messageItems = new List<IMessagePart>();
            messageItems.AddRange(comment.HeaderSubTextItems);
            MessageItems = messageItems;
            Thumbnail = comment.UserIcon;
            Id = comment.Id.ToString();
            PostTime = comment.PostedAt.ToString("HH:mm:ss");
            Info = "メンバーシップ登録";
        }
        else
        {
            //メンバーシップメッセージ
            var messageItems = new List<IMessagePart>();
            messageItems.AddRange(comment.HeaderPrimaryTextItems);
            messageItems.Add(MessagePartFactory.CreateMessageText(Environment.NewLine));
            messageItems.AddRange(comment.HeaderSubTextItems);
            messageItems.Add(MessagePartFactory.CreateMessageText(Environment.NewLine));
            messageItems.AddRange(comment.CommentItems);
            MessageItems = messageItems;
            Thumbnail = comment.UserIcon;
            Id = comment.Id.ToString();
            PostTime = comment.PostedAt.ToString("HH:mm:ss");
            Info = "メンバーシップメッセージ";
        }
    }
    public McvYouTubeLiveCommentViewModel(IYouTubeLiveConnected connected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
        : this(connName, options, user)
    {
        MessageItems = MessagePartFactory.CreateMessageItems(connected.Text);
    }
    public McvYouTubeLiveCommentViewModel(IYouTubeLiveDisconnected disconnected, ConnectionName connName, IMainViewPluginOptions options, MyUser? user)
        : this(connName, options, user)
    {
        MessageItems = MessagePartFactory.CreateMessageItems(disconnected.Text);
    }
    protected override SolidColorBrush CreateSiteForeground()
    {
        return new SolidColorBrush(_options.ForeColor);
    }

    protected override SolidColorBrush CreateSiteBackground()
    {
        return new SolidColorBrush(_options.BackColor);
    }
}