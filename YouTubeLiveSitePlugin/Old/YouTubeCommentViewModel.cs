using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SitePlugin;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;
namespace YouTubeLiveSitePlugin.Old
{


    public class YouTubeCommentViewModel : IYouTubeCommentViewModel
    {
        public string ConnectionName { get { return _connectionName.Name; } }

        public IEnumerable<IMessagePart> NameItems { get; set; }

        public IEnumerable<IMessagePart> MessageItems { get; set; }

        public string Info { get; set; }

        public string Id { get; set; }

        public string Nickname { get; set; }

        public bool IsInfo { get; set; } = false;

        public bool IsFirstComment { get; set; } = false;

        public IEnumerable<IMessagePart> Thumbnail { get; set; }

        public FontFamily FontFamily { get { return _options.FontFamily; } }

        public FontStyle FontStyle { get { return _options.FontStyle; } }

        public FontWeight FontWeight { get { return _options.FontWeight; } }

        public int FontSize { get { return _options.FontSize; } }

        public SolidColorBrush Foreground { get { return new SolidColorBrush(_options.ForeColor); } }

        public SolidColorBrush Background { get { return new SolidColorBrush(_options.BackColor); } }

        public bool IsVisible { get; set; } = true;
        public IUser User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string UserId => throw new NotImplementedException();

        private readonly IOptions _options;
        private readonly YouTubeSiteOptions _siteOptions;
        private readonly ConnectionName _connectionName;
        public YouTubeCommentViewModel(ConnectionName connectionName, IOptions options, YouTubeSiteOptions siteOptions)
        {
            _connectionName = connectionName;
            _options = options;
            _siteOptions = siteOptions;
            WeakEventManager<ConnectionName, PropertyChangedEventArgs>.AddHandler(_connectionName, nameof(_connectionName.PropertyChanged), ConnectionName_PropertyChanged);
        }

        private void ConnectionName_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_connectionName.Name):
                    RaisePropertyChanged(nameof(ConnectionName));
                    break;
            }
        }

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
