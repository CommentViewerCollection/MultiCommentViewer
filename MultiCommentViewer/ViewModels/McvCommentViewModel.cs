using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SitePlugin;
using System.Windows.Media;
using System.Windows;

namespace MultiCommentViewer
{
    public class McvWhowatchCommentViewModel : IMcvCommentViewModel
    {
        private readonly WhowatchSitePlugin.IWhowatchMessage _message;
        private readonly IMessageMetadata _metadata;
        private readonly IMessageMethods _methods;
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
        private McvWhowatchCommentViewModel(IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
        {
            _metadata = metadata;
            _methods = methods;

            ConnectionName = connectionName;
            ConnectionName.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ConnectionName.Name):
                        RaisePropertyChanged(nameof(ConnectionName));
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
        public McvWhowatchCommentViewModel(WhowatchSitePlugin.IWhowatchComment comment, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
            : this(metadata, methods, connectionName)
        {
            _message = comment;

            _nameItems = comment.NameItems;
            MessageItems = comment.CommentItems;
            Thumbnail = comment.UserIcon;
            Id = comment.Id.ToString();
            PostTime = comment.PostTime;
        }
        public McvWhowatchCommentViewModel(WhowatchSitePlugin.IWhowatchItem item, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
            : this(metadata, methods, connectionName)
        {
            var comment = item;
            _message = comment;

            _nameItems = comment.NameItems;
            MessageItems = comment.CommentItems;
            Thumbnail = new Common.MessageImage
            {
                Url = comment.UserIconUrl,
                Alt = "",
                Height = 40,//_optionsにcolumnの幅を動的に入れて、ここで反映させたい。propertyChangedはどうやって発生させるか
                Width = 40,
            };
            Id = comment.Id.ToString();
            PostTime = UnixtimeToDateTime(comment.PostedAt / 1000).ToString("HH:mm:ss");
        }
        public McvWhowatchCommentViewModel(WhowatchSitePlugin.IWhowatchConnected connected, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
            : this(metadata, methods, connectionName)
        {
            _message = connected;
            MessageItems = connected.CommentItems;
        }
        public McvWhowatchCommentViewModel(WhowatchSitePlugin.IWhowatchDisconnected disconnected, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
            : this(metadata, methods, connectionName)
        {
            _message = disconnected;
            MessageItems = disconnected.CommentItems;
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

        public IEnumerable<IMessagePart> MessageItems { get; private set; }

        public SolidColorBrush Background => new SolidColorBrush(_metadata.BackColor);

        public ICommentProvider CommentProvider => _metadata.CommentProvider;

        public FontFamily FontFamily => _metadata.FontFamily;

        public int FontSize => _metadata.FontSize;

        public FontStyle FontStyle => _metadata.FontStyle;

        public FontWeight FontWeight => _metadata.FontWeight;

        public SolidColorBrush Foreground => new SolidColorBrush(_metadata.ForeColor);

        public string Id { get; private set; }

        public string Info { get; private set; }

        public bool IsVisible => true;

        public string PostTime { get; private set; }

        public IMessageImage Thumbnail { get; private set; }

        public string UserId => _metadata.User?.UserId;

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

        private static DateTime UnixtimeToDateTime(long unixTimeStamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dt.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
    public class McvYouTubeLiveCommentViewModel : IMcvCommentViewModel
    {
        private readonly YouTubeLiveSitePlugin.IYouTubeLiveMessage _message;
        private readonly IMessageMetadata _metadata;
        private readonly IMessageMethods _methods;
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
        private McvYouTubeLiveCommentViewModel(IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
        {
            _metadata = metadata;
            _methods = methods;

            ConnectionName = connectionName;
            ConnectionName.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ConnectionName.Name):
                        RaisePropertyChanged(nameof(ConnectionName));
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
        public McvYouTubeLiveCommentViewModel(YouTubeLiveSitePlugin.IYouTubeLiveComment comment, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
            : this(metadata, methods, connectionName)
        {
            _message = comment;

            _nameItems = comment.NameItems;
            MessageItems = comment.CommentItems;
            Thumbnail = comment.UserIcon;
            Id = comment.Id.ToString();
            PostTime = comment.PostTime;
        }
        public McvYouTubeLiveCommentViewModel(YouTubeLiveSitePlugin.IYouTubeLiveSuperchat item, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
            : this(metadata, methods, connectionName)
        {
            var comment = item;
            _message = comment;

            _nameItems = comment.NameItems;
            MessageItems = comment.CommentItems;
            Thumbnail = comment.UserIcon;
            Id = comment.Id.ToString();
            PostTime = comment.PostTime;
        }
        public McvYouTubeLiveCommentViewModel(YouTubeLiveSitePlugin.IYouTubeLiveConnected connected, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
            : this(metadata, methods, connectionName)
        {
            _message = connected;
            MessageItems = connected.CommentItems;
        }
        public McvYouTubeLiveCommentViewModel(YouTubeLiveSitePlugin.IYouTubeLiveDisconnected disconnected, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
            : this(metadata, methods, connectionName)
        {
            _message = disconnected;
            MessageItems = disconnected.CommentItems;
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

        public IEnumerable<IMessagePart> MessageItems { get; private set; }

        public SolidColorBrush Background => new SolidColorBrush(_metadata.BackColor);

        public ICommentProvider CommentProvider => _metadata.CommentProvider;

        public FontFamily FontFamily => _metadata.FontFamily;

        public int FontSize => _metadata.FontSize;

        public FontStyle FontStyle => _metadata.FontStyle;

        public FontWeight FontWeight => _metadata.FontWeight;

        public SolidColorBrush Foreground => new SolidColorBrush(_metadata.ForeColor);

        public string Id { get; private set; }

        public string Info { get; private set; }

        public bool IsVisible => true;

        public string PostTime { get; private set; }

        public IMessageImage Thumbnail { get; private set; }

        public string UserId => _metadata.User?.UserId;

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
    public class McvMirrativCommentViewModel : IMcvCommentViewModel
    {
        private readonly MirrativSitePlugin.IMirrativMessage _message;
        private readonly IMessageMetadata _metadata;
        private readonly IMessageMethods _methods;
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
        private McvMirrativCommentViewModel(IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
        {
            _metadata = metadata;
            _methods = methods;

            ConnectionName = connectionName;
            ConnectionName.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ConnectionName.Name):
                        RaisePropertyChanged(nameof(ConnectionName));
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
        public McvMirrativCommentViewModel(MirrativSitePlugin.IMirrativComment comment, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
            : this(metadata, methods, connectionName)
        {
            _message = comment;

            _nameItems = comment.NameItems;
            MessageItems = comment.CommentItems;
            Thumbnail = comment.UserIcon;
            Id = comment.Id.ToString();
            //PostTime = UnixTimeStampToDateTime(comment.PostedAt / 1000).ToString("HH:mm:ss");
            PostTime = comment.PostTime;
        }
        public McvMirrativCommentViewModel(MirrativSitePlugin.IMirrativJoinRoom item, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
            : this(metadata, methods, connectionName)
        {
            var comment = item;
            _message = comment;

            _nameItems = comment.NameItems;
            MessageItems = comment.CommentItems;
            Thumbnail = comment.UserIcon;
            Id = null;
            PostTime = comment.PostTime;
        }
        public McvMirrativCommentViewModel(MirrativSitePlugin.IMirrativConnected connected, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
            : this(metadata, methods, connectionName)
        {
            _message = connected;
            MessageItems = connected.CommentItems;
        }
        public McvMirrativCommentViewModel(MirrativSitePlugin.IMirrativDisconnected disconnected, IMessageMetadata metadata, IMessageMethods methods, ConnectionName connectionName)
            : this(metadata, methods, connectionName)
        {
            _message = disconnected;
            MessageItems = disconnected.CommentItems;
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

        public IEnumerable<IMessagePart> MessageItems { get; private set; }

        public SolidColorBrush Background => new SolidColorBrush(_metadata.BackColor);

        public ICommentProvider CommentProvider => _metadata.CommentProvider;

        public FontFamily FontFamily => _metadata.FontFamily;

        public int FontSize => _metadata.FontSize;

        public FontStyle FontStyle => _metadata.FontStyle;

        public FontWeight FontWeight => _metadata.FontWeight;

        public SolidColorBrush Foreground => new SolidColorBrush(_metadata.ForeColor);

        public string Id { get; private set; }

        public string Info { get; private set; }

        public bool IsVisible => true;

        public string PostTime { get; private set; }

        public IMessageImage Thumbnail { get; private set; }

        public string UserId => _metadata.User?.UserId;

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
    public class McvCommentViewModel : IMcvCommentViewModel
    {
        private readonly ICommentViewModel _cvm;

        public McvCommentViewModel(ICommentViewModel cvm, ConnectionName connectionName)
        {
            _cvm = cvm;
            _cvm.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_cvm.FontFamily):
                        RaisePropertyChanged(nameof(FontFamily));
                        break;
                    case nameof(_cvm.FontStyle):
                        RaisePropertyChanged(nameof(FontStyle));
                        break;
                    case nameof(_cvm.FontWeight):
                        RaisePropertyChanged(nameof(FontWeight));
                        break;
                    case nameof(_cvm.FontSize):
                        RaisePropertyChanged(nameof(FontSize));
                        break;
                    case nameof(_cvm.NameItems):
                        RaisePropertyChanged(nameof(NameItems));
                        break;
                    case nameof(_cvm.IsVisible):
                        RaisePropertyChanged(nameof(IsVisible));
                        break;

                }
            };
            ConnectionName = connectionName;
            ConnectionName.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(ConnectionName.Name):
                        RaisePropertyChanged(nameof(connectionName));
                        break;
                }
            };
        }

        public ConnectionName ConnectionName { get; }

        public IEnumerable<IMessagePart> NameItems => _cvm.NameItems;

        public IEnumerable<IMessagePart> MessageItems => _cvm.MessageItems;

        public string Info => _cvm.Info;

        public string Id => _cvm.Id;

        public string UserId => _cvm.UserId;

        //public IUser User => _cvm.User;

        public string PostTime => _cvm.PostTime;

        public ICommentProvider CommentProvider => _cvm.CommentProvider;

        public IMessageImage Thumbnail => _cvm.Thumbnail;

        public FontFamily FontFamily => _cvm.FontFamily;

        public FontStyle FontStyle => _cvm.FontStyle;

        public FontWeight FontWeight => _cvm.FontWeight;

        public int FontSize => _cvm.FontSize;

        public SolidColorBrush Foreground => _cvm.Foreground;

        public SolidColorBrush Background => _cvm.Background;

        public bool IsVisible => _cvm.IsVisible;

        public Task AfterCommentAdded()
        {
            throw new NotImplementedException();
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
    }
}

