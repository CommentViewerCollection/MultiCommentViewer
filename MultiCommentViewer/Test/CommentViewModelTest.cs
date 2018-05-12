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
        private readonly TestSiteOptions _siteOptions;
        private readonly ICommentOptions _options;
        public TestSiteCommentViewModel(IEnumerable<IMessagePart> name, IEnumerable<IMessagePart> message, ICommentOptions options, TestSiteOptions siteOptions)
        {
            NameItems = name;
            MessageItems = message;
            _options = options;
            _siteOptions = siteOptions;
        }

        public IEnumerable<IMessagePart> NameItems { get; }

        public IEnumerable<IMessagePart> MessageItems { get; }

        public string Info { get; set; }

        public string Id { get; set; }

        public string Nickname { get; set; }

        public bool IsInfo { get { return false; } }

        public bool IsFirstComment { get; set; }

        public string PostTime { get; set; }

        public IMessageImage Thumbnail => null;

        public FontFamily FontFamily => _options.FontFamily;

        public FontStyle FontStyle => _options.FontStyle;

        public FontWeight FontWeight => _options.FontWeight;

        public int FontSize => _options.FontSize;
        public bool IsVisible { get; set; } = true;

        public SolidColorBrush Foreground => new SolidColorBrush(_options.ForeColor);

        public SolidColorBrush Background => new SolidColorBrush(_options.BackColor);

        public IUser User { get; set; }

        public string UserId => "abc";

        public ICommentProvider CommentProvider
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Task AfterCommentAdded()
        {
            throw new NotImplementedException();
        }
    }
}
