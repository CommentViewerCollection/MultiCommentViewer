using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Mcv.MainViewPlugin;

class CommentDataGridViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ICommand TranslateCommand { get; }
    public ICollectionView Comments
    {
        get
        {
            return _comments;
        }
        //set
        //{
        //    _comments = value;
        //    RaisePropertyChanged();
        //}
    }
    public System.Windows.Controls.ScrollUnit ScrollUnit
    {
        get
        {
            if (_options.Options.IsPixelScrolling)
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
        get { return new SolidColorBrush(_options.Options.HorizontalGridLineColor); }
    }
    public Brush VerticalGridLineBrush
    {
        get { return new SolidColorBrush(_options.Options.VerticalGridLineColor); }
    }
    public Brush CommentListBackground => new SolidColorBrush(_options.Options.CommentListBackColor);
    public Brush CommentListBorderBrush => new SolidColorBrush(_options.Options.CommentListBorderColor);
    public double ConnectionNameWidth
    {
        get { return _options.Options.ConnectionNameWidth; }
        set { _options.Options.ConnectionNameWidth = value; }
    }
    public bool IsShowConnectionName
    {
        get { return _options.Options.IsShowConnectionName; }
        set { _options.Options.IsShowConnectionName = value; }
    }
    public int ConnectionNameDisplayIndex
    {
        get { return _options.Options.ConnectionNameDisplayIndex; }
        set { _options.Options.ConnectionNameDisplayIndex = value; }
    }
    public double ThumbnailWidth
    {
        get { return _options.Options.ThumbnailWidth; }
        set { _options.Options.ThumbnailWidth = value; }
    }
    public virtual bool IsShowThumbnail
    {
        get { return _options.Options.IsShowThumbnail; }
        set { _options.Options.IsShowThumbnail = value; }
    }
    public int ThumbnailDisplayIndex
    {
        get { return _options.Options.ThumbnailDisplayIndex; }
        set { _options.Options.ThumbnailDisplayIndex = value; }
    }
    public double CommentIdWidth
    {
        get { return _options.Options.CommentIdWidth; }
        set { _options.Options.CommentIdWidth = value; }
    }
    public bool IsShowCommentId
    {
        get { return _options.Options.IsShowCommentId; }
        set { _options.Options.IsShowCommentId = value; }
    }
    public int CommentIdDisplayIndex
    {
        get { return _options.Options.CommentIdDisplayIndex; }
        set { _options.Options.CommentIdDisplayIndex = value; }
    }
    public double UsernameWidth
    {
        get { return _options.Options.UsernameWidth; }
        set { _options.Options.UsernameWidth = value; }
    }
    public virtual bool IsShowUsername
    {
        get { return _options.Options.IsShowUsername; }
        set { _options.Options.IsShowUsername = value; }
    }
    public int UsernameDisplayIndex
    {
        get { return _options.Options.UsernameDisplayIndex; }
        set { _options.Options.UsernameDisplayIndex = value; }
    }

    public double MessageWidth
    {
        get { return _options.Options.MessageWidth; }
        set { _options.Options.MessageWidth = value; }
    }
    public bool IsShowMessage
    {
        get { return _options.Options.IsShowMessage; }
        set { _options.Options.IsShowMessage = value; }
    }
    public int MessageDisplayIndex
    {
        get { return _options.Options.MessageDisplayIndex; }
        set { _options.Options.MessageDisplayIndex = value; }
    }
    #region PostTime
    public double PostTimeWidth
    {
        get { return _options.Options.PostTimeWidth; }
        set { _options.Options.PostTimeWidth = value; }
    }
    public bool IsShowPostTime
    {
        get { return _options.Options.IsShowPostTime; }
        set { _options.Options.IsShowPostTime = value; }
    }
    public int PostTimeDisplayIndex
    {
        get { return _options.Options.PostTimeDisplayIndex; }
        set { _options.Options.PostTimeDisplayIndex = value; }
    }
    #endregion

    public double InfoWidth
    {
        get { return _options.Options.InfoWidth; }
        set { _options.Options.InfoWidth = value; }
    }
    public bool IsShowInfo
    {
        get { return _options.Options.IsShowInfo; }
        set { _options.Options.IsShowInfo = value; }
    }
    public int InfoDisplayIndex
    {
        get { return _options.Options.InfoDisplayIndex; }
        set { _options.Options.InfoDisplayIndex = value; }
    }
    public Color SelectedRowBackColor
    {
        get { return _options.Options.SelectedRowBackColor; }
        set { _options.Options.SelectedRowBackColor = value; }
    }
    public Color SelectedRowForeColor
    {
        get { return _options.Options.SelectedRowForeColor; }
        set { _options.Options.SelectedRowForeColor = value; }
    }
    public IMcvCommentViewModel? SelectedComment { get; set; }
    protected readonly IAdapter _options;
    private readonly ICollectionView _comments;
    //private readonly ObservableCollection<IMcvCommentViewModel> _commentsOriginal;
    protected readonly Dispatcher _dispatcher;

    private void ShowUserInfo()
    {

        var current = SelectedComment;
        if (current is null) return;
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            Debug.WriteLine("UserIdがnull");
            return;
        }
        var userVm = new UserViewModel();
        var view = new CollectionViewSource { Source = _comments.SourceCollection }.View;
        //var view = _comments;
        view.Filter = obj =>
        {
            if (!(obj is IMcvCommentViewModel cvm))
            {
                return false;
            }
            return cvm.UserId == userId;
        };
        var userInfoVm = new UserInfoViewModel(view, userVm, _options);
        WeakReferenceMessenger.Default.Send(new ShowUserInfoViewMessage(userInfoVm));
    }
    public ICommand ShowUserInfoCommand { get; }
    //public CommentDataGridViewModel(IAdapter options, ICollectionView comments)
    //{
    //    _options = options;
    //    _comments = comments;
    //    _dispatcher = Dispatcher.CurrentDispatcher;
    //    TranslateCommand = new RelayCommand(Translate);
    //    ShowUserInfoCommand = new RelayCommand(ShowUserInfo);

    //}

    public CommentDataGridViewModel(IAdapter options, ICollectionView comments)
    {
        _options = options;
        //_comments = new CollectionViewSource { Source = comments }.View;
        //_commentsOriginal = comments;
        _comments = comments;
        _dispatcher = Dispatcher.CurrentDispatcher;
        TranslateCommand = new RelayCommand(Translate);
        ShowUserInfoCommand = new RelayCommand(ShowUserInfo);
    }
    private void Translate()
    {

    }
}