using SitePlugin;
using System.Windows;
using System.Windows.Media;

namespace WhowatchSitePlugin
{
    internal class CommentMessageMetadata : MessageMetadataBase
    {
        public override Color BackColor
        {
            get
            {
                if (User != null && !string.IsNullOrEmpty(User.BackColorArgb))
                {
                    return Common.Utils.ColorFromArgb(User.BackColorArgb);
                }
                else if (IsFirstComment)
                {
                    return _options.FirstCommentBackColor;
                }
                else
                {
                    return base.BackColor;
                }
            }
        }
        public override Color ForeColor
        {
            get
            {
                if (User != null && !string.IsNullOrEmpty(User.ForeColorArgb))
                {
                    return Common.Utils.ColorFromArgb(User.ForeColorArgb);
                }
                else if (IsFirstComment)
                {
                    return _options.FirstCommentForeColor;
                }
                else
                {
                    return base.ForeColor;
                }
            }
        }
        public override FontFamily FontFamily
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontFamily;
                }
                else
                {
                    return base.FontFamily;
                }
            }
        }
        public override int FontSize
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontSize;
                }
                else
                {
                    return base.FontSize;
                }
            }
        }
        public override FontStyle FontStyle
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontStyle;
                }
                else
                {
                    return base.FontStyle;
                }
            }
        }
        public override FontWeight FontWeight
        {
            get
            {
                if (IsFirstComment)
                {
                    return _options.FirstCommentFontWeight;
                }
                else
                {
                    return base.FontWeight;
                }
            }
        }
        public override bool IsNgUser => User.IsNgUser;
        public CommentMessageMetadata(IWhowatchMessage message, ICommentOptions options, IWhowatchSiteOptions siteOptions, IUser user, ICommentProvider cp, bool isFirstComment)
            : base(options, siteOptions, cp)
        {
            User = user;
            IsFirstComment = isFirstComment;
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
    }
}
