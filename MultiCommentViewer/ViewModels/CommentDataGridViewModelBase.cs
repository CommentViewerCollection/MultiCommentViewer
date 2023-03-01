using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using SitePlugin;
using System.Threading;
using System.Collections.ObjectModel;
using Plugin;
using ryu_s.BrowserCookie;
using System.Diagnostics;
using System.Windows.Threading;
using System.Net;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Data;
using Common;

namespace MultiCommentViewer
{
    /// <summary>
    /// MainViewModelとUserViewModelの共通項
    /// </summary>
    public abstract class CommentDataGridViewModelBase : ObservableObject
    {
        public ICommand TranslateCommand { get; }
        public ICollectionView Comments
        {
            get
            {
                return _comments;
            }
            set
            {
                _comments = value;
                OnPropertyChanged();
            }
        }
        public System.Windows.Controls.ScrollUnit ScrollUnit
        {
            get
            {
                if (_options.IsPixelScrolling)
                {
                    return System.Windows.Controls.ScrollUnit.Pixel;
                }
                else
                {
                    return System.Windows.Controls.ScrollUnit.Item;
                }
            }
        }
        public Brush HorizontalGridLineBrush
        {
            get { return new SolidColorBrush(_options.HorizontalGridLineColor); }
        }
        public Brush VerticalGridLineBrush
        {
            get { return new SolidColorBrush(_options.VerticalGridLineColor); }
        }
        public double ConnectionNameWidth
        {
            get { return _options.ConnectionNameWidth; }
            set { _options.ConnectionNameWidth = value; }
        }
        public bool IsShowConnectionName
        {
            get { return _options.IsShowConnectionName; }
            set { _options.IsShowConnectionName = value; }
        }
        public int ConnectionNameDisplayIndex
        {
            get { return _options.ConnectionNameDisplayIndex; }
            set { _options.ConnectionNameDisplayIndex = value; }
        }
        public double ThumbnailWidth
        {
            get { return _options.ThumbnailWidth; }
            set { _options.ThumbnailWidth = value; }
        }
        public virtual bool IsShowThumbnail
        {
            get { return _options.IsShowThumbnail; }
            set { _options.IsShowThumbnail = value; }
        }
        public int ThumbnailDisplayIndex
        {
            get { return _options.ThumbnailDisplayIndex; }
            set { _options.ThumbnailDisplayIndex = value; }
        }
        public double CommentIdWidth
        {
            get { return _options.CommentIdWidth; }
            set { _options.CommentIdWidth = value; }
        }
        public bool IsShowCommentId
        {
            get { return _options.IsShowCommentId; }
            set { _options.IsShowCommentId = value; }
        }
        public int CommentIdDisplayIndex
        {
            get { return _options.CommentIdDisplayIndex; }
            set { _options.CommentIdDisplayIndex = value; }
        }
        public double UsernameWidth
        {
            get { return _options.UsernameWidth; }
            set { _options.UsernameWidth = value; }
        }
        public virtual bool IsShowUsername
        {
            get { return _options.IsShowUsername; }
            set { _options.IsShowUsername = value; }
        }
        public int UsernameDisplayIndex
        {
            get { return _options.UsernameDisplayIndex; }
            set { _options.UsernameDisplayIndex = value; }
        }

        public double MessageWidth
        {
            get { return _options.MessageWidth; }
            set { _options.MessageWidth = value; }
        }
        public bool IsShowMessage
        {
            get { return _options.IsShowMessage; }
            set { _options.IsShowMessage = value; }
        }
        public int MessageDisplayIndex
        {
            get { return _options.MessageDisplayIndex; }
            set { _options.MessageDisplayIndex = value; }
        }
        #region PostTime
        public double PostTimeWidth
        {
            get { return _options.PostTimeWidth; }
            set { _options.PostTimeWidth = value; }
        }
        public bool IsShowPostTime
        {
            get { return _options.IsShowPostTime; }
            set { _options.IsShowPostTime = value; }
        }
        public int PostTimeDisplayIndex
        {
            get { return _options.PostTimeDisplayIndex; }
            set { _options.PostTimeDisplayIndex = value; }
        }
        #endregion

        public double InfoWidth
        {
            get { return _options.InfoWidth; }
            set { _options.InfoWidth = value; }
        }
        public bool IsShowInfo
        {
            get { return _options.IsShowInfo; }
            set { _options.IsShowInfo = value; }
        }
        public int InfoDisplayIndex
        {
            get { return _options.InfoDisplayIndex; }
            set { _options.InfoDisplayIndex = value; }
        }
        public Color SelectedRowBackColor
        {
            get { return _options.SelectedRowBackColor; }
            set { _options.SelectedRowBackColor = value; }
        }
        public Color SelectedRowForeColor
        {
            get { return _options.SelectedRowForeColor; }
            set { _options.SelectedRowForeColor = value; }
        }
        public IMcvCommentViewModel SelectedComment { get; set; }
        protected readonly IOptions _options;
        private ICollectionView _comments;
        protected readonly Dispatcher _dispatcher;
        public CommentDataGridViewModelBase(IOptions options)
        {
            _options = options;
            Comments = CollectionViewSource.GetDefaultView(new ObservableCollection<ICommentViewModel>());
            _dispatcher = Dispatcher.CurrentDispatcher;
            TranslateCommand = new RelayCommand(Translate);
        }
        public CommentDataGridViewModelBase(IOptions options, ICollectionView comments)
        {
            _options = options;
            Comments = comments;
            _dispatcher = Dispatcher.CurrentDispatcher;
            TranslateCommand = new RelayCommand(Translate);
        }
        TexTra.TexTraTranslator translator = new TexTra.TexTraTranslator();
        private async void Translate()
        {
            var selectedComment = SelectedComment;
            if (selectedComment.IsTranslated)
            {
                return;
            }
            var text = selectedComment.MessageItems.ToText();
            try
            {
                var a = await translator.Traslate(text, "rysestock", "1f9ea34966481b37424e659bcdfac50c05f770d82", "62f016b234031ca2cef2f904b9edcb6d");
                if (!a.IsError)
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        var list = new List<IMessagePart>(selectedComment.MessageItems)
                        {
                        Common.MessagePartFactory.CreateMessageText(Environment.NewLine + "(訳)" + a.Translated)
                        };
                        selectedComment.MessageItems = list;
                    });
                    selectedComment.IsTranslated = true;
                }
                else
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        var list = new List<IMessagePart>(selectedComment.MessageItems)
                        {
                        Common.MessagePartFactory.CreateMessageText(Environment.NewLine + a.ErrorMessage)
                        };
                        selectedComment.MessageItems = list;
                    });
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await _dispatcher.InvokeAsync(() =>
                    {
                        var list = new List<IMessagePart>(selectedComment.MessageItems)
                            {
                        Common.MessagePartFactory.CreateMessageText(Environment.NewLine + ex.Message)
                            };
                        selectedComment.MessageItems = list;
                    });
                }
                catch { }
            }
        }
    }
}
