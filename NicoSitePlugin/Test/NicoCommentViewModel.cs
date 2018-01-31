using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SitePlugin;
namespace NicoSitePlugin.Test
{
    public class NicoCommentViewModel : INicoCommentViewModel
    {
        public string ConnectionName => _connectionName.Name;

        public IEnumerable<IMessagePart> NameItems { get; set; }

        public IEnumerable<IMessagePart> MessageItems { get; set; }

        public string Info { get; set; }

        public string Id { get; set; }

        public string Nickname { get; set; }

        public bool IsInfo { get; set; }

        public bool IsFirstComment { get; set; }

        public IEnumerable<IMessagePart> Thumbnail => new List<IMessagePart>();

        public FontFamily FontFamily => _options.FontFamily;

        public FontStyle FontStyle => _options.FontStyle;

        public FontWeight FontWeight => _options.FontWeight;

        public int FontSize => _options.FontSize;

        public bool IsVisible { get; set; } = true;

        public SolidColorBrush Foreground => new SolidColorBrush(_options.ForeColor);

        public SolidColorBrush Background => new SolidColorBrush(_options.BackColor);

        
        
        
        public Task AfterCommentAdded()
        {
            throw new NotImplementedException();
        }

        private readonly ConnectionName _connectionName;
        private readonly chat _chat;
        private readonly IOptions _options;
        private readonly NicoSiteOptions _siteOptions;
        internal NicoCommentViewModel(ConnectionName connectionName, chat chat, IOptions options, NicoSiteOptions siteOptions)
        {
            _connectionName = connectionName;
            _chat = chat;
            _options = options;
            _siteOptions = siteOptions;
            NameItems = new List<IMessagePart> { new MessageText { Text = chat.user_id } };
            MessageItems = new List<IMessagePart> { new MessageText { Text = chat.text } };
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
    class MessageText : IMessageText
    {
        public string Text { get; set; }
    }
}
