using Common;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SitePluginCommon
{
    public interface IInfoMessage : ISiteMessage
    {
        InfoType Type { get; set; }
        string Text { get; }
        DateTime CreatedAt { get; }
    }
    public class InfoMessage : IInfoMessage
    {
        public InfoType Type { get; set; }
        public string Raw { get; }
        public SiteType SiteType { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; } = DateTime.Now;

        public event EventHandler<ValueChangedEventArgs> ValueChanged;
    }
    public class InfoMessageMetadata : IMessageMetadata
    {
        private readonly IInfoMessage _infoMessage;
        private readonly ICommentOptions _options;

        public Color BackColor => _options.InfoBackColor;
        public Color ForeColor => _options.InfoForeColor;
        public FontFamily FontFamily => _options.FontFamily;
        public int FontSize => _options.FontSize;
        public FontWeight FontWeight => _options.FontWeight;
        public FontStyle FontStyle => _options.FontStyle;
        public bool IsNgUser => false;
        public bool IsSiteNgUser => false;
        public bool IsFirstComment => false;
        public bool IsInitialComment => false;
        public bool Is184 => false;
        public IUser User => null;
        public ICommentProvider CommentProvider => null;
        public bool IsVisible
        {
            get
            {
                return true;
            }
        }
        public bool IsNameWrapping => false;
        public Guid SiteContextGuid { get; set; }
        public ISiteOptions SiteOptions { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public InfoMessageMetadata(IInfoMessage infoMessage, ICommentOptions options)
        {
            _infoMessage = infoMessage;
            _options = options;
        }
    }
    public class InfoMessageMethods : IMessageMethods
    {
        public Task AfterCommentAdded()
        {
            return Task.CompletedTask;
        }
    }
    public class InfoMessageContext : IMessageContext
    {
        public SitePlugin.ISiteMessage Message { get; }

        public IMessageMetadata Metadata { get; }

        public IMessageMethods Methods { get; }
        public InfoMessageContext(IInfoMessage message, InfoMessageMetadata metadata, InfoMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
        public static InfoMessageContext Create(InfoMessage message, ICommentOptions options)
        {
            var metadata = new InfoMessageMetadata(message, options);
            var methods = new InfoMessageMethods();
            var context = new InfoMessageContext(message, metadata, methods);
            return context;

        }
    }
}
