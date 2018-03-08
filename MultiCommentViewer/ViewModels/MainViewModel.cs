using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
using System.Reflection;
using System.ComponentModel;
using MultiCommentViewer.Test;
using Common;
using System.Windows.Data;

namespace MultiCommentViewer
{
    public class MainViewModel : CommentDataGridViewModelBase
    {
        #region Commands
        public ICommand ActivatedCommand { get; }
        public ICommand LoadedCommand { get; }
        public ICommand MainViewContentRenderedCommand { get; }
        public ICommand MainViewClosingCommand { get; }
        public ICommand ShowOptionsWindowCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ShowWebSiteCommand { get; }
        public ICommand ShowDevelopersTwitterCommand { get; }
        public ICommand CheckUpdateCommand { get; }
        public ICommand ShowUserInfoCommand { get; }
        public ICommand RemoveSelectedConnectionCommand { get; }
        public ICommand AddNewConnectionCommand { get; }
        public ICommand ClearAllCommentsCommand { get; }
        #endregion //Commands

        #region Fields
        private readonly Dictionary<IPlugin, PluginMenuItemViewModel> _pluginMenuItemDict = new Dictionary<IPlugin, PluginMenuItemViewModel>();
        private readonly ILogger _logger;
        private IPluginManager _pluginManager;
        private readonly ISitePluginLoader _sitePluginLoader;
        private readonly IBrowserLoader _browserLoader;
        private readonly IIo _io;
        IOptions _options;
        IEnumerable<ISiteContext> _siteContexts;
        IEnumerable<SiteViewModel> _siteVms;
        IEnumerable<BrowserViewModel> _browserVms;

        private readonly Dispatcher _dispatcher;
        private readonly IUserStore _userStore;
        Dictionary<ConnectionViewModel, MetadataViewModel> _metaDict = new Dictionary<ConnectionViewModel, MetadataViewModel>();
        #endregion //Fields


        #region Methods
        private void Activated()
        {

        }
        private void Loaded()
        {

        }
        private void ClearAllComments()
        {
            try
            {
                //Comments.Clear();
                //個別ユーザのコメントはどうしようか

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
                foreach (var site in _siteContexts)
                {
                    try
                    {
                        var tabPanel = site.TabPanel;
                        if (tabPanel == null)
                            continue;
                        list.Add(tabPanel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                        Debug.WriteLine(ex.Message);
                    }
                }
                MessengerInstance.Send(new ShowOptionsViewMessage(list));
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private string GetOptionsPath()
        {
            return System.IO.Path.Combine(_options.SettingsDirPath, "options.txt");
        }
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
                //Observable.Interval()
                //_optionsLoader.LoadAsync().
                _siteContexts = _sitePluginLoader.LoadSitePlugins(_options, _logger, _userStore, _dic1, _dispatcher);
                foreach (var site in _siteContexts)
                {
                    try
                    {
                        var path = GetSiteOptionsPath(site);
                        site.LoadOptions(path, _io);
                    }catch(Exception ex)
                    {
                        _logger.LogException(ex);
                    }
                }
                _siteVms = _siteContexts.Select(c => new SiteViewModel(c));

                _browserVms = _browserLoader.LoadBrowsers().Select(b => new BrowserViewModel(b));
                //もしブラウザが無かったらclass EmptyBrowserProfileを使う。
                if (_browserVms.Count() == 0)
                {
                    _browserVms = new List<BrowserViewModel>
                            {
                                new BrowserViewModel( new EmptyBrowserProfile()),
                            };
                }

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
        private void Closing(CancelEventArgs e)
        {
            if (_siteContexts != null)
            {
                foreach (var site in _siteContexts)
                {
                    try
                    {
                        var path = GetSiteOptionsPath(site);
                        site.SaveOptions(path, _io);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
            _pluginManager?.OnClosing();
        }
        private void RemoveSelectedConnection()
        {
            try
            {
                var toRemove = Connections.Where(conn => conn.IsSelected).ToList();
                foreach (var conn in toRemove)
                {
                    Connections.Remove(conn);
                    var meta = _metaDict[conn];
                    _metaDict.Remove(conn);
                    MetaCollection.Remove(meta);
                    OnConnectionDeleted(conn.ConnectionName);
                }
                //TODO:この接続に関連するコメントも全て消したい

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private string GetDefaultName(IEnumerable<string> existingNames)
        {
            for (var n = 1; ; n++)
            {
                var testName = "#" + n;
                if (!existingNames.Contains(testName))
                {
                    return testName;
                }
            }
        }
        private void AddNewConnection()
        {
            try
            {
                var name = GetDefaultName(Connections.Select(c => c.Name));
                var connectionName = new ConnectionName { Name = name };
                var connection = new ConnectionViewModel(connectionName, _siteVms, _browserVms, _logger);
                connection.Renamed += Connection_Renamed;
                connection.CommentReceived += Connection_CommentReceived;
                connection.InitialCommentsReceived += Connection_InitialCommentsReceived;
                connection.MetadataReceived += Connection_MetadataReceived;
                connection.SelectedSiteChanged += Connection_SelectedSiteChanged;
                var metaVm = new MetadataViewModel(connectionName);
                _metaDict.Add(connection, metaVm);
                MetaCollection.Add(metaVm);
                Connections.Add(connection);
                OnConnectionAdded(connection);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Debug.WriteLine(ex.Message);
                Debugger.Break();
            }
        }
        /// <summary>
        /// 将来的にSiteContext毎に別のIUserStoreを使い分ける可能性を考えて今のうちに。
        /// </summary>
        Dictionary<ISiteContext, IUserStore> _dic1 = new Dictionary<ISiteContext, IUserStore>();
        /// <summary>
        /// Connection_SelectedSiteChanged内で値を設定
        /// </summary>
        Dictionary<ICommentProvider, IUserStore> _dict2 = new Dictionary<ICommentProvider, IUserStore>();
        //Dictionary<ConnectionName, >
        private void Connection_SelectedSiteChanged(object sender, SelectedSiteChangedEventArgs e)
        {
            SetDict(e.NewValue);

            var connectionVm = sender as ConnectionViewModel;
            Debug.Assert(connectionVm != null);
            if (connectionVm == SelectedConnection)
            {
                MessengerInstance.Send(new SetPostCommentPanel(connectionVm.CommentPostPanel));
            }
        }

        private void Connection_Renamed(object sender, RenamedEventArgs e)
        {
            //TODO:プラグインに通知
            Debug.WriteLine($"ConnectionRenamed:{e.OldValue}→{e.NewValue}");
        }

        private void OnConnectionAdded(ConnectionViewModel connection)
        {
            //TODO:プラグインに通知
            Debug.WriteLine($"ConnectionAdded:{connection.ConnectionName.Guid}");

            var context = connection.GetCurrent();
            SetDict(context);

            if(SelectedConnection == null)
            {
                SelectedConnection = connection;
            }
        }
        private void SetDict(ConnectionContext context)
        {
            var newSiteContext = context.SiteContext;
            var newCommentProvider = context.CommentProvider;
            var userStore = _dic1[newSiteContext];
            if (!_dict2.ContainsKey(newCommentProvider))
            {
                _dict2.Add(newCommentProvider, userStore);
            }
        }
        private void OnConnectionDeleted(ConnectionName connectionName)
        {
            //TODO:プラグインに通知
            Debug.WriteLine($"ConnectionDeleted:{connectionName.Guid}");
        }
        string Name
        {
            get { return "MultiCommentViewer"; }
        }
        string Fullname
        {
            get { return $""; }
        }
        void SetInfo(string message)
        {

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
        #endregion //Methods
        
        private void AddComment(ICommentViewModel cvm, ConnectionName connectionName)
        {
            var mcvCvm = new McvCommentViewModel(cvm, connectionName);
            _comments.Add(mcvCvm);
        }
        #region EventHandler
        private async void Connection_InitialCommentsReceived(object sender, List<ICommentViewModel> e)
        {
            var connectionViewModel = sender as ConnectionViewModel;
            Debug.Assert(connectionViewModel != null);
            try
            {
                //TODO:Comments.AddRange()が欲しい
                await _dispatcher.BeginInvoke((Action)(() =>
                {
                    foreach (var comment in e)
                    {
                        AddComment(comment, connectionViewModel.ConnectionName);
                    }
                }), DispatcherPriority.Normal);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
            //TODO:Pluginに渡す
        }
        private async void Connection_CommentReceived(object sender, ICommentViewModel e)
        {
            var connectionViewModel = sender as ConnectionViewModel;
            Debug.Assert(connectionViewModel != null);
            try
            {
                //TODO:Comments.AddRange()が欲しい
                await _dispatcher.BeginInvoke((Action)(() =>
                {
                    var comment = e;
                    //if (!_userDict.TryGetValue(comment.UserId, out UserViewModel uvm))
                    //{
                    //    var user = _userStore.GetUser(comment.UserId);
                    //    uvm = new UserViewModel(user, _options);
                    //    _userDict.Add(comment.UserId, uvm);
                    //}
                    //comment.User = uvm.User;
                    AddComment(comment, connectionViewModel.ConnectionName);
                    //uvm.Comments.Add(comment);
                }), DispatcherPriority.Normal);
                if (!e.IsInfo)
                {
                    _pluginManager.SetComments(e);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
            }
        }
        private void Connection_MetadataReceived(object sender, IMetadata e)
        {
            try
            {
                if (sender is ConnectionViewModel connection)
                {
                    var metaVm = _metaDict[connection];
                    if (e.Title != null)
                        metaVm.Title = e.Title;
                    if (e.Active != null)
                        metaVm.Active = e.Active;
                    if (e.CurrentViewers != null)
                        metaVm.CurrentViewers = e.CurrentViewers;
                    if (e.TotalViewers != null)
                        metaVm.TotalViewers = e.TotalViewers;
                    if (e.Elapsed != null)
                        metaVm.Elapsed = e.Elapsed;
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        #endregion //EventHandler




        #region Properties
        public ObservableCollection<MetadataViewModel> MetaCollection { get; } = new ObservableCollection<MetadataViewModel>();
        public ObservableCollection<PluginMenuItemViewModel> PluginMenuItemCollection { get; } = new ObservableCollection<PluginMenuItemViewModel>();
        private readonly ObservableCollection<McvCommentViewModel> _comments = new ObservableCollection<McvCommentViewModel>();
        public ICollectionView Comments { get; }
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();

        private ConnectionViewModel _selectedConnection;
        public ConnectionViewModel SelectedConnection
        {
            get { return _selectedConnection; }
            set
            {
                _selectedConnection = value;
                if (_selectedConnection == null)
                {
                    MessengerInstance.Send(new SetPostCommentPanel(null));
                }
                else
                {
                    MessengerInstance.Send(new SetPostCommentPanel(_selectedConnection.CommentPostPanel));
                }
                RaisePropertyChanged();
            }
        }
        public string Title
        {
            get
            {
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var ver = asm.GetName().Version;
                var title = asm.GetName().Name;
                var s = $"{title} v{ver.Major}.{ver.Minor}.{ver.Build}";
#if DEBUG
                s += " (DEBUG)";
#endif
                return s;
            }
        }
        public bool Topmost { get { return false; } }
        public double MainViewHeight
        {
            get { return _options.MainViewHeight; }
            set { _options.MainViewHeight = value; }
        }
        public double MainViewWidth
        {
            get { return _options.MainViewWidth; }
            set { _options.MainViewWidth = value; }
        }
        public double MainViewLeft
        {
            get { return _options.MainViewLeft; }
            set { _options.MainViewLeft = value; }
        }
        public double MainViewTop
        {
            get { return _options.MainViewTop; }
            set { _options.MainViewTop = value; }
        }
        public double ConnectionViewHeight
        {
            get { return _options.ConnectionViewHeight; }
            set { _options.ConnectionViewHeight = value; }
        }
        public double MetadataViewHeight
        {
            get { return _options.MetadataViewHeight; }
            set { _options.MetadataViewHeight = value; }
        }
        //public Brush HorizontalGridLineBrush
        //{
        //    get { return new SolidColorBrush(_options.HorizontalGridLineColor); }
        //}
        //public Brush VerticalGridLineBrush
        //{
        //    get { return new SolidColorBrush(_options.VerticalGridLineColor); }
        //}
        //public double ConnectionNameWidth
        //{
        //    get { return _options.ConnectionNameWidth; }
        //    set { _options.ConnectionNameWidth = value; }
        //}
        //public bool IsShowConnectionName
        //{
        //    get { return _options.IsShowConnectionName; }
        //    set { _options.IsShowConnectionName = value; }
        //}
        //public int ConnectionNameDisplayIndex
        //{
        //    get { return _options.ConnectionNameDisplayIndex; }
        //    set { _options.ConnectionNameDisplayIndex = value; }
        //}
        //public double ThumbnailWidth
        //{
        //    get { return _options.ThumbnailWidth; }
        //    set { _options.ThumbnailWidth = value; }
        //}
        //public bool IsShowThumbnail
        //{
        //    get { return _options.IsShowThumbnail; }
        //    set { _options.IsShowThumbnail = value; }
        //}
        //public int ThumbnailDisplayIndex
        //{
        //    get { return _options.ThumbnailDisplayIndex; }
        //    set { _options.ThumbnailDisplayIndex = value; }
        //}
        //public double CommentIdWidth
        //{
        //    get { return _options.CommentIdWidth; }
        //    set { _options.CommentIdWidth = value; }
        //}
        //public bool IsShowCommentId
        //{
        //    get { return _options.IsShowCommentId; }
        //    set { _options.IsShowCommentId = value; }
        //}
        //public int CommentIdDisplayIndex
        //{
        //    get { return _options.CommentIdDisplayIndex; }
        //    set { _options.CommentIdDisplayIndex = value; }
        //}
        //public double UsernameWidth
        //{
        //    get { return _options.UsernameWidth; }
        //    set { _options.UsernameWidth = value; }
        //}
        //public bool IsShowUsername
        //{
        //    get { return _options.IsShowUsername; }
        //    set { _options.IsShowUsername = value; }
        //}
        //public int UsernameDisplayIndex
        //{
        //    get { return _options.UsernameDisplayIndex; }
        //    set { _options.UsernameDisplayIndex = value; }
        //}

        //public double MessageWidth
        //{
        //    get { return _options.MessageWidth; }
        //    set { _options.MessageWidth = value; }
        //}
        //public bool IsShowMessage
        //{
        //    get { return _options.IsShowMessage; }
        //    set { _options.IsShowMessage = value; }
        //}
        //public int MessageDisplayIndex
        //{
        //    get { return _options.MessageDisplayIndex; }
        //    set { _options.MessageDisplayIndex = value; }
        //}

        //public double InfoWidth
        //{
        //    get { return _options.InfoWidth; }
        //    set { _options.InfoWidth = value; }
        //}
        //public bool IsShowInfo
        //{
        //    get { return _options.IsShowInfo; }
        //    set { _options.IsShowInfo = value; }
        //}
        //public int InfoDisplayIndex
        //{
        //    get { return _options.InfoDisplayIndex; }
        //    set { _options.InfoDisplayIndex = value; }
        //}
        //public Color SelectedRowBackColor
        //{
        //    get { return _options.SelectedRowBackColor; }
        //    set { _options.SelectedRowBackColor = value; }
        //}
        //public Color SelectedRowForeColor
        //{
        //    get { return _options.SelectedRowForeColor; }
        //    set { _options.SelectedRowForeColor = value; }
        //}
        #endregion

        public MainViewModel():base(new DynamicOptionsTest())
        {
            if (IsInDesignMode)
            {

            }
            else
            {
                throw new NotSupportedException();
            }
        }
        [GalaSoft.MvvmLight.Ioc.PreferredConstructor]
        public MainViewModel(IIo io, ILogger logger, IOptions options, ISitePluginLoader sitePluginLoader, IBrowserLoader browserLoader, IUserStore userStore)
            :base(options)
        {
            _io = io;
            _dispatcher = Dispatcher.CurrentDispatcher;
            
            _options = options;
            _userStore = userStore;

            _logger = logger;
            _sitePluginLoader = sitePluginLoader;
            _browserLoader = browserLoader;

            Comments = CollectionViewSource.GetDefaultView(_comments);

            MainViewContentRenderedCommand = new RelayCommand(ContentRendered);
            MainViewClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
            ShowOptionsWindowCommand = new RelayCommand(ShowOptionsWindow);
            ExitCommand = new RelayCommand(Exit);
            ShowWebSiteCommand = new RelayCommand(ShowWebSite);
            ShowDevelopersTwitterCommand = new RelayCommand(ShowDevelopersTwitter);
            CheckUpdateCommand = new RelayCommand(CheckUpdate);
            AddNewConnectionCommand = new RelayCommand(AddNewConnection);
            RemoveSelectedConnectionCommand = new RelayCommand(RemoveSelectedConnection);
            ClearAllCommentsCommand = new RelayCommand(ClearAllComments);
            ShowUserInfoCommand = new RelayCommand(ShowUserInfo);
            ActivatedCommand = new RelayCommand(Activated);
            LoadedCommand = new RelayCommand(Loaded);
            _options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_options.MainViewLeft):
                        RaisePropertyChanged(nameof(MainViewLeft));
                        break;
                    case nameof(_options.MainViewTop):
                        RaisePropertyChanged(nameof(MainViewTop));
                        break;
                    case nameof(_options.MainViewHeight):
                        RaisePropertyChanged(nameof(MainViewHeight));
                        break;
                    case nameof(_options.MainViewWidth):
                        RaisePropertyChanged(nameof(MainViewWidth));
                        break;
                    case nameof(_options.IsShowThumbnail):
                        RaisePropertyChanged(nameof(IsShowThumbnail));
                        break;
                    case nameof(_options.IsShowUsername):
                        RaisePropertyChanged(nameof(IsShowUsername));
                        break;
                    case nameof(_options.IsShowConnectionName):
                        RaisePropertyChanged(nameof(IsShowConnectionName));
                        break;
                    case nameof(_options.IsShowCommentId):
                        RaisePropertyChanged(nameof(IsShowCommentId));
                        break;
                    case nameof(_options.IsShowMessage):
                        RaisePropertyChanged(nameof(IsShowMessage));
                        break;
                    case nameof(_options.IsShowInfo):
                        RaisePropertyChanged(nameof(IsShowInfo));
                        break;
                }
            };
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

        private void ShowUserInfo()
        {
            var current = SelectedComment;
            try
            {
                Debug.Assert(current != null);
                Debug.Assert(current is McvCommentViewModel);

                var userId = current.UserId;
                var view = new CollectionViewSource { Source = _comments }.View;
                view.Filter = obj =>
                {
                    if(!(obj is McvCommentViewModel cvm))
                    {
                        return false;
                    }
                    return cvm.UserId == userId;
                };
                //ICommentProviderが必要。。。ConnectionViewModel経由で取れないだろうか。
                //Connectionを切断したり、サイトを変更してもコメントは残る。残ったコメントのユーザ情報を見ようとした時にConnectionViewModel経由で取るのは無理だろう。
                //やっぱりCommentViewModelにICommentProviderを持たせるしかなさそう。
                ICommentProvider commentProvider = current.CommentProvider;
                //var s = commentProvider.GetUserComments(current.User) as ObservableCollection<ICommentViewModel>;
                //var collection = new ObservableCollection<McvCommentViewModel>(s.Select(m => new McvCommentViewModel(m, current.ConnectionName));

                //s.CollectionChanged += (sender, e) =>
                //{
                //    switch (e.Action)
                //    {
                //        case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                //            break;
                //    }
                //};
                var userStore = _dict2[commentProvider];
                var user = userStore.GetUser(userId);
                var uvm = new UserViewModel(user, _options, view);
                MessengerInstance.Send(new ShowUserViewMessage(uvm));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
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
                System.Diagnostics.Process.Start("https://ryu-s.github.io/app/multicommentviewer");
            }catch(Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        private void Exit()
        {

        }
    }
    public class PluginHost : IPluginHost
    {
        public string SettingsDirPath => _options.SettingsDirPath;

        public double MainViewLeft => _options.MainViewLeft;

        public double MainViewTop => _options.MainViewTop;
        public bool IsTopmost => _options.IsTopmost;
        public string LoadOptions(string path)
        {
            var s = _io.ReadFile(path);
            return s;
        }

        public void SaveOptions(string path, string s)
        {
            _io.WriteFile(path, s);
        }

        private readonly MainViewModel _vm;
        private readonly IOptions _options;
        private readonly IIo _io;
        public PluginHost(MainViewModel vm, IOptions options, IIo io)
        {
            _vm = vm;
            _options = options;
            _io = io;
        }
    }
    public class CommentData : Plugin.ICommentData
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string Nickname { get; set; }

        public string Comment { get; set; }
        public bool IsNgUser { get; set; }
    }
    public class EmptyBrowserProfile : IBrowserProfile
    {
        public string Path => "";

        public string ProfileName => "無し";

        public BrowserType Type { get { return BrowserType.Unknown; } }

        public Cookie GetCookie(string domain, string name)
        {
            return null;
        }

        public CookieCollection GetCookieCollection(string domain)
        {
            return new CookieCollection();
        }
    }
    public class UserTest : IUser
    {
        public string UserId { get { return _userid; } }
        public string ForeColorArgb { get; set; }
        public string BackColorArgb { get; set; }

        private string _nickname;
        public string Nickname
        {
            get { return _nickname; }
            set
            {
                if (_nickname == value)
                    return;
                _nickname = value;
                RaisePropertyChanged();
            }
        }
        private readonly string _userid;
        public UserTest(string userId)
        {
            _userid = userId;
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
    public class UserStoreTest : IUserStore
    {
        //Dictionary<string, IUser> _dict = new Dictionary<string, IUser>();
        System.Collections.Concurrent.ConcurrentDictionary<string, IUser> _dict = new System.Collections.Concurrent.ConcurrentDictionary<string, IUser>();
        public IUser GetUser(string userid)
        {
            if (!_dict.TryGetValue(userid, out IUser user))
            {
                user = new UserTest(userid);
                _dict.AddOrUpdate(userid, user, (_,u)=>u);
            }
            return user;
        }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public void Update(IUser user)
        {
            throw new NotImplementedException();
        }
    }
    public class PluginMenuItemViewModel:ViewModelBase
    {
        public string Name { get; set; }
        public ObservableCollection<PluginMenuItemViewModel> Children { get; } = new ObservableCollection<PluginMenuItemViewModel>();
        private RelayCommand _show;
        public ICommand ShowSettingViewCommand
        {
            //以前はコンストラクタ中でICommandに代入していたが、項目をクリックしてもTest()が呼ばれないことがあった。今の状態に書き換えたら問題なくなった。何故だ？IPluginを保持するようにしたから？GCで無くなっちゃってたとか？
            get
            {
                if(_show == null)
                {
                    _show = new RelayCommand(()=> Test(_plugin));
                }
                return _show;
            }
        }
        private readonly IPlugin _plugin;
        public PluginMenuItemViewModel(IPlugin plugin)// PluginContext plugin, string name, ICommand command)
        {
            Name = plugin.Name;
            _plugin = plugin;
        }
        private void Test(IPlugin plugin)
        {
            try
            {
                plugin.ShowSettingView();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}

