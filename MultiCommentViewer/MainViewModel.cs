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
namespace MultiCommentViewer
{
    public class MainViewModel: ViewModelBase
    {
        #region Commands
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
        private readonly IPluginManager _pluginManager;
        private readonly ISitePluginLoader _sitePluginLoader;
        private readonly IBrowserLoader _browserLoader;
        IOptions _options;
        IEnumerable<ISiteContext> _siteContexts;
        IEnumerable<SiteViewModel> _siteVms;
        IEnumerable<BrowserViewModel> _browserVms;

        private readonly Dispatcher _dispatcher;
        private readonly IUserStore _userStore;
        Dictionary<string, UserViewModel> _userDict = new Dictionary<string, UserViewModel>();
        Dictionary<ConnectionViewModel, MetadataViewModel> _metaDict = new Dictionary<ConnectionViewModel, MetadataViewModel>();
        #endregion //Fields


        #region Methods
        private void ClearAllComments()
        {
            try
            {
                Comments.Clear();
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
                        list.Add(site.TabPanel);
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
        private void ContentRendered()
        {
            try
            {
                _siteContexts = _sitePluginLoader.LoadSitePlugins(_options, _logger);
                foreach (var site in _siteContexts)
                {
                    site.LoadOptions(_options.SettingsDirPath);
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

                _pluginManager.LoadPlugins(new PluginHost(this, _options));

                _pluginManager.OnLoaded();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Debug.WriteLine(ex.Message);
            }
        }
        private void Closing()
        {
            try
            {
                foreach (var site in _siteContexts)
                {
                    site.SaveOptions(_options.SettingsDirPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Debug.WriteLine(ex.Message);
            }
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
                }
                //TODO:この接続に関連するコメントも全て消したい

            }catch(Exception ex)
            {
                _logger.LogException(ex);
            }
        }

        private void AddNewConnection()
        {
            try
            {
                var connectionName = new ConnectionName();//TODO:一意の名前を設定
                var connection = new ConnectionViewModel(connectionName, _siteVms, _browserVms, _logger);
                connection.CommentReceived += Connection_CommentReceived;
                connection.MetadataReceived += Connection_MetadataReceived;
                var metaVm = new MetadataViewModel(connectionName);
                _metaDict.Add(connection, metaVm);
                MetaCollection.Add(metaVm);
                Connections.Add(connection);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                Debug.WriteLine(ex.Message);
                Debugger.Break();
            }
        }
        #endregion //Methods

        #region EventHandler
        private async void Connection_CommentReceived(object sender, ICommentViewModel e)
        {
            try
            {
                //TODO:Comments.AddRange()が欲しい
                await _dispatcher.BeginInvoke((Action)(() =>
                {
                    var comment = e;
                    if (!_userDict.TryGetValue(comment.UserId, out UserViewModel uvm))
                    {
                        var user = _userStore.Get(comment.UserId);
                        uvm = new UserViewModel(user, _options);
                        _userDict.Add(comment.UserId, uvm);
                    }
                    comment.User = uvm.User;
                    Comments.Add(comment);
                    uvm.Comments.Add(comment);
                }), DispatcherPriority.Normal);
            }catch(Exception ex)
            {
                _logger.LogException(ex);
            }
            try
            {
                _pluginManager.SetComments(e);
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
                }
            }catch(Exception ex)
            {
                _logger.LogException(ex);
            }
        }
        #endregion //EventHandler




        #region Properties
        public ObservableCollection<MetadataViewModel> MetaCollection { get; } = new ObservableCollection<MetadataViewModel>();
        public ObservableCollection<PluginMenuItemViewModel> PluginMenuItemCollection { get; } = new ObservableCollection<PluginMenuItemViewModel>();
        public ObservableCollection<ICommentViewModel> Comments { get; } = new ObservableCollection<ICommentViewModel>();
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();
        public ICommentViewModel SelectedComment { get; set; }
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
        public bool IsShowThumbnail
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
        public bool IsShowUsername
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
        #endregion

        public MainViewModel()
        {

        }
        [GalaSoft.MvvmLight.Ioc.PreferredConstructor]
        public MainViewModel(ILogger logger, IOptions options, ISitePluginLoader sitePluginLoader, IBrowserLoader browserLoader, IUserStore userStore)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            //読み込み
            //IOptionsLoader optionsLoader = DiContainer.Instance.GetNewInstance<IOptionsLoader>();
            _options = options;// optionsLoader.LoadOptions();
            _userStore = userStore;// DiContainer.Instance.GetNewInstance<IUserStore>();
            _pluginManager = new PluginManager(_options);
            _pluginManager.PluginAdded += PluginManager_PluginAdded;
            _logger = logger;//= DiContainer.Instance.GetNewInstance<ILogger>();
            _sitePluginLoader = sitePluginLoader;
            _browserLoader = browserLoader;

            MainViewContentRenderedCommand = new RelayCommand(ContentRendered);
            MainViewClosingCommand = new RelayCommand(Closing);
            ShowOptionsWindowCommand = new RelayCommand(ShowOptionsWindow);
            ExitCommand = new RelayCommand(Exit);
            ShowWebSiteCommand = new RelayCommand(ShowWebSite);
            ShowDevelopersTwitterCommand = new RelayCommand(ShowDevelopersTwitter);
            CheckUpdateCommand = new RelayCommand(CheckUpdate);
            AddNewConnectionCommand = new RelayCommand(AddNewConnection);
            RemoveSelectedConnectionCommand = new RelayCommand(RemoveSelectedConnection);
            ClearAllCommentsCommand = new RelayCommand(ClearAllComments);
            ShowUserInfoCommand = new RelayCommand(ShowUserInfo);

            _options.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
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

        private void PluginManager_PluginAdded(object sender, IPlugin e)
        {
            try
            {
                var vm = new PluginMenuItemViewModel(e);
                _pluginMenuItemDict.Add(e, vm);
                PluginMenuItemCollection.Add(vm);
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
                Debug.Assert(current is ICommentViewModel);
                var uvm = _userDict[current.UserId];
                MessengerInstance.Send(new ShowUserViewMessage(uvm));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
            }
        }
        private void CheckUpdate()
        {

        }
        private void ShowDevelopersTwitter()
        {

        }
        private void ShowWebSite()
        {

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
        private readonly MainViewModel _vm;
        private readonly IOptions _options;
        public PluginHost(MainViewModel vm, IOptions options)
        {
            _vm = vm;
            _options = options;
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
    public interface IBrowserLoader
    {
        IEnumerable<IBrowserProfile> LoadBrowsers();
    }
    public class BrowserLoader : IBrowserLoader
    {
        public IEnumerable<IBrowserProfile> LoadBrowsers()
        {
            var list = new List<IBrowserProfile>();
            list.AddRange(new ChromeManager().GetProfiles());
            return list;
        }
    }
    public interface IUserStore
    {
        IUser Get(string userid);
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
        Dictionary<string, IUser> _dict = new Dictionary<string, IUser>();
        public IUser Get(string userid)
        {
            if (!_dict.TryGetValue(userid, out IUser user))
            {
                user = new UserTest(userid);
                _dict.Add(userid, user);
            }
            return user;
        }
    }
    public class PluginMenuItemViewModel
    {
        public string Name { get; set; }
        public ObservableCollection<PluginMenuItemViewModel> Children { get; } = new ObservableCollection<PluginMenuItemViewModel>();
        public ICommand ShowSettingViewCommand { get; }
        public PluginMenuItemViewModel(IPlugin plugin)// PluginContext plugin, string name, ICommand command)
        {
            Name = plugin.Name;            
            ShowSettingViewCommand = new RelayCommand(() => Test(plugin));
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

