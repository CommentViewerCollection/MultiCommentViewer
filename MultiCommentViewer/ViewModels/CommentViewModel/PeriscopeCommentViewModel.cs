using Common;
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
    public class PeriscopeCommentViewModel : IMcvCommentViewModel
    {
        private readonly PeriscopeSitePlugin.IPeriscopeMessage _message;
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
        private PeriscopeCommentViewModel(IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
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
        public PeriscopeCommentViewModel(PeriscopeSitePlugin.IPeriscopeComment comment, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(metadata, methods, connectionStatus, options)
        {
            _message = comment;

            _nameItems = MessagePartFactory.CreateMessageItems(comment.DisplayName);
            MessageItems = MessagePartFactory.CreateMessageItems(comment.Text);
            Thumbnail = null;
            Id = comment.Id;
            PostTime = comment.PostedAt.HasValue ? comment.PostedAt.Value.ToString("HH:mm:ss") : (string)null;
        }
        public PeriscopeCommentViewModel(PeriscopeSitePlugin.IPeriscopeJoin join, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(metadata, methods, connectionStatus, options)
        {
            _message = join;

            _nameItems = MessagePartFactory.CreateMessageItems(join.DisplayName);
            MessageItems = MessagePartFactory.CreateMessageItems(join.Text);
            //Thumbnail = join..UserIcon;
            //Id = join.Id.ToString();
            //PostTime = join.PostTime;
        }
        public PeriscopeCommentViewModel(PeriscopeSitePlugin.IPeriscopeLeave leave, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(metadata, methods, connectionStatus, options)
        {
            _message = leave;

            _nameItems = MessagePartFactory.CreateMessageItems(leave.DisplayName);
            MessageItems = MessagePartFactory.CreateMessageItems(leave.Text);
            //Thumbnail = join..UserIcon;
            //Id = join.Id.ToString();
            //PostTime = join.PostTime;
        }
        //public PeriscopeCommentViewModel(PeriscopeSitePlugin.IPeriscopeItem item, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionStatus)
        //    : this(metadata, methods, connectionStatus)
        //{
        //    var comment = item;
        //    _message = comment;

        //    _nameItems = comment.NameItems;
        //    MessageItems = comment.CommentItems;
        //    Thumbnail = new Common.MessageImage
        //    {
        //        Url = comment.UserIconUrl,
        //        Alt = "",
        //        Height = 40,//_optionsにcolumnの幅を動的に入れて、ここで反映させたい。propertyChangedはどうやって発生させるか
        //        Width = 40,
        //    };
        //    Id = comment.Id.ToString();
        //    PostTime = UnixtimeToDateTime(comment.PostedAt / 1000).ToString("HH:mm:ss");
        //}
        public PeriscopeCommentViewModel(PeriscopeSitePlugin.IPeriscopeConnected connected, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(metadata, methods, connectionStatus, options)
        {
            _message = connected;
            MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
        }
        public PeriscopeCommentViewModel(PeriscopeSitePlugin.IPeriscopeDisconnected disconnected, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(metadata, methods, connectionStatus, options)
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
                    return new SolidColorBrush(_options.PeriscopeBackColor);
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
                    return new SolidColorBrush(_options.PeriscopeForeColor);
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
    }
}
