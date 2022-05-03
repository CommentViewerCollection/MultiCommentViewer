using System;
using System.ComponentModel;

namespace TwicasSitePlugin
{
    //class TwicasCommentViewModel : CommentViewModelBase
    //{
    //    public override MessageType MessageType { get; protected set; }
    //    public override string UserId { get; }
    //    public override SolidColorBrush Background
    //    {
    //        get
    //        {
    //            if (!string.IsNullOrEmpty(User.BackColorArgb))
    //            {
    //                return new SolidColorBrush(Tools.ColorFromArgb(User.BackColorArgb));
    //            }
    //            else
    //            {
    //                return base.Background;
    //            }
    //        }
    //    }
    //    public override SolidColorBrush Foreground
    //    {
    //        get
    //        {
    //            if (!string.IsNullOrEmpty(User.ForeColorArgb))
    //            {
    //                return new SolidColorBrush(Tools.ColorFromArgb(User.ForeColorArgb));
    //            }
    //            else
    //            {
    //                return base.Foreground;
    //            }
    //        }
    //    }
    //    public TwicasCommentViewModel(ICommentOptions options,ITwicasSiteOptions siteOptions, ICommentData data, IUser user, ICommentProvider commentProvider)
    //        : base(options, user, commentProvider, false)
    //    {
    //        MessageType = MessageType.Comment;
    //        var messageText = data.Message.ToText();
    //        UserId = data.UserId;
    //        Id = data.Id.ToString();
    //        NameItemsInternal = new List<IMessagePart> { MessagePartFactory.CreateMessageText(data.Name) };
    //        MessageItems = data.Message;
    //        Thumbnail = new MessageImage { Url = data.ThumbnailUrl, Height = data.ThumbnailHeight, Width = data.ThumbnailWidth };

    //        if (siteOptions.IsAutoSetNickname)
    //        {
    //            var nick = ExtractNickname(messageText);
    //            if (!string.IsNullOrEmpty(nick))
    //            {
    //                user.Nickname = nick;
    //            }
    //        }
    //        User.Name = NameItemsInternal;

    //        PostTime = data.Date.ToString("HH:mm:ss");

    //        Init();
    //    }
    //}
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
