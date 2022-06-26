using Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MultiCommentViewer
{
    public class TestCommentViewModel : IMcvCommentViewModel
    {
        private readonly TestSitePlugin.ITestMessage _message;
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
        private TestCommentViewModel(IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus)
        {
            _metadata = metadata;
            _methods = methods;

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
        public TestCommentViewModel(TestSitePlugin.ITestComment comment, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus)
            : this(metadata, methods, connectionStatus)
        {
            _message = comment;

            _nameItems = Common.MessagePartFactory.CreateMessageItems(comment.UserName);
            MessageItems = Common.MessagePartFactory.CreateMessageItems(comment.Text);
            Thumbnail = null;
            Id = null;
            PostTime = comment.PostedAt.ToString("HH:mm:ss");
        }
        public TestCommentViewModel(TestSitePlugin.ITestConnected connected, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus)
            : this(metadata, methods, connectionStatus)
        {
            _message = connected;
            MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
        }
        public TestCommentViewModel(TestSitePlugin.ITestDisconnected disconnected, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus)
            : this(metadata, methods, connectionStatus)
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

        public SolidColorBrush Background => new SolidColorBrush(_metadata.BackColor);

        public FontFamily FontFamily => _metadata.FontFamily;

        public int FontSize => _metadata.FontSize;

        public FontStyle FontStyle => _metadata.FontStyle;

        public FontWeight FontWeight => _metadata.FontWeight;

        public SolidColorBrush Foreground => new SolidColorBrush(_metadata.ForeColor);

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
    }
}
