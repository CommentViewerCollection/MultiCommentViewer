using BigoSitePlugin;
using Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
namespace MultiCommentViewer
{
    public class McvBigoCommentViewModel : McvBigoCommentViewModelBase
    {
        public McvBigoCommentViewModel(IBigoComment comment, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : base(metadata, methods, connectionStatus, options)
        {
            MessageItems = Common.MessagePartFactory.CreateMessageItems(comment.Message);
            _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.Name);
            //Id = comment.Id;
            PostTime = comment.PostedAt.ToString("HH:mm:ss");
        }
    }
    public class McvBigoGiftViewModel : McvBigoCommentViewModelBase
    {
        public McvBigoGiftViewModel(IBigoGift comment, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : base(metadata, methods, connectionStatus, options)
        {
            MessageItems = new List<IMessagePart>
            {
                new Common.MessageImage
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
    public class McvBigoCommentViewModelBase : IMcvCommentViewModel
    {
        private readonly YouTubeLiveSitePlugin.IYouTubeLiveMessage _message;
        private readonly IMessageMetadata _metadata;
        private readonly IMessageMethods _methods;
        private readonly IOptions _options;

        private void SetNickname(IUser user)
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
        public McvBigoCommentViewModelBase(IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
        {
            _metadata = metadata;
            _methods = methods;

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
            _metadata.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_metadata.BackColor):
                        RaisePropertyChanged(nameof(Background));
                        break;
                    case nameof(_metadata.ForeColor):
                        RaisePropertyChanged(nameof(Foreground));
                        break;
                    case nameof(_metadata.FontFamily):
                        RaisePropertyChanged(nameof(FontFamily));
                        break;
                    case nameof(_metadata.FontStyle):
                        RaisePropertyChanged(nameof(FontStyle));
                        break;
                    case nameof(_metadata.FontWeight):
                        RaisePropertyChanged(nameof(FontWeight));
                        break;
                    case nameof(_metadata.FontSize):
                        RaisePropertyChanged(nameof(FontSize));
                        break;
                    case nameof(_metadata.IsNameWrapping):
                        RaisePropertyChanged(nameof(UserNameWrapping));
                        break;
                    case nameof(_metadata.IsVisible):
                        RaisePropertyChanged(nameof(IsVisible));
                        break;
                }
            };
            if (_metadata.User != null)
            {
                var user = _metadata.User;
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

        public IConnectionStatus ConnectionName { get; }

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

        public IEnumerable<IMessagePart> MessageItems { get; protected set; }

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
                    return new SolidColorBrush(_metadata.BackColor);
                }
            }
        }

        public FontFamily FontFamily => _metadata.FontFamily;

        public int FontSize => _metadata.FontSize;

        public FontStyle FontStyle => _metadata.FontStyle;

        public FontWeight FontWeight => _metadata.FontWeight;

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
                    return new SolidColorBrush(_metadata.ForeColor);
                }
            }
        }

        public string Id { get; protected set; }

        public string Info { get; protected set; }

        public bool IsVisible => _metadata.IsVisible;

        public string PostTime { get; protected set; }

        public IMessageImage Thumbnail { get; private set; }

        public string UserId => _metadata.User?.UserId;

        public TextWrapping UserNameWrapping
        {
            get
            {
                if (_metadata.IsNameWrapping)
                {
                    return TextWrapping.Wrap;
                }
                else
                {
                    return TextWrapping.NoWrap;
                }
            }
        }

        IEnumerable<IMessagePart> IMcvCommentViewModel.MessageItems { get; set; }
        public bool IsTranslated { get; set; }

        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
        #region INotifyPropertyChanged
        [NonSerialized]
        private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
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

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dt.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}

