
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Mcv.MainViewPlugin
{
    class McvYouTubeLiveCommentViewModel : IMcvCommentViewModel
    {
        private readonly YouTubeLiveSitePlugin.IYouTubeLiveMessage _message;

        private readonly IMainViewPluginOptions _options;
        private readonly IUser _user;

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
        private McvYouTubeLiveCommentViewModel(ConnectionName connectionStatus, IMainViewPluginOptions options, IUser user)
        {
            ConnectionName = connectionStatus;
            _options = options;
            _user = user;
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
                        //case nameof(_options.IsVisible):
                        //    RaisePropertyChanged(nameof(IsVisible));
                        //    break;
                }
            };
            if (_user != null)
            {
                _user.PropertyChanged += (s, e) =>
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
        public McvYouTubeLiveCommentViewModel(YouTubeLiveSitePlugin.IYouTubeLiveComment comment, IMessageMethods methods, ConnectionName connName, IMainViewPluginOptions options, IUser user)
            : this(connName, options, user)
        {
            _message = comment;

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
        //public McvYouTubeLiveCommentViewModel(YouTubeLiveSitePlugin.IYouTubeLiveSuperchat item, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
        //    : this(metadata, methods, connectionStatus, options)
        //{
        //    var comment = item;
        //    _message = comment;

        //    _nameItems = comment.NameItems;
        //    //MessageItems = comment.CommentItems;

        //    var list = new List<IMessagePart>();
        //    var s = item.PurchaseAmount;
        //    if (item.CommentItems.Count() > 0)
        //        s += Environment.NewLine;
        //    list.Add(MessagePartFactory.CreateMessageText(s));
        //    list.AddRange(item.CommentItems);
        //    MessageItems = list;

        //    Thumbnail = comment.UserIcon;
        //    Id = comment.Id;
        //    PostTime = comment.PostedAt.ToString("HH:mm:ss");
        //}
        //public McvYouTubeLiveCommentViewModel(YouTubeLiveSitePlugin.IYouTubeLiveMembership comment, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
        //    : this(metadata, methods, connectionStatus, options)
        //{
        //    _message = comment;

        //    _nameItems = comment.NameItems;
        //    var messageItems = new List<IMessagePart>();
        //    messageItems.AddRange(comment.HeaderPrimaryTextItems);
        //    messageItems.AddRange(comment.HeaderSubTextItems);
        //    messageItems.AddRange(comment.CommentItems);
        //    MessageItems = messageItems;
        //    Thumbnail = comment.UserIcon;
        //    Id = comment.Id.ToString();
        //    PostTime = comment.PostedAt.ToString("HH:mm:ss");
        //    Info = "メンバー登録";
        //}
        //public McvYouTubeLiveCommentViewModel(YouTubeLiveSitePlugin.IYouTubeLiveConnected connected, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
        //    : this(metadata, methods, connectionStatus, options)
        //{
        //    _message = connected;
        //    MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
        //}
        //public McvYouTubeLiveCommentViewModel(YouTubeLiveSitePlugin.IYouTubeLiveDisconnected disconnected, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
        //    : this(metadata, methods, connectionStatus, options)
        //{
        //    _message = disconnected;
        //    MessageItems = Common.MessagePartFactory.CreateMessageItems(disconnected.Text);
        //}

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

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dt.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}