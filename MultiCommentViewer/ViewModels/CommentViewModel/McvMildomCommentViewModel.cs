﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SitePlugin;
using System.Windows.Media;
using System.Windows;
using Plugin;
using Common;

namespace MultiCommentViewer
{
    public class McvMildomCommentViewModel : IMcvCommentViewModel
    {
        private readonly MildomSitePlugin.IMildomMessage _message;
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
        private McvMildomCommentViewModel(IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
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
        public McvMildomCommentViewModel(MildomSitePlugin.IMildomComment comment, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(metadata, methods, connectionStatus, options)
        {
            _message = comment;

            _nameItems = MessagePartFactory.CreateMessageItems(comment.UserName);
            MessageItems = comment.CommentItems;
            Thumbnail = null;
            Id = null;
            PostTime = comment.PostedAt.ToString("HH:mm:ss");
        }
        public McvMildomCommentViewModel(MildomSitePlugin.IMildomJoinRoom item, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(metadata, methods, connectionStatus, options)
        {
            var comment = item;
            _message = comment;

            _nameItems = comment.NameItems;
            MessageItems = comment.CommentItems;
            Thumbnail = comment.UserIcon;
            Id = null;
            PostTime = comment.PostedAt.ToString("HH:mm:ss");
        }
        public McvMildomCommentViewModel(MildomSitePlugin.IMildomGift gift, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(metadata, methods, connectionStatus, options)
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
        public McvMildomCommentViewModel(MildomSitePlugin.IMildomConnected connected, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
            : this(metadata, methods, connectionStatus, options)
        {
            _message = connected;
            MessageItems = Common.MessagePartFactory.CreateMessageItems(connected.Text);
        }
        public McvMildomCommentViewModel(MildomSitePlugin.IMildomDisconnected disconnected, IMessageMetadata metadata, IMessageMethods methods, IConnectionStatus connectionStatus, IOptions options)
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
                return new SolidColorBrush(_metadata.BackColor);
                //}
            }
        }

        public ICommentProvider CommentProvider => _metadata.CommentProvider;

        public FontFamily FontFamily => _metadata.FontFamily;

        public int FontSize => _metadata.FontSize;

        public FontStyle FontStyle => _metadata.FontStyle;

        public FontWeight FontWeight => _metadata.FontWeight;

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
                return new SolidColorBrush(_metadata.ForeColor);
                //}
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

        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dt.AddSeconds(unixTimeStamp).ToLocalTime();
        }
    }
}

