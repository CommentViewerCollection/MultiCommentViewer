using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SitePlugin;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using GalaSoft.MvvmLight;

namespace MultiCommentViewer.Test
{
    //TODO:テストを書く ex:OptionsのFontFamilyが変更になった時にPropertyChangedをしっかり受け取って、自信も発信できるか
    //TODO:IUserを作って、ユーザ毎のコメントを見られるようにする

    public class TestSiteCommentViewModel : ViewModelBase, ICommentViewModel
    {
        private readonly ConnectionName _connectionName;
        private readonly TestSiteOptions _siteOptions;
        private readonly IOptions _options;
        public TestSiteCommentViewModel(ConnectionName connectionName, IEnumerable<IMessagePart> name, IEnumerable<IMessagePart> message, IOptions options, TestSiteOptions siteOptions)
        {
            _connectionName = connectionName;
            NameItems = name;
            MessageItems = message;
            _options = options;
            _siteOptions = siteOptions;
            connectionName.PropertyChanged += ConnectionName_PropertyChanged;
        }

        private void ConnectionName_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SitePlugin.ConnectionName.Name):
                    base.RaisePropertyChanged(nameof(ConnectionName));
                    break;
            }
        }

        public string ConnectionName => _connectionName.Name;

        public IEnumerable<IMessagePart> NameItems { get; }

        public IEnumerable<IMessagePart> MessageItems { get; }

        public string Info { get; set; }

        public string Id { get; set; }

        public string Nickname { get; set; }

        public bool IsInfo { get { return false; } }

        public bool IsFirstComment { get; set; }

        public IEnumerable<IMessagePart> Thumbnail => new List<IMessagePart>();

        public FontFamily FontFamily => _options.FontFamily;

        public FontStyle FontStyle => _options.FontStyle;

        public FontWeight FontWeight => _options.FontWeight;

        public int FontSize => _options.FontSize;
        public bool IsVisible { get; set; } = true;

        public SolidColorBrush Foreground => new SolidColorBrush(_options.ForeColor);

        public SolidColorBrush Background => new SolidColorBrush(_options.BackColor);

        public IUser User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string UserId => throw new NotImplementedException();

        public Task AfterCommentAdded()
        {
            throw new NotImplementedException();
        }
    }
}
