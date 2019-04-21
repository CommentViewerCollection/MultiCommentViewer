using SitePlugin;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace TestSitePlugin
{
    class TestMetadata : IMessageMetadata
    {
        public Color BackColor { get; } = Colors.White;
        public Color ForeColor { get; } = Colors.Black;
        public FontFamily FontFamily { get; } = new FontFamily("Meiryo");
        public int FontSize { get; } = 14;
        public FontWeight FontWeight { get; } = FontWeights.Normal;
        public FontStyle FontStyle { get; } = FontStyles.Normal;
        public bool IsNgUser { get; } = false;
        public bool IsSiteNgUser { get; } = false;
        public bool IsFirstComment { get; } = false;
        public bool IsInitialComment { get; } = false;
        public bool Is184 { get; } = false;
        public IUser User { get; }
        public ICommentProvider CommentProvider { get; }
        public bool IsVisible { get; } = true;
        public bool IsNameWrapping { get; } = false;
        public Guid SiteContextGuid { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public TestMetadata(IUser user)
        {
            User = user;
        }
    }
}
