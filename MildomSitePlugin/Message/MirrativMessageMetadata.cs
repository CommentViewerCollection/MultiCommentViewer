using SitePlugin;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace MildomSitePlugin
{
    internal interface IMildomMessageMetadata : IMessageMetadata
    {

    }
    internal abstract class MessageMetadataBase : IMildomMessageMetadata
    {
        //protected readonly IMildomMessage _message;
        protected readonly ICommentOptions _options;
        protected readonly IMildomSiteOptions _siteOptions;

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
        public bool Is184 { get; }
        public IUser User { get; protected set; }
        public ICommentProvider CommentProvider { get; protected set; }
        public bool IsVisible
        {
            get
            {
                if (IsNgUser || IsSiteNgUser) return false;

                //TODO:ConnectedとかDisconnectedの場合、表示するエラーレベルがError以下の場合にfalseにしたい
                return true;
            }
        }
        public bool IsInitialComment { get; set; }
        public bool IsNameWrapping => _options.IsUserNameWrapping;
        public Guid SiteContextGuid { get; set; }
        protected MessageMetadataBase(ICommentOptions options, IMildomSiteOptions siteOptions)
        {
            _options = options;
            _siteOptions = siteOptions;
            options.PropertyChanged += Options_PropertyChanged;
            siteOptions.PropertyChanged += SiteOptions_PropertyChanged;
        }


        private void SiteOptions_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_siteOptions.NeedAutoSubNickname):
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
    internal class ConnectedMessageMetadata : MessageMetadataBase
    {
        public override Color BackColor => _options.BroadcastInfoBackColor;
        public override Color ForeColor => _options.BroadcastInfoForeColor;
        protected internal ConnectedMessageMetadata(IMildomConnected connected, ICommentOptions options, IMildomSiteOptions siteOptions)
            : base(options, siteOptions)
        {
        }
    }
    internal class DisconnectedMessageMetadata : MessageMetadataBase
    {
        public override Color BackColor => _options.BroadcastInfoBackColor;
        public override Color ForeColor => _options.BroadcastInfoForeColor;
        protected internal DisconnectedMessageMetadata(IMildomDisconnected disconnected, ICommentOptions options, IMildomSiteOptions siteOptions)
            : base(options, siteOptions)
        {
        }
    }
    internal class JoinMessageMetadata : MessageMetadataBase
    {
        public override Color BackColor => _options.BroadcastInfoBackColor;
        public override Color ForeColor => _options.BroadcastInfoForeColor;
        public JoinMessageMetadata(IMildomJoinRoom _, ICommentOptions options, IMildomSiteOptions siteOptions, IUser user, ICommentProvider cp)
            : base(options, siteOptions)
        {
            User = user;
            CommentProvider = cp;
        }
    }
    internal class CommentMessageMetadata : MessageMetadataBase
    {
        public override Color BackColor
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
                    var color = Common.Utils.ColorFromArgb(User.ForeColorArgb);
                    return color;
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
        public override bool IsNgUser => User != null ? User.IsNgUser : false;
        private bool _isFirstComment;
        public override bool IsFirstComment
        {
            get
            {
                return base.IsFirstComment;
            }
        }
        public CommentMessageMetadata(IMildomComment comment, ICommentOptions options, IMildomSiteOptions siteOptions, IUser user, ICommentProvider cp, bool isFirstComment)
            : base(options, siteOptions)
        {
            User = user;
            CommentProvider = cp;
            _isFirstComment = isFirstComment;

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
    //internal class MildomMessageMetadata : IMessageMetadata
    //{
    //    private readonly IMildomMessage _message;
    //    private readonly ICommentOptions _options;
    //    private readonly IMildomSiteOptions _siteOptions;
    //    private readonly bool _isFirstComment;

    //    public Color BackColor
    //    {
    //        get
    //        {
    //            if (User != null && !string.IsNullOrEmpty(User.BackColorArgb))
    //            {
    //                var color = Common.Utils.ColorFromArgb(User.BackColorArgb);
    //                return color;
    //            }
    //            else if (IsFirstComment)
    //            {
    //                return _options.FirstCommentBackColor;
    //            }
    //            else if (_message is IMildomConnected)
    //            {
    //                return _options.BroadcastInfoBackColor;
    //            }
    //            else if(_message is IMildomDisconnected)
    //            {
    //                return _options.BroadcastInfoBackColor;
    //            }
    //            else if(_message is IMildomJoinRoom)
    //            {
    //                return _options.BroadcastInfoBackColor;
    //            }
    //            else if (_message is IMildomItem item)
    //            {
    //                return _siteOptions.ItemBackColor;
    //            }
    //            else
    //            {
    //                return _options.BackColor;
    //            }
    //        }
    //    }

    //    public Color ForeColor
    //    {
    //        get
    //        {
    //            if (User != null && !string.IsNullOrEmpty(User.ForeColorArgb))
    //            {
    //                var color = Common.Utils.ColorFromArgb(User.ForeColorArgb);
    //                return color;
    //            }
    //            else if (IsFirstComment)
    //            {
    //                return _options.FirstCommentForeColor;
    //            }
    //            else if (_message is IMildomConnected)
    //            {
    //                return _options.BroadcastInfoForeColor;
    //            }
    //            else if (_message is IMildomDisconnected)
    //            {
    //                return _options.BroadcastInfoForeColor;
    //            }
    //            else if (_message is IMildomJoinRoom)
    //            {
    //                return _options.BroadcastInfoForeColor;
    //            }
    //            else if (_message is IMildomItem item)
    //            {
    //                return _siteOptions.ItemForeColor;
    //            }
    //            else
    //            {
    //                return _options.ForeColor;
    //            }
    //        }
    //    }

    //    public FontFamily FontFamily
    //    {
    //        get
    //        {
    //            if (_isFirstComment)
    //            {
    //                return _options.FirstCommentFontFamily;
    //            }
    //            else
    //            {
    //                return _options.FontFamily;
    //            }
    //        }
    //    }

    //    public int FontSize
    //    {
    //        get
    //        {
    //            if (_isFirstComment)
    //            {
    //                return _options.FirstCommentFontSize;
    //            }
    //            else
    //            {
    //                return _options.FontSize;
    //            }
    //        }
    //    }

    //    public FontWeight FontWeight
    //    {
    //        get
    //        {
    //            if (_isFirstComment)
    //            {
    //                return _options.FirstCommentFontWeight;
    //            }
    //            else
    //            {
    //                return _options.FontWeight;
    //            }
    //        }
    //    }

    //    public FontStyle FontStyle
    //    {
    //        get
    //        {
    //            if (_isFirstComment)
    //            {
    //                return _options.FirstCommentFontStyle;
    //            }
    //            else
    //            {
    //                return _options.FontStyle;
    //            }
    //        }
    //    }

    //    public bool IsNgUser => User != null ? User.IsNgUser : false;
    //    public bool IsSiteNgUser => false;//TODO:IUserにIsSiteNgUserを追加する
    //    public bool IsFirstComment { get; }
    //    public string SiteName { get; }
    //    public bool Is184 { get; }
    //    public IUser User { get; }
    //    public ICommentProvider CommentProvider { get; }
    //    public bool IsVisible
    //    {
    //        get
    //        {
    //            if (IsNgUser || IsSiteNgUser) return false;

    //            //TODO:ConnectedとかDisconnectedの場合、表示するエラーレベルがError以下の場合にfalseにしたい
    //            return true;
    //        }
    //    }
    //    public bool IsInitialComment { get; set; }
    //    public bool IsNameWrapping => _options.IsUserNameWrapping;
    //    public Guid SiteContextGuid { get; set; }
    //    public MildomMessageMetadata(IMildomMessage message, ICommentOptions options, IMildomSiteOptions siteOptions, IUser user, ICommentProvider cp, bool isFirstComment)
    //    {
    //        if(user == null)
    //        {

    //        }
    //        Debug.Assert(user != null);
    //        _message = message;
    //        _options = options;
    //        _siteOptions = siteOptions;
    //        User = user;
    //        CommentProvider = cp;
    //        _isFirstComment = isFirstComment;

    //        options.PropertyChanged += Options_PropertyChanged;
    //        siteOptions.PropertyChanged += SiteOptions_PropertyChanged;
    //        user.PropertyChanged += User_PropertyChanged;
    //    }

    //    private void User_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    //    {
    //        switch (e.PropertyName)
    //        {
    //            case nameof(User.IsNgUser):
    //                //case nameof(User.IsSiteNgUser):
    //                RaisePropertyChanged(nameof(IsVisible));
    //                break;
    //            case nameof(User.BackColorArgb):
    //                RaisePropertyChanged(nameof(BackColor));
    //                break;
    //            case nameof(User.ForeColorArgb):
    //                RaisePropertyChanged(nameof(ForeColor));
    //                break;
    //        }
    //    }

    //    private void SiteOptions_PropertyChanged(object sender, PropertyChangedEventArgs e)
    //    {
    //        switch (e.PropertyName)
    //        {
    //            case nameof(_siteOptions.NeedAutoSubNickname):
    //                RaisePropertyChanged(nameof(IsNameWrapping));
    //                break;
    //        }
    //    }

    //    private void Options_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    //    {
    //        switch (e.PropertyName)
    //        {
    //            case nameof(_options.BackColor):
    //                RaisePropertyChanged(nameof(BackColor));
    //                break;
    //            case nameof(_options.ForeColor):
    //                RaisePropertyChanged(nameof(ForeColor));
    //                break;
    //            case nameof(_options.FontFamily):
    //                RaisePropertyChanged(nameof(FontFamily));
    //                break;
    //            case nameof(_options.FontStyle):
    //                RaisePropertyChanged(nameof(FontStyle));
    //                break;
    //            case nameof(_options.FontWeight):
    //                RaisePropertyChanged(nameof(FontWeight));
    //                break;
    //            case nameof(_options.FontSize):
    //                RaisePropertyChanged(nameof(FontSize));
    //                break;
    //            case nameof(_options.FirstCommentFontFamily):
    //                RaisePropertyChanged(nameof(FontFamily));
    //                break;
    //            case nameof(_options.FirstCommentFontStyle):
    //                RaisePropertyChanged(nameof(FontStyle));
    //                break;
    //            case nameof(_options.FirstCommentFontWeight):
    //                RaisePropertyChanged(nameof(FontWeight));
    //                break;
    //            case nameof(_options.FirstCommentFontSize):
    //                RaisePropertyChanged(nameof(FontSize));
    //                break;
    //            case nameof(_options.IsUserNameWrapping):
    //                RaisePropertyChanged(nameof(IsNameWrapping));
    //                break;
    //        }
    //    }
    //    #region INotifyPropertyChanged
    //    [NonSerialized]
    //    private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
    //    {
    //        add { _propertyChanged += value; }
    //        remove { _propertyChanged -= value; }
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="propertyName"></param>
    //    protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    //    {
    //        _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    //    }
    //    #endregion
    //}
}
