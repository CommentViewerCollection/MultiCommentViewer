using System;
using System.Collections.Generic;
using SitePlugin;
using Common;
using System.Windows.Media;
using System.ComponentModel;

namespace TwicasSitePlugin
{
    class TwicasCommentViewModel : CommentViewModelBase
    {
        public override string UserId { get; }
        private IUser _user;
        public override IUser User => _user;
        public override bool IsVisible
        {
            get
            {
                return base.IsVisible;
            }

            protected set
            {
                if (base.IsVisible == value)
                    return;
                base.IsVisible = value;
                RaisePropertyChanged();
            }
        }
        public override SolidColorBrush Background
        {
            get
            {
                if (!string.IsNullOrEmpty(_user.BackColorArgb))
                {
                    return new SolidColorBrush(Tools.ColorFromArgb(_user.BackColorArgb));
                }
                else
                {
                    return base.Background;
                }
            }
        }
        public override SolidColorBrush Foreground
        {
            get
            {
                if (!string.IsNullOrEmpty(_user.ForeColorArgb))
                {
                    return new SolidColorBrush(Tools.ColorFromArgb(_user.ForeColorArgb));
                }
                else
                {
                    return base.Foreground;
                }
            }
        }
        private void SetVisibility(IUser user)
        {
            IsVisible = !user.IsNgUser;
        }
        public TwicasCommentViewModel(ICommentOptions options, ICommentData data, IUser user) :
            base(options)
        {
            UserId = data.UserId;
            Id = data.Id.ToString();
            NameItems = new List<IMessagePart> { new MessageText(data.Name) };
            MessageItems = data.Message;
            Thumbnail = new MessageImage { Url = data.ThumbnailUrl, Height = data.ThumbnailHeight, Width = data.ThumbnailWidth };
            _user = user;

            //いずれNameの型をIEnumerable<IMessagePart>にした。とりあえずstringで。
            _user.Name = NameItems;
            
            PostTime = data.Date.ToString("HH:mm:ss");
            SetVisibility(user);
            user.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(user.IsNgUser):
                        SetVisibility(user);
                        break;
                    case nameof(user.BackColorArgb):
                        RaisePropertyChanged(nameof(Background));
                        break;
                    case nameof(user.ForeColorArgb):
                        RaisePropertyChanged(nameof(Foreground));
                        break;
                    case nameof(user.Nickname):
                        RaisePropertyChanged(nameof(NameItems));
                        break;
                }
            };
        }
    }
    public class TwicasOptionsViewModel : INotifyPropertyChanged
    {
        private readonly TwicasSiteOptions _origin;
        private readonly TwicasSiteOptions _changed;
        internal TwicasSiteOptions OriginOptions { get { return _origin; } }
        internal TwicasSiteOptions ChangedOptions { get { return _changed; } }

        internal TwicasOptionsViewModel(TwicasSiteOptions siteOptions)
        {
            _origin = siteOptions;
            _changed = siteOptions.Clone();
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
