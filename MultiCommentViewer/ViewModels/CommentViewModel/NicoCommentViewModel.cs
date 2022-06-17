using Common;
using NicoSitePlugin;
using Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MultiCommentViewer
{
    public class NicoCommentViewModel : IMcvCommentViewModel
    {
        private readonly NicoSitePlugin.INicoMessage _message;
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
        private NicoCommentViewModel(NicoSitePlugin.INicoMessage message, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
        {
            _message = message;
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
            _message.ValueChanged += (s, e) =>
            {
                if (_message is INicoComment nicoComment)
                {
                    switch (e.PropertyName)
                    {
                        case nameof(nicoComment.UserName):
                            RaisePropertyChanged(nameof(NameItems));
                            break;
                    }
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
        public NicoCommentViewModel(NicoSitePlugin.INicoComment comment, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(comment as NicoSitePlugin.INicoMessage, metadata, methods, connectionStatus, options)
        {
            //if (!string.IsNullOrEmpty(comment.UserName))
            //{
            //    _nameItems = MessagePartFactory.CreateMessageItems(comment.UserName);
            //}
            var siteOptions = (INicoSiteOptions)metadata.SiteOptions;
            siteOptions.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(siteOptions.IsAutoSetNickname):
                        {
                            SetNameItems(comment, metadata, siteOptions);
                        }
                        break;
                }
            };
            SetNameItems(comment, metadata, siteOptions);
            MessageItems = MessagePartFactory.CreateMessageItems(comment.Text);
            if (IsValudThumbnailUrl(comment.ThumbnailUrl))
            {
                Thumbnail = new MessageImage
                {
                    Url = comment.ThumbnailUrl,
                    Height = 40,
                    Width = 40,
                };
            }
            Id = comment.Id;
            PostTime = comment.PostedAt.ToLocalTime().ToString("HH:mm:ss");
        }

        private void SetNameItems(INicoComment comment, IMessageMetadata metadata, INicoSiteOptions siteOptions)
        {
            if (!comment.Is184)
            {
                if (siteOptions.IsAutoGetUsername)
                {
                    _nameItems = metadata.User.Name;
                    RaisePropertyChanged(nameof(NameItems));
                }
                else
                {
                    _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.UserId);
                    RaisePropertyChanged(nameof(NameItems));
                }
            }
            else
            {
                if (siteOptions.IsShow184Id)
                {
                    _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.UserId);
                    RaisePropertyChanged(nameof(NameItems));
                }
                else
                {
                    _nameItems = null;
                    RaisePropertyChanged(nameof(NameItems));
                }
            }
        }

        public NicoCommentViewModel(NicoSitePlugin.INicoAd ad, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(ad as NicoSitePlugin.INicoMessage, metadata, methods, connectionStatus, options)
        {
            //_nameItems = MessagePartFactory.CreateMessageItems(ad.UserName);
            MessageItems = MessagePartFactory.CreateMessageItems(ad.Text);
            PostTime = ad.PostedAt.ToString("HH:mm:ss");
            Info = "広告";
        }
        public NicoCommentViewModel(NicoSitePlugin.INicoGift item, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(item as NicoSitePlugin.INicoMessage, metadata, methods, connectionStatus, options)
        {
            //_nameItems = MessagePartFactory.CreateMessageItems(item.UserName);
            MessageItems = MessagePartFactory.CreateMessageItems(item.Text);
            PostTime = item.PostedAt.ToString("HH:mm:ss");
            Info = "ギフト";
        }
        public NicoCommentViewModel(NicoSitePlugin.INicoSpi item, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
   : this(item as NicoSitePlugin.INicoMessage, metadata, methods, connectionStatus, options)
        {
            //_nameItems = MessagePartFactory.CreateMessageItems(item.UserName);
            MessageItems = MessagePartFactory.CreateMessageItems(item.Text);
            PostTime = item.PostedAt.ToString("HH:mm:ss");
            Info = "リクエスト";
        }
        public NicoCommentViewModel(NicoSitePlugin.INicoEmotion item, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(item as NicoSitePlugin.INicoMessage, metadata, methods, connectionStatus, options)
        {
            //_nameItems = MessagePartFactory.CreateMessageItems(item.UserName);
            MessageItems = MessagePartFactory.CreateMessageItems(item.Content);
            PostTime = item.PostedAt.ToString("HH:mm:ss");
            Info = "エモーション";
        }
        public NicoCommentViewModel(NicoSitePlugin.INicoInfo info, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(info as NicoSitePlugin.INicoMessage, metadata, methods, connectionStatus, options)
        {
            //_nameItems = MessagePartFactory.CreateMessageItems(info.UserName);
            MessageItems = MessagePartFactory.CreateMessageItems(info.Text);
            PostTime = info.PostedAt.ToString("HH:mm:ss");
        }
        public NicoCommentViewModel(NicoSitePlugin.INicoConnected connected, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(connected as NicoSitePlugin.INicoMessage, metadata, methods, connectionStatus, options)
        {
            _message = connected;
            MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
        }
        public NicoCommentViewModel(NicoSitePlugin.INicoDisconnected disconnected, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(disconnected as NicoSitePlugin.INicoMessage, metadata, methods, connectionStatus, options)
        {
            _message = disconnected;
            MessageItems = Common.MessagePartFactory.CreateMessageItems(disconnected.Text);
        }

        public IConnectionStatus ConnectionName { get; }

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
                if (_options.IsEnabledSiteConnectionColor && _options.SiteConnectionColorType == SiteConnectionColorType.Site)
                {
                    return new SolidColorBrush(_options.NicoLiveBackColor);
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
                    return new SolidColorBrush(_options.NicoLiveForeColor);
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

        public string Id { get; private set; }

        public string Info { get; private set; }

        public bool IsVisible => _metadata.IsVisible;

        public string PostTime { get; private set; }

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

        private static DateTime UnixtimeToDateTime(long unixTimeStamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dt.AddSeconds(unixTimeStamp).ToLocalTime();
        }
        private static bool IsValudThumbnailUrl(string thumbnailUrl)
        {
            return !string.IsNullOrEmpty(thumbnailUrl);
        }
    }
}
