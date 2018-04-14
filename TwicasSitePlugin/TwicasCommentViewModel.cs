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
        public string PostTime { get; }
        public override string UserId { get; }
        public TwicasCommentViewModel(ICommentOptions options, ICommentData data, IUser user) :
            base(options)
        {
            UserId = data.UserId;
            Id = data.Id.ToString();
            NameItems = new List<IMessagePart> { new MessageText(data.Name) };
            MessageItems = data.Message;
            Thumbnail = new MessageImage { Url = data.ThumbnailUrl, Height = data.ThumbnailHeight, Width = data.ThumbnailWidth };
            //User = user;
            PostTime = data.Date.ToString("HH:mm:ss");
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
