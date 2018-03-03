using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.Windows.Input;
using YouTubeLiveSitePlugin;
using SitePlugin;
using Plugin;
using YouTubeLiveSitePlugin.Test2;
using Common;
using System.Windows.Threading;
using System;
using System.Diagnostics;
using System.ComponentModel;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Data;

namespace YouTubeLiveCommentViewer.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Commands
        public ICommand ActivatedCommand { get; }
        public ICommand LoadedCommand { get; }
        public ICommand MainViewContentRenderedCommand { get; }
        public ICommand MainViewClosingCommand { get; }
        public ICommand ShowOptionsWindowCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ShowWebSiteCommand { get; }
        public ICommand ShowDevelopersTwitterCommand { get; }
        public ICommand CheckUpdateCommand { get; }
        public ICommand ShowUserInfoCommand { get; }
        public ICommand RemoveSelectedConnectionCommand { get; }
        public ICommand AddNewConnectionCommand { get; }
        public ICommand ClearAllCommentsCommand { get; }
        #endregion //Commands

        #region Fields
        private readonly Dispatcher _dispatcher;
        private readonly ILogger _logger;
        private readonly Dictionary<IPlugin, PluginMenuItemViewModel> _pluginMenuItemDict = new Dictionary<IPlugin, PluginMenuItemViewModel>();
        private IPluginManager _pluginManager;
        private readonly IIo _io;
        public ObservableCollection<BrowserViewModel> BrowserCollection { get; } = new ObservableCollection<BrowserViewModel>();
        private readonly IBrowserLoader _browserLoader;
        private bool _isAddingNewCommentTop;

        private BrowserViewModel _selectedBrowserViewModel;
        public BrowserViewModel SelectedBrowserViewModel
        {
            get { return _selectedBrowserViewModel; }
            set
            {
                if (_selectedBrowserViewModel == value) return;
                _selectedBrowserViewModel = value;
                RaisePropertyChanged();
            }
        }
        #endregion
        public bool Topmost
        {
            get { return _options.IsTopmost; }
            set { _options.IsTopmost = value; }
        }
        public double Height
        {
            get { return _options.MainViewHeight; }
            set { _options.MainViewHeight = value; }
        }
        public double Width
        {
            get { return _options.MainViewWidth; }
            set { _options.MainViewWidth = value; }
        }
        public double Left
        {
            get { return _options.MainViewLeft; }
            set { _options.MainViewLeft = value; }
        }
        public double Top
        {
            get { return _options.MainViewTop; }
            set { _options.MainViewTop = value; }
        }
        public bool IsShowLiveInfo
        {
            get { return true; }
            set
            {
                //TODO:
            }
        }
        string Name
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var title = asm.GetName().Name;
                return title;
            }
        }
        string Fullname
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var ver = asm.GetName().Version;
                var s = $"{Name} v{ver.Major}.{ver.Minor}.{ver.Build}";
                return s;
            }
        }
        public string Title
        {
            get
            {
                var s = Fullname;
#if DEBUG
                s += " (DEBUG)";
#endif
                return s;
            }
        }
        #region Thumbnail
        public int ThumbnailDisplayIndex
        {
            get { return _options.ThumbnailDisplayIndex; }
            set { _options.ThumbnailDisplayIndex = value; }
        }
        public double ThumbnailWidth
        {
            get { return _options.ThumbnailWidth; }
            set { _options.ThumbnailWidth = value; }
        }
        public bool IsShowThumbnail
        {
            get { return _options.IsShowThumbnail; }
            set { _options.IsShowThumbnail = value; }
        }
        #endregion

        #region Username
        public int UsernameDisplayIndex
        {
            get { return _options.UsernameDisplayIndex; }
            set { _options.UsernameDisplayIndex = value; }
        }
        public double UsernameWidth
        {
            get { return _options.UsernameWidth; }
            set { _options.UsernameWidth = value; }
        }
        public bool IsShowUsername
        {
            get { return _options.IsShowUsername; }
            set { _options.IsShowUsername = value; }
        }
        #endregion

        #region UserId
        public int UserIdDisplayIndex
        {
            get { return _options.UserIdDisplayIndex; }
            set { _options.UserIdDisplayIndex = value; }
        }
        public double UserIdWidth
        {
            get { return _options.UserIdWidth; }
            set { _options.UserIdWidth = value; }
        }
        public bool IsShowUserId
        {
            get { return _options.IsShowUserId; }
            set { _options.IsShowUserId = value; }
        }
        #endregion

        #region PostTime
        public int PostTimeDisplayIndex
        {
            get { return _options.PostTimeDisplayIndex; }
            set { _options.PostTimeDisplayIndex = value; }
        }
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
        #endregion

        #region Message
        public int MessageDisplayIndex
        {
            get { return _options.MessageDisplayIndex; }
            set { _options.MessageDisplayIndex = value; }
        }
        public double MessageWidth
        {
            get { return _options.MessageWidth; }
            set { _options.MessageWidth = value; }
        }
        #endregion

        //public bool IsShowCommentId
        //{
        //    get { return _options.IsShowCommentId; }
        //    set { _options.IsShowCommentId = value; }
        //}

        public Color SelectedRowBackColor
        {
            get { return _options.SelectedRowBackColor; }
        }
        public Color SelectedRowForeColor
        {
            get { return _options.SelectedRowForeColor; }
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
        public IValueConverter ThumbnailConverter { get; } = new Common.Wpf.ThumbnailConverter();

        private string GetSiteOptionsPath(ISiteContext site)
        {
            var path = System.IO.Path.Combine(_options.SettingsDirPath, site.DisplayName + ".txt");
            return path;
        }
        private async void ContentRendered()
        {
            //なんか気持ち悪い書き方だけど一応動く。
            //ここでawaitするとそれ以降が実行されないからこうするしかない。
            try
            {
                MessengerInstance.Send(new SetPostCommentPanel(_siteContext.GetCommentPostPanel(_commentProvider)));
                MessengerInstance.Send(new SetAddingCommentDirection { IsTop = _options.IsAddingNewCommentTop });
                _isAddingNewCommentTop = _options.IsAddingNewCommentTop;

                var siteOptionsPath = GetSiteOptionsPath(_siteContext);
                _siteContext.LoadOptions(siteOptionsPath, _io);

                var browsers = _browserLoader.LoadBrowsers().Select(b => new BrowserViewModel(b));
                //もしブラウザが無かったらclass EmptyBrowserProfileを使う。
                if (browsers.Count() == 0)
                {
                    browsers = new List<BrowserViewModel>
                    {
                        new BrowserViewModel( new EmptyBrowserProfile()),
                    };
                }
                foreach (var browser in browsers)
                {
                    BrowserCollection.Add(browser);
                }
                SelectedBrowserViewModel = BrowserCollection[0];

                _pluginManager = new PluginManager(_options);
                _pluginManager.PluginAdded += PluginManager_PluginAdded;
                _pluginManager.LoadPlugins(new PluginHost(this, _options, _io));

                _pluginManager.OnLoaded();

                if (_options.IsAutoCheckIfUpdateExists)
                {
                    await CheckIfUpdateExists(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Debug.WriteLine(ex.Message);
            }
        }
        bool _canClose = false;
        private async void Closing(CancelEventArgs e)
        {
            e.Cancel = !_canClose;
            if (_canClose)
                return;
            try
            {
                var path = GetSiteOptionsPath(_siteContext);
                _siteContext.SaveOptions(path, _io);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Debug.WriteLine(ex.Message);
            }
            _pluginManager?.OnClosing();

            _canClose = true;
            App.Current.Shutdown();
        }
        void SetInfo(string message)
        {
            var cvm = new InfoCommentViewModel(_options, message);
            AddComment(cvm);
        }
        private async Task CheckIfUpdateExists(bool isAutoCheck)
        {
            //新しいバージョンがあるか確認
            Common.AutoUpdate.LatestVersionInfo latestVersionInfo;
            try
            {
                latestVersionInfo = await Common.AutoUpdate.Tools.GetLatestVersionInfo(Name);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                if (!isAutoCheck)
                {
                    SetInfo("サーバに障害が発生している可能性があります。しばらく経ってから再度試してみて下さい。");
                }
                return;
            }

            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var myVer = asm.GetName().Version;
            if (myVer < latestVersionInfo.Version)
            {
                //新しいバージョンがあった
                MessengerInstance.Send(new Common.AutoUpdate.ShowUpdateDialogMessage(true, myVer, latestVersionInfo, _logger));
            }
            else
            {
                //自動チェックの時は、アップデートが無ければ何も表示しない
                if (!isAutoCheck)
                {
                    //アップデートはありません
                    MessengerInstance.Send(new Common.AutoUpdate.ShowUpdateDialogMessage(false, myVer, latestVersionInfo, _logger));
                }
            }
        }
        private async void CheckUpdate()
        {
            try
            {
                await CheckIfUpdateExists(false);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private void ShowDevelopersTwitter()
        {
            try
            {
                System.Diagnostics.Process.Start("https://twitter.com/kv510k");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private void ShowWebSite()
        {
            try
            {
                System.Diagnostics.Process.Start("https://ryu-s.github.io/app/youtubelivecommentviewer");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private void ShowOptionsWindow()
        {
            try
            {
                var list = new List<IOptionsTabPage>();
                var mainOptionsPanel = new MainOptionsPanel();
                mainOptionsPanel.SetViewModel(new MainOptionsViewModel(_options));
                list.Add(new MainTabPage("一般", mainOptionsPanel));
                list.Add(_siteContext.TabPanel);
                MessengerInstance.Send(new ShowOptionsViewMessage(list));
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private void Close()
        {
            MessengerInstance.Send(new MainViewCloseMessage());
        }
        private readonly IYouTubeSiteContext _siteContext;

        public MainViewModel()
        {

        }
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }

        #region CanConnect
        public bool CanConnect
        {
            get { return IsValidInput && _commentProvider.CanConnect; }
        }
        #endregion //CanConnect

        #region CanDisconnect        
        public bool CanDisconnect
        {
            get { return IsValidInput && _commentProvider.CanDisconnect; }
        }
        #endregion //CanDisconnect

        #region LiveViewers
        private string _liveViewers;
        public string LiveViewers
        {
            get { return _liveViewers; }
            set
            {
                if (_liveViewers == value) return;
                _liveViewers = value;
                RaisePropertyChanged();
            }
        }
        #endregion //LiveViewers

        #region LiveTitle
        private string _liveTitle;
        public string LiveTitle
        {
            get { return _liveTitle; }
            set
            {
                if (_liveTitle == value) return;
                _liveTitle = value;
                RaisePropertyChanged();
            }
        }
        #endregion //LiveTitle

        public Brush HorizontalGridLineBrush
        {
            get { return new SolidColorBrush(_options.HorizontalGridLineColor); }
        }
        public Brush VerticalGridLineBrush
        {
            get { return new SolidColorBrush(_options.HorizontalGridLineColor); }
        }

        ICommentProvider _commentProvider;
        IOptions _options;
        [GalaSoft.MvvmLight.Ioc.PreferredConstructor]
        internal MainViewModel(IYouTubeSiteContext siteContext, IOptions options, IIo io, ILogger logger)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _siteContext = siteContext;
            _browserLoader = new BrowserLoader(logger);
            _options = options;
            _io = io;
            _logger = logger;
            
            _commentProvider = siteContext.CreateCommentProvider();
            _commentProvider.InitialCommentsReceived += CommentProvider_InitialCommentsReceived;
            _commentProvider.CommentReceived += CommentProvider_CommentReceived;
            _commentProvider.MetadataUpdated += CommentProvider_MetadataUpdated;
            _commentProvider.CanConnectChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(CanConnect));
                RaisePropertyChanged(nameof(CanDisconnect));
            };
            _commentProvider.CanDisconnectChanged += (s, e) =>
            {
                RaisePropertyChanged(nameof(CanConnect));
                RaisePropertyChanged(nameof(CanDisconnect));
            };

            MainViewContentRenderedCommand = new RelayCommand(ContentRendered);
            MainViewClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
            ShowOptionsWindowCommand = new RelayCommand(ShowOptionsWindow);
            CloseCommand = new RelayCommand(Close);
            ShowWebSiteCommand = new RelayCommand(ShowWebSite);
            ShowDevelopersTwitterCommand = new RelayCommand(ShowDevelopersTwitter);
            CheckUpdateCommand = new RelayCommand(CheckUpdate);
            //ShowUserInfoCommand = new RelayCommand(ShowUserInfo);
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
            options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(options.IsTopmost):
                        RaisePropertyChanged(nameof(Topmost));
                        _pluginManager.ForeachPlugin(p => p.OnTopmostChanged(options.IsTopmost));
                        break;
                    case nameof(options.IsShowThumbnail):
                        RaisePropertyChanged(nameof(IsShowThumbnail));
                        break;
                    case nameof(options.IsShowUsername):
                        RaisePropertyChanged(nameof(IsShowUsername));
                        break;
                    case nameof(options.IsShowUserId):
                        RaisePropertyChanged(nameof(IsShowUserId));
                        break;
                    case nameof(options.IsShowPostTime):
                        RaisePropertyChanged(nameof(IsShowPostTime));
                        break;
                }
            };
        }

        private void CommentProvider_MetadataUpdated(object sender, IMetadata e)
        {
            if (e.Title != null)
            {
                LiveTitle = e.Title;
            }
            if (e.CurrentViewers != null)
            {
                LiveViewers = e.CurrentViewers;
            }
        }

        private void CommentProvider_InitialCommentsReceived(object sender, List<ICommentViewModel> e)
        {
            foreach (var comment in e)
            {
                AddComment(comment);
            }
        }
        private async void PluginManager_PluginAdded(object sender, IPlugin e)
        {
            try
            {
                await _dispatcher.BeginInvoke((Action)(() =>
                {
                    var vm = new PluginMenuItemViewModel(e);
                    _pluginMenuItemDict.Add(e, vm);
                    PluginMenuItemCollection.Add(vm);
                }), DispatcherPriority.Normal);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        public ObservableCollection<PluginMenuItemViewModel> PluginMenuItemCollection { get; } = new ObservableCollection<PluginMenuItemViewModel>();
        public ObservableCollection<ICommentViewModel> Comments { get; } = new ObservableCollection<ICommentViewModel>();
        private void AddComment(ICommentViewModel cvm)
        {
            if (_isAddingNewCommentTop)
            {
                Comments.Insert(0, cvm);
            }
            else
            {
                Comments.Add(cvm);
            }
        }
        private void CommentProvider_CommentReceived(object sender, ICommentViewModel e)
        {
            try
            {
                AddComment(e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            if (!e.IsInfo)
            {
                _pluginManager.SetComments(e);
            }
        }
        bool IsValidInput { get; set; }
        private string _input;
        public string Input
        {
            get { return _input; }
            set
            {
                if (_input == value) return;
                _input = value;
                IsValidInput = _siteContext.IsValidInput(_input);
                RaisePropertyChanged(nameof(CanConnect));
                RaisePropertyChanged(nameof(CanDisconnect));
            }
        }
        private async void Connect()
        {
            var selectedBrowser = SelectedBrowserViewModel.Browser;
            var input = Input;
            Comments.Clear();
            try
            {
                await _commentProvider.ConnectAsync(input, selectedBrowser);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            SetInfo("切断されました");
        }
        private void Disconnect()
        {
            _commentProvider.Disconnect();
        }
    }
}
