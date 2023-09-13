using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Mcv.MainViewPlugin;

/// <summary>
/// 
/// </summary>
class CommentDataGridViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ICommand CommentCopyCommand { get; }
    public ICommand OpenUrlCommand { get; }
    public ICommand TranslateCommand { get; }
    public ICollectionView Comments
    {
        get
        {
            return _comments;
        }
        protected set
        {
            _comments = value;
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
    public Brush CommentListBackground => new SolidColorBrush(_options.CommentListBackColor);
    public Brush CommentListBorderBrush => new SolidColorBrush(_options.CommentListBorderColor);
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
    public IMcvCommentViewModel? SelectedComment { get; set; }
    protected readonly IMainViewPluginOptions _options;
    private ICollectionView _comments;
    //private readonly ObservableCollection<IMcvCommentViewModel> _commentsOriginal;
    protected readonly Dispatcher _dispatcher;

    private void ShowUserInfo()
    {

        var current = SelectedComment;
        if (current is null) return;
        var userId = current.UserId;
        if (string.IsNullOrEmpty(userId) || current.User is null)
        {
            Debug.WriteLine("UserIdがnull");
            return;
        }
        var userVm = new UserViewModel(current.User, _options);
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
    public CommentDataGridViewModel()
    {
        if (!DesignModeUtils.IsDesignMode)
        {
            throw new NotSupportedException();
        }
    }
    public CommentDataGridViewModel(IMainViewPluginOptions options, ICollectionView comments)
    {
        _options = options;
        //_comments = new CollectionViewSource { Source = comments }.View;
        //_commentsOriginal = comments;
        _comments = comments;
        _dispatcher = Dispatcher.CurrentDispatcher;
        CommentCopyCommand = new RelayCommand(CopySelectedComment);
        OpenUrlCommand = new RelayCommand(OpenUrl);
        TranslateCommand = new RelayCommand(Translate);
        ShowUserInfoCommand = new RelayCommand(ShowUserInfo);
        options.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(options.IsShowThumbnail):
                    RaisePropertyChanged(nameof(IsShowThumbnail));
                    break;
                case nameof(options.IsShowUsername):
                    RaisePropertyChanged(nameof(IsShowUsername));
                    break;
                case nameof(options.IsShowConnectionName):
                    RaisePropertyChanged(nameof(IsShowConnectionName));
                    break;
                case nameof(options.IsShowCommentId):
                    RaisePropertyChanged(nameof(IsShowCommentId));
                    break;
                case nameof(options.IsShowMessage):
                    RaisePropertyChanged(nameof(IsShowMessage));
                    break;
                case nameof(options.IsShowPostTime):
                    RaisePropertyChanged(nameof(IsShowPostTime));
                    break;
                case nameof(options.IsShowInfo):
                    RaisePropertyChanged(nameof(IsShowInfo));
                    break;
            }
        };
    }
    private void CopySelectedComment()
    {
        var selected = SelectedComment;
        if (selected is null)
        {
            return;
        }

    }
    public bool ContainsUrl
    {
        get
        {
            var ret = false;
            try
            {
                var selected = SelectedComment;
                if (selected is null)
                {
                    return false;
                }
            }
            catch (Exception)
            {

            }
            return ret;
        }
    }
    private void OpenUrl()
    {

    }
    private void Translate()
    {

    }
}
