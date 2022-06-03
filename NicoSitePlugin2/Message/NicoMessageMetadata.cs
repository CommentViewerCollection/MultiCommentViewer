using SitePlugin;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace NicoSitePlugin
{
    internal interface INicoMessageMetadata : IMessageMetadata
    {

    }
    internal abstract class MessageMetadataBase : INicoMessageMetadata
    {
        //protected readonly IMirrativMessage _message;
        protected readonly ICommentOptions _options;
        protected readonly INicoSiteOptions _siteOptions;

        public virtual Color BackColor => _options.BackColor;

        public virtual Color ForeColor => _options.ForeColor;

        public virtual FontFamily FontFamily => _options.FontFamily;

        public virtual int FontSize => _options.FontSize;

        public virtual FontWeight FontWeight => _options.FontWeight;

        public virtual FontStyle FontStyle => _options.FontStyle;

        public virtual bool IsNgUser => false;
        public bool IsSiteNgUser => false;//TODO:IUserにIsSiteNgUserを追加する
        public virtual bool IsFirstComment => false;
        public string SiteName { get; }
        public bool Is184 { get; set; }
        public IUser User { get; protected set; }
        public ICommentProvider CommentProvider { get; protected set; }
        public virtual bool IsVisible
        {
            get
            {
                if (IsNgUser || IsSiteNgUser || (!_siteOptions.IsShow184 && Is184)) return false;

                //TODO:ConnectedとかDisconnectedの場合、表示するエラーレベルがError以下の場合にfalseにしたい
                return true;
            }
        }
        public bool IsInitialComment { get; set; }
        public bool IsNameWrapping => _options.IsUserNameWrapping;
        public Guid SiteContextGuid { get; set; }
        public ISiteOptions SiteOptions => _siteOptions;

        protected MessageMetadataBase(ICommentOptions options, INicoSiteOptions siteOptions)
        {
            _options = options;
            _siteOptions = siteOptions;
            options.PropertyChanged += Options_PropertyChanged;
            siteOptions.PropertyChanged += SiteOptions_PropertyChanged;
        }


        protected void SiteOptions_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_siteOptions.IsAutoSetNickname):
                    RaisePropertyChanged(nameof(IsNameWrapping));
                    break;
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
    internal class CommentMessageMetadata : MessageMetadataBase
    {
        public override Color BackColor
        {
            get
            {
                if (!string.IsNullOrEmpty(User.BackColorArgb))
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
                if (!string.IsNullOrEmpty(User.ForeColorArgb))
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
        public override bool IsNgUser => User.IsNgUser;

        private readonly INicoComment _comment;
        private bool _isFirstComment;
        public override bool IsFirstComment => _isFirstComment;
        public override bool IsVisible => base.IsVisible;
        public CommentMessageMetadata(INicoComment comment, ICommentOptions options, INicoSiteOptions siteOptions, IUser user, ICommentProvider cp, bool isFirstComment)
            : base(options, siteOptions)
        {
            Debug.Assert(user != null);
            User = user;
            CommentProvider = cp;
            _comment = comment;
            _isFirstComment = isFirstComment;
            Is184 = comment.Is184;
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
    internal class InfoMessageMetadata : MessageMetadataBase
    {
        public override Color BackColor => _options.InfoBackColor;
        public override Color ForeColor => _options.InfoForeColor;

        public InfoMessageMetadata(INicoInfo info, ICommentOptions options, INicoSiteOptions siteOptions)
            : base(options, siteOptions)
        {
        }
    }
    internal class ConnectedMessageMetadata : MessageMetadataBase
    {
        private readonly INicoConnected _connected;

        public override Color BackColor => _options.InfoBackColor;
        public override Color ForeColor => _options.InfoForeColor;

        public ConnectedMessageMetadata(INicoConnected connected, ICommentOptions options, INicoSiteOptions siteOptions)
            : base(options, siteOptions)
        {
            _connected = connected;
        }
    }
    internal class AdMessageMetadata : MessageMetadataBase
    {
        private readonly INicoAd _ad;

        public override Color BackColor => _siteOptions.AdBackColor;
        public override Color ForeColor => _siteOptions.AdForeColor;

        public AdMessageMetadata(INicoAd ad, ICommentOptions options, INicoSiteOptions siteOptions)
            : base(options, siteOptions)
        {
            _ad = ad;
        }
    }
    internal class ItemMessageMetadata : MessageMetadataBase
    {
        private readonly INicoGift _item;

        public override Color BackColor => _siteOptions.ItemBackColor;
        public override Color ForeColor => _siteOptions.ItemForeColor;

        public ItemMessageMetadata(INicoGift item, ICommentOptions options, INicoSiteOptions siteOptions)
            : base(options, siteOptions)
        {
            _item = item;
        }
    }
    internal class SpiMessageMetadata : MessageMetadataBase
    {
        private readonly INicoSpi _item;

        public override Color BackColor => _siteOptions.SpiBackColor;
        public override Color ForeColor => _siteOptions.SpiForeColor;

        public SpiMessageMetadata(INicoSpi item, ICommentOptions options, INicoSiteOptions siteOptions)
            : base(options, siteOptions)
        {
            _item = item;
        }
    }
    internal class EmotionMessageMetadata : MessageMetadataBase
    {
        private readonly INicoEmotion _item;

        public override Color BackColor => _siteOptions.EmotionBackColor;
        public override Color ForeColor => _siteOptions.EmotionForeColor;

        public EmotionMessageMetadata(INicoEmotion item, ICommentOptions options, INicoSiteOptions siteOptions, IUser user, ICommentProvider cp)
            : base(options, siteOptions)
        {
            _item = item;
            User = user;
            CommentProvider = cp;

        }
    }
    internal class DisconnectedMessageMetadata : MessageMetadataBase
    {
        public override Color BackColor => _options.InfoBackColor;
        public override Color ForeColor => _options.InfoForeColor;

        public DisconnectedMessageMetadata(INicoDisconnected comment, ICommentOptions options, INicoSiteOptions siteOptions)
            : base(options, siteOptions)
        {
        }
    }
}
