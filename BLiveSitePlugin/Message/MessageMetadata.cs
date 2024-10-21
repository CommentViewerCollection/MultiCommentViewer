using SitePlugin;
using System;
using System.Windows;
using System.Windows.Media;

namespace BLiveSitePlugin
{
    internal class MessageMetadata : IMessageMetadata
    {
        private readonly IBLiveMessage _message;
        private readonly ICommentOptions _options;
        private readonly IBLiveSiteOptions _siteOptions;

        public Color BackColor
        {
            get
            {
                if (User != null && !string.IsNullOrEmpty(User.BackColorArgb))
                {
                    var color = Common.Utils.ColorFromArgb(User.BackColorArgb);
                    return color;
                }
                else if (IsFirstComment)
                {
                    return _options.FirstCommentBackColor;
                }
                //if (_message is IBLiveItem item)
                //{
                //    return _siteOptions.ItemBackColor;
                //}
                else
                {
                    return _options.BackColor;
                }
            }
        }

        public Color ForeColor
        {
            get
            {
                if (User != null && !string.IsNullOrEmpty(User.ForeColorArgb))
                {
                    var color = Common.Utils.ColorFromArgb(User.ForeColorArgb);
                    return color;
                }
                else if (IsFirstComment)
                {
                    return _options.FirstCommentForeColor;
                }
                //if (_message is IBLiveItem item)
                //{
                //    return _siteOptions.ItemForeColor;
                //}
                else
                {
                    return _options.ForeColor;
                }
            }
        }

        public FontFamily FontFamily
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontFamily;
                }
                else
                {
                    return _options.FontFamily;
                }
            }
        }

        public int FontSize
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontSize;
                }
                else
                {
                    return _options.FontSize;
                }
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontWeight;
                }
                else
                {
                    return _options.FontWeight;
                }
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontStyle;
                }
                else
                {
                    return _options.FontStyle;
                }
            }
        }

        public bool IsNgUser => User != null ? User.IsNgUser : false;
        public bool IsSiteNgUser => false;//TODO:IUserにIsSiteNgUserを追加する
        public bool IsFirstComment { get; }
        public bool Is184 { get; }
        public IUser User { get; }
        public ICommentProvider CommentProvider { get; }
        public bool IsVisible
        {
            get
            {
                if (IsNgUser || IsSiteNgUser) return false;

                //TODO:ConnectedとかDisconnectedの場合、表示するエラーレベルがError以下の場合にfalseにしたい
                //→Connected,Disconnectedくらいは常に表示でも良いかも。エラーメッセージだけエラーレベルを設けようか。
                return true;
            }
        }
        public bool IsInitialComment { get; set; }
        public bool IsNameWrapping => _options.IsUserNameWrapping;
        public Guid SiteContextGuid { get; set; }
        public ISiteOptions SiteOptions { get; }

        public MessageMetadata(IBLiveMessage message, ICommentOptions options, IBLiveSiteOptions siteOptions, IUser user, ICommentProvider cp, bool isFirstComment)
        {
            _message = message;
            _options = options;
            _siteOptions = siteOptions;
            IsFirstComment = isFirstComment;
            User = user;
            CommentProvider = cp;

            //TODO:siteOptionsのpropertyChangedが発生したら関係するプロパティの変更通知を出したい

            options.PropertyChanged += Options_PropertyChanged;
            siteOptions.PropertyChanged += SiteOptions_PropertyChanged;
            user.PropertyChanged += User_PropertyChanged;
        }

        private void User_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(User.IsNgUser):
                    //case nameof(User.IsSiteNgUser):
                    RaisePropertyChanged(nameof(IsVisible));
                    break;
                case nameof(User.BackColorArgb):
                    RaisePropertyChanged(nameof(BackColor));
                    break;
                case nameof(User.ForeColorArgb):
                    RaisePropertyChanged(nameof(ForeColor));
                    break;
            }
        }

        private void SiteOptions_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                //case nameof(_siteOptions.ItemBackColor):
                //    if (_message is IBLiveItem)
                //    {
                //        RaisePropertyChanged(nameof(BackColor));
                //    }
                //    break;
                //case nameof(_siteOptions.ItemForeColor):
                //    if (_message is IBLiveItem)
                //    {
                //        RaisePropertyChanged(nameof(ForeColor));
                //    }
                //    break;
            }
        }

        private void Options_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_options.BackColor):
                    RaisePropertyChanged(nameof(BackColor));
                    break;
                case nameof(_options.ForeColor):
                    RaisePropertyChanged(nameof(ForeColor));
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
                case nameof(_options.FirstCommentFontFamily):
                    RaisePropertyChanged(nameof(FontFamily));
                    break;
                case nameof(_options.FirstCommentFontStyle):
                    RaisePropertyChanged(nameof(FontStyle));
                    break;
                case nameof(_options.FirstCommentFontWeight):
                    RaisePropertyChanged(nameof(FontWeight));
                    break;
                case nameof(_options.FirstCommentFontSize):
                    RaisePropertyChanged(nameof(FontSize));
                    break;
                case nameof(_options.IsUserNameWrapping):
                    RaisePropertyChanged(nameof(IsNameWrapping));
                    break;
            }
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
