using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Mcv.PluginV2;
using MultiCommentViewer.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Mcv.MainViewPlugin
{
    static class DesignModeUtils
    {
        public static bool IsDesignMode { get; } = (bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(System.Windows.DependencyObject)).DefaultValue);
    }
    interface ILiveSiteMessageProcessor
    {
        bool IsValidMessage(ISiteMessage message);
        IMcvCommentViewModel? CreateViewModel(ISiteMessage message, ConnectionName connName, IMainViewPluginOptions options, MyUser? user);
    }
    interface IConnectionNameHost : INotifyPropertyChanged
    {
        Task<string> GetConnectionName(ConnectionId connId);
        void SetConnectionName(ConnectionId connId, string newConnectionName);
    }
    class ConnectionName : ViewModelBase, INotifyPropertyChanged
    {
        private readonly IConnectionNameHost _host;
        private readonly ConnectionId _connId;
        public string Name
        {
            get
            {
                GetName();
                return _name;
            }
            set
            {
                _host.SetConnectionName(_connId, value);
            }
        }
        string _name = "";
        private async void GetName()
        {
            var name = await _host.GetConnectionName(_connId);
            if (_name != name)
            {
                _name = name;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public Color BackColor { get; internal set; }
        public Color ForeColor { get; internal set; }

        public void OnNameChanged()
        {
            RaisePropertyChanged(nameof(Name));
        }
        public ConnectionName(IConnectionNameHost host, ConnectionId connId)
        {
            _host = host;
            _connId = connId;
        }
    }
    class ShowOptionsViewMessage : ValueChangedMessage<List<IOptionsTabPage>>
    {
        public ShowOptionsViewMessage(List<IOptionsTabPage> value) : base(value)
        {
        }
    }
    internal class ShowUserInfoViewMessage : RequestMessage<string>
    {
        public ShowUserInfoViewMessage(UserInfoViewModel userInfoVm)
        {
            UserInfoVm = userInfoVm;
        }

        public UserInfoViewModel UserInfoVm { get; }
    }
    internal class ShowUserListViewMessage : RequestMessage<string>
    {
        public ShowUserListViewMessage(UserListViewModel userListVm)
        {
            UserListVm = userListVm;
        }

        public UserListViewModel UserListVm { get; }
    }
    class DesignTimeComment : IMcvCommentViewModel
    {
        public string UserId { get; }
        public IEnumerable<IMessagePart> MessageItems { get; set; }
        public bool IsTranslated { get; set; }
        public MyUser? User { get; }

        public DesignTimeComment(string userId, IEnumerable<IMessagePart> message)
        {
            UserId = userId;
            MessageItems = message;
        }
    }
    class UserInfoViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly IMainViewPluginOptions _options;
        public CommentDataGridViewModel CommentVm { get; }
        public UserInfoViewModel(ICollectionView comments, UserViewModel userVm, IMainViewPluginOptions options)
        {
            UserVm = userVm;
            _options = options;
            CommentVm = new CommentDataGridViewModel(options, comments);
        }
        public UserInfoViewModel()
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(System.Windows.DependencyObject)).DefaultValue))
            {
                _options = default!;//TODO:nullではなくてclassを作る
                var TestList = new ObservableCollection<IMcvCommentViewModel>
                {
                    new DesignTimeComment("abc", MessagePartFactory.CreateMessageItems("k1")),
                    new DesignTimeComment("abc", MessagePartFactory.CreateMessageItems("k2")),
                    new DesignTimeComment("abc", MessagePartFactory.CreateMessageItems("k3")),
                };
                var comments = CollectionViewSource.GetDefaultView(TestList);
                CommentVm = new CommentDataGridViewModel(_options, comments);
                UserVm = new UserViewModel(new MyUser("abc")
                {
                    Name = MessagePartFactory.CreateMessageItems("USERNAME"),
                    Nickname = "NICKNAME",
                    IsNgUser = true,
                    IsSiteNgUser = true,
                    BackColorArgb = "#FF0000",
                    ForeColorArgb = "#00FF00",
                }, _options);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public UserViewModel UserVm { get; }
        public bool Topmost
        {
            get { return _options.IsTopmost; }
            set { _options.IsTopmost = value; }
        }
        public double UserInfoViewHeight
        {
            get { return _options.UserInfoViewHeight; }
            set { _options.UserInfoViewHeight = value; }
        }
        public double UserInfoViewWidth
        {
            get { return _options.UserInfoViewWidth; }
            set { _options.UserInfoViewWidth = value; }
        }
        public double UserInfoViewLeft
        {
            get { return _options.UserInfoViewLeft; }
            set { _options.UserInfoViewLeft = value; }
        }
        public double UserInfoViewTop
        {
            get { return _options.UserInfoViewTop; }
            set { _options.UserInfoViewTop = value; }
        }
    }
    class MainViewModel : ViewModelBase, INotifyPropertyChanged, IUserViewModelProvider
    {
        public Brush MenuBackground => new SolidColorBrush(_adapter.Options.MenuBackColor);
        public Brush MenuForeground => new SolidColorBrush(_adapter.Options.MenuForeColor);
        public Brush MenuSeparatorBackground => new SolidColorBrush(_adapter.Options.MenuSeparatorBackColor);
        public Brush MenuPopupBorderBrush => new SolidColorBrush(_adapter.Options.MenuPopupBorderColor);
        public Brush MenuItemMouseOverBackground => new SolidColorBrush(_adapter.Options.MenuItemMouseOverBackColor);

        public Brush MetadataDataGridBackground => new SolidColorBrush(_adapter.Options.CommentListBackColor);
        public Brush MetadataDataGridBorderBrush => new SolidColorBrush(_adapter.Options.CommentListBorderColor);

        public bool Topmost
        {
            get { return _adapter.Options.IsTopmost; }
            set { _adapter.Options.IsTopmost = value; }
        }
        public double MainViewHeight
        {
            get { return _adapter.Options.MainViewHeight; }
            set { _adapter.Options.MainViewHeight = value; }
        }
        public double MainViewWidth
        {
            get { return _adapter.Options.MainViewWidth; }
            set { _adapter.Options.MainViewWidth = value; }
        }
        public double MainViewLeft
        {
            get { return _adapter.Options.MainViewLeft; }
            set { _adapter.Options.MainViewLeft = value; }
        }
        public double MainViewTop
        {
            get { return _adapter.Options.MainViewTop; }
            set { _adapter.Options.MainViewTop = value; }
        }
        public double ConnectionViewHeight
        {
            get { return _adapter.Options.ConnectionViewHeight; }
            set { _adapter.Options.ConnectionViewHeight = value; }
        }
        public double MetadataViewHeight
        {
            get { return _adapter.Options.MetadataViewHeight; }
            set { _adapter.Options.MetadataViewHeight = value; }
        }
        public double MetadataViewConnectionNameColumnWidth
        {
            get { return _adapter.Options.MetadataViewConnectionNameColumnWidth; }
            set { _adapter.Options.MetadataViewConnectionNameColumnWidth = value; }
        }
        public double MetadataViewTitleColumnWidth
        {
            get { return _adapter.Options.MetadataViewTitleColumnWidth; }
            set { _adapter.Options.MetadataViewTitleColumnWidth = value; }
        }
        public double MetadataViewElapsedColumnWidth
        {
            get { return _adapter.Options.MetadataViewElapsedColumnWidth; }
            set { _adapter.Options.MetadataViewElapsedColumnWidth = value; }
        }
        public double MetadataViewCurrentViewersColumnWidth
        {
            get { return _adapter.Options.MetadataViewCurrentViewersColumnWidth; }
            set { _adapter.Options.MetadataViewCurrentViewersColumnWidth = value; }
        }
        public double MetadataViewTotalViewersColumnWidth
        {
            get { return _adapter.Options.MetadataViewTotalViewersColumnWidth; }
            set { _adapter.Options.MetadataViewTotalViewersColumnWidth = value; }
        }
        public double MetadataViewActiveColumnWidth
        {
            get { return _adapter.Options.MetadataViewActiveColumnWidth; }
            set { _adapter.Options.MetadataViewActiveColumnWidth = value; }
        }
        public double MetadataViewOthersColumnWidth
        {
            get { return _adapter.Options.MetadataViewOthersColumnWidth; }
            set { _adapter.Options.MetadataViewOthersColumnWidth = value; }
        }
        public bool IsShowMetaConnectionName
        {
            get => _adapter.Options.IsShowMetaConnectionName;
            set => _adapter.Options.IsShowMetaConnectionName = value;
        }
        public int MetadataViewConnectionNameDisplayIndex
        {
            get => _adapter.Options.MetadataViewConnectionNameDisplayIndex;
            set => _adapter.Options.MetadataViewConnectionNameDisplayIndex = value;
        }

        public bool IsShowMetaTitle
        {
            get => _adapter.Options.IsShowMetaTitle;
            set => _adapter.Options.IsShowMetaTitle = value;
        }
        public int MetadataViewTitleDisplayIndex
        {
            get => _adapter.Options.MetadataViewTitleDisplayIndex;
            set => _adapter.Options.MetadataViewTitleDisplayIndex = value;
        }

        public bool IsShowMetaElapse
        {
            get => _adapter.Options.IsShowMetaElapse;
            set => _adapter.Options.IsShowMetaElapse = value;
        }
        public int MetadataViewElapsedDisplayIndex
        {
            get => _adapter.Options.MetadataViewElapsedDisplayIndex;
            set => _adapter.Options.MetadataViewElapsedDisplayIndex = value;
        }

        public bool IsShowMetaCurrentViewers
        {
            get => _adapter.Options.IsShowMetaCurrentViewers;
            set => _adapter.Options.IsShowMetaCurrentViewers = value;
        }
        public int MetadataViewCurrentViewersDisplayIndex
        {
            get => _adapter.Options.MetadataViewCurrentViewersDisplayIndex;
            set => _adapter.Options.MetadataViewCurrentViewersDisplayIndex = value;
        }

        public bool IsShowMetaTotalViewers
        {
            get => _adapter.Options.IsShowMetaTotalViewers;
            set => _adapter.Options.IsShowMetaTotalViewers = value;
        }
        public int MetadataViewTotalViewersDisplayIndex
        {
            get => _adapter.Options.MetadataViewTotalViewersDisplayIndex;
            set => _adapter.Options.MetadataViewTotalViewersDisplayIndex = value;
        }

        public bool IsShowMetaActive
        {
            get => _adapter.Options.IsShowMetaActive;
            set => _adapter.Options.IsShowMetaActive = value;
        }
        public int MetadataViewActiveDisplayIndex
        {
            get => _adapter.Options.MetadataViewActiveDisplayIndex;
            set => _adapter.Options.MetadataViewActiveDisplayIndex = value;
        }

        public bool IsShowMetaOthers
        {
            get => _adapter.Options.IsShowMetaOthers;
            set => _adapter.Options.IsShowMetaOthers = value;
        }
        public int MetadataViewOthersDisplayIndex
        {
            get => _adapter.Options.MetadataViewOthersDisplayIndex;
            set => _adapter.Options.MetadataViewOthersDisplayIndex = value;
        }
        #region Commands
        public ICommand MainViewClosingCommand { get; }
        public ICommand ShowOptionsWindowCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ShowWebSiteCommand { get; }
        public ICommand ShowDevelopersTwitterCommand { get; }
        public ICommand CheckUpdateCommand { get; }
        public ICommand ShowUserInfoCommand { get; }
        public ICommand ShowUserListCommand { get; }
        public ICommand RemoveSelectedConnectionCommand { get; }
        public ICommand AddNewConnectionCommand { get; }
        public ICommand ClearAllCommentsCommand { get; }


        #endregion //Commands

        #region Fields
        //private readonly Dictionary<IPlugin, PluginMenuItemViewModel> _pluginMenuItemDict = new Dictionary<IPlugin, PluginMenuItemViewModel>();
        //private readonly ILogger _logger;
        //private IPluginManager _pluginManager;
        //private readonly ISitePluginLoader _sitePluginLoader;
        //public ISiteContext GetSiteContext(Guid siteContextGuid)
        //{
        //    return _sitePluginLoader.GetSiteContext(siteContextGuid);
        //}
        //private readonly IBrowserLoader _browserLoader;
        //private readonly IIo _io;
        ////IEnumerable<ISiteContext> _siteContexts;
        //IEnumerable<SiteViewModel> _siteVms;
        //IEnumerable<BrowserViewModel> _browserVms;

        //Dictionary<ConnectionViewModel, MetadataViewModel> _metaDict = new Dictionary<ConnectionViewModel, MetadataViewModel>();
        //ConnectionSerializerLoader _connectionSerializerLoader = new ConnectionSerializerLoader("settings\\connections.txt");
        #endregion //Fields

        #region Methods
        private void ClearAllComments()
        {
            try
            {
                //Comments.Clear();
                //個別ユーザのコメントはどうしようか

            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        private async void ShowOptionsWindow()
        {
            var list = new List<IOptionsTabPage>();
            var mainOptionsPanel = new MainOptionsPanel();
            mainOptionsPanel.SetViewModel(new MainOptionsViewModel(_adapter.Options));
            list.Add(new MainTabPage("一般", mainOptionsPanel));
            var panels = await _adapter.RequestSettingsPanels();
            list.AddRange(panels.Select(kv => kv.Item2));
            WeakReferenceMessenger.Default.Send(new ShowOptionsViewMessage(list));
        }
        private void LogException(Exception ex)
        {

        }
        internal bool IsClose { get; set; } = false;
        private void Closing(CancelEventArgs e)
        {
            if (IsClose)
            {
                return;
            }
            _adapter.RequestCloseApp();
            e.Cancel = true;
            //try
            //{
            //    _connectionSerializerLoader.Save(Connections);
            //}
            //catch (Exception ex)
            //{
            //    LogException(ex);
            //    Debug.WriteLine(ex.Message);
            //}
            //foreach (var site in GetSiteContexts())
            //{
            //    try
            //    {
            //        var path = GetSiteOptionsPath(site.DisplayName);
            //        site.SaveOptions(path, _io);
            //    }
            //    catch (Exception ex)
            //    {
            //        LogException(ex);
            //        Debug.WriteLine(ex.Message);
            //    }
            //}
            //try
            //{
            //    _pluginManager?.OnClosing();
            //}
            //catch (Exception ex)
            //{
            //    LogException(ex);
            //    Debug.WriteLine(ex.Message);
            //}

            //try
            //{
            //    _sitePluginLoader.Save();
            //}
            //catch (Exception ex)
            //{
            //    LogException(ex);
            //    Debug.WriteLine(ex.Message);
            //}
        }
        private void RemoveSelectedConnection()
        {
            var selectedConnections = ConnectionsVm.GetSelectedConnections();
            _adapter.RemoveConnections(selectedConnections);
            //try
            //{
            //    var toRemove = Connections.Where(conn => conn.IsSelected).ToList();
            //    foreach (var conn in toRemove)
            //    {
            //        Connections.Remove(conn);
            //        var meta = _metaDict[conn];
            //        _metaDict.Remove(conn);
            //        MetaCollection.Remove(meta);
            //        OnConnectionDeleted(conn.ConnectionName);
            //    }
            //    //TODO:この接続に関連するコメントも全て消したい

            //}
            //catch (Exception ex)
            //{
            //    LogException(ex);
            //}
        }
        //private void SetSystemInfo(string message, InfoType type)
        //{
        //    //var context = InfoMessageContext.Create(new InfoMessage
        //    //{
        //    //    Text = message,
        //    //    SiteType = SiteType.Unknown,
        //    //    Type = type,
        //    //}, _options);
        //    //AddComment(context, null);
        //}

        private void RequestAddNewConnection()
        {
            _adapter.RequestAddConnection();
        }
        private void AddConnection(IAdapter adapter, IConnectionStatus connSt, SiteViewModel selectedSite, BrowserViewModel selectedBrowser)
        {
            var name = new ConnectionName(adapter, connSt.Id) { BackColor = Colors.White, ForeColor = Colors.Black };
            //name.SetName(connSt.Name);
            var vm = new ConnectionViewModel(_adapter, connSt, Sites, Browsers, selectedSite, selectedBrowser, name);

            _connDict.Add(connSt.Id, vm);
            _connNameDict.Add(connSt.Id, name);
            ConnectionsVm.AddConnection(vm);

            var metaVm = new MetadataViewModel(name);
            _metaDict.Add(connSt.Id, metaVm);
            MetaCollection.Add(metaVm);
        }

        private void OnConnectionAdded(ConnectionId connId, IConnectionStatus connSt, BrowserProfileId browserProfileId)
        {
            var selectedSite = GetSiteViewModel(connSt.SelectedSite);
            var browser = _browserDict[browserProfileId];
            AddConnection(_adapter, connSt, selectedSite, browser);
        }

        private void OnConnectionRemoved(ConnectionId connId)
        {
            var vm = _connDict[connId];
            _connDict.Remove(connId);
            ConnectionsVm.RemoveConnection(vm);

            var metaVm = _metaDict[connId];
            _metaDict.Remove(connId);
            MetaCollection.Remove(metaVm);
        }
        private void OnConnectionStatusChanged(ConnectionId connId, IConnectionStatusDiff connStDiff)
        {
            var conn = _connDict[connId];
            if (connStDiff.Name != null)
            {
                var name = _connNameDict[connId];
                name.OnNameChanged();
            }
            if (connStDiff.SelectedSite != null)
            {
                var site = _siteDict[connStDiff.SelectedSite];
                conn.SetSelectedSite(site);
            }
            if (connStDiff.SelectedBrowser != null)
            {
                var browser = _browserDict[connStDiff.SelectedBrowser];
                conn.SetSelectedBrowser(browser);
            }
            if (connStDiff.IsConnected.HasValue)
            {
                var isConnected = connStDiff.IsConnected.Value;
                //conn.CanConnect = !isConnected;
                //conn.CanDisconnect = isConnected;
            }
            if (connStDiff.CanConnect.HasValue)
            {
                conn.SetCanConnect(connStDiff.CanConnect.Value);
            }
            if (connStDiff.CanDisconnect.HasValue)
            {
                conn.SetCanDisconnect(connStDiff.CanDisconnect.Value);
            }
        }

        private readonly ConcurrentDictionary<PluginId, SiteViewModel> _siteDict = new();
        private readonly ConcurrentDictionary<BrowserProfileId, BrowserViewModel> _browserDict = new();
        private readonly Dictionary<ConnectionId, ConnectionViewModel> _connDict = new();
        private readonly Dictionary<ConnectionId, ConnectionName> _connNameDict = new();
        private readonly Dictionary<ConnectionId, MetadataViewModel> _metaDict = new();

        private SiteViewModel GetSiteViewModel(PluginId id)
        {
            return _siteDict[id];
        }
        private BrowserViewModel GetBrowserViewModel(BrowserProfileId id)
        {
            return _browserDict[id];
        }
        private async Task CheckIfUpdateExists(bool isAutoCheck)
        {
            await Task.CompletedTask;
        }
        #endregion //Methods
        public event EventHandler<EventArgs>? CloseRequested;
        public void RequestClose()
        {
            OnCloseRequested(EventArgs.Empty);
        }

        protected virtual void OnCloseRequested(EventArgs e)
        {
            CloseRequested?.Invoke(this, e);
        }
        #region Properties
        public ObservableCollection<MetadataViewModel> MetaCollection { get; } = new();
        public ObservableCollection<PluginMenuItemViewModel> PluginMenuItemCollection { get; } = new();
        private readonly ObservableCollection<IMcvCommentViewModel> _comments = new();


        private ConnectionViewModel _selectedConnection;
        public ConnectionViewModel SelectedConnection
        {
            get { return _selectedConnection; }
            set
            {
                _selectedConnection = value;
                //if (_selectedConnection == null)
                //{
                //    MessengerInstance.Send(new SetPostCommentPanel(null));
                //}
                //else
                //{
                //    MessengerInstance.Send(new SetPostCommentPanel(_selectedConnection.CommentPostPanel));
                //}
                RaisePropertyChanged();
            }
        }
        private Task<string> GetVersionNumber()
        {
            return _adapter.GetVersion();
        }
        private Task<string> GetAppName()
        {
            return _adapter.GetAppName();
        }
        private Task<string> GetAppSolutionConfiguration()
        {
            return _adapter.GetAppSolutionConfiguration();
        }
        private async void GetTitleAsync()
        {
            var name = await GetAppName();
            var version = await GetVersionNumber();
            var conf = await GetAppSolutionConfiguration();
            var title = $"{name} v{version} ({conf})";
            if (title != _title)
            {
                _title = title;
                RaisePropertyChanged(nameof(Title));
            }
        }
        private string _title = "";
        public string Title
        {
            get
            {
                GetTitleAsync();
                return _title;
            }
        }


        private readonly Color _myColor = new Color { A = 0xFF, R = 45, G = 45, B = 48 };
        private readonly IAdapter _adapter;
        #endregion //Properties
        public MainViewModel()
        {
            if (!DesignModeUtils.IsDesignMode)
            {
                throw new NotSupportedException();
            }
        }
        private void SendException(Exception ex)
        {

        }
        public CommentDataGridViewModel CommentVm { get; }
        public ConnectionsViewModel ConnectionsVm { get; }
        private readonly Dispatcher _dispatcher;
        public MainViewModel(IAdapter adapter)
        {
            var collectionView = CollectionViewSource.GetDefaultView(_comments);
            CommentVm = new CommentDataGridViewModel(adapter.Options, collectionView);
            ConnectionsVm = new ConnectionsViewModel(adapter);
            //_io = io;
            //_logger = logger;
            //_sitePluginLoader = sitePluginLoader;
            //_browserLoader = browserLoader;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _adapter = adapter;
            _adapter.ConnectionAdded += (s, e) =>
            {
                try
                {
                    OnConnectionAdded(e.ConnSt.Id, e.ConnSt, e.Browser);
                }
                catch (Exception ex)
                {
                    SendException(ex);
                }
            };
            _adapter.ConnectionRemoved += (s, e) =>
            {
                OnConnectionRemoved(e.ConnId);
            };
            _adapter.ConnectionStatusChanged += (s, e) =>
            {
                try
                {
                    OnConnectionStatusChanged(e.ConnStDiff.Id, e.ConnStDiff);
                }
                catch (Exception ex)
                {
                    SendException(ex);
                }
            };
            _adapter.SiteAdded += (s, e) =>
            {
                try
                {
                    OnSiteAdded(e.SiteId, e.SiteDisplayName);
                }
                catch (Exception ex)
                {
                    SendException(ex);
                }
            };
            _adapter.BrowserAdded += (s, e) =>
            {
                try
                {
                    OnBrowserAdded(e.BrowserProfileId, e.BrowserDisplayName, e.ProfileDisplayName);
                }
                catch (Exception ex)
                {
                    SendException(ex);
                }
            };
            _adapter.BrowserRemoved += (s, e) =>
            {
                try
                {
                    OnBrowserRemoved(e.BrowserProfileId);
                }
                catch (Exception ex)
                {
                    SendException(ex);
                }
            };
            _adapter.MessageReceived += (s, e) =>
            {
                try
                {
                    OnMessageReceived(e.MessageReceived.ConnectionId, e.MessageReceived.Message, e.User);
                }
                catch (Exception ex)
                {
                    SendException(ex);
                }
            };
            _adapter.MetadataUpdated += (s, e) =>
            {
                var newMeta = e.MetadataUpdated.Metadata;
                var connId = e.MetadataUpdated.ConnId;
                try
                {
                    var metaVm = _metaDict[connId];

                    if (newMeta.Title != null)
                    {
                        metaVm.Title = newMeta.Title;
                    }
                    if (newMeta.Elapsed != null)
                    {
                        metaVm.Elapsed = newMeta.Elapsed;
                    }
                    if (newMeta.CurrentViewers != null)
                    {
                        metaVm.CurrentViewers = newMeta.CurrentViewers;
                    }
                    if (newMeta.TotalViewers != null)
                    {
                        metaVm.TotalViewers = newMeta.TotalViewers;
                    }
                    if (newMeta.Active != null)
                    {
                        metaVm.Active = newMeta.Active;
                    }
                    if (newMeta.Others != null)
                    {
                        metaVm.Others = newMeta.Others;
                    }
                }
                catch (Exception ex)
                {
                    SendException(ex);
                }
            };
            _adapter.PluginAdded += (s, e) =>
            {
                try
                {
                    OnPluginAdded(e.PluginName, e.PluginId);
                }
                catch (Exception ex)
                {
                    SendException(ex);
                }
            };
            _adapter.SelectedSiteChanged += (s, e) =>
            {
                OnSelectedSiteChanged(e.ConnId, e.SiteId);
            };
            _adapter.UserAdded += (s, e) =>
            {
                var user = new UserViewModel(e.User, _adapter.Options);
                _users.Add(user);
            };
            _adapter.UserRemoved += (s, e) =>
            {
                var user = _users.SingleOrDefault(u => u.UserId == e.UserId);
                if (user is not null)
                {
                    _users.Remove(user);
                }
            };
            //_settingsContext = settingsContext;
            //settingsContext.Applied += (s, e) =>
            //{
            //    _adapter.OnSettingsApplied(e.ModifiedData);
            //};
            MainViewClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
            ShowOptionsWindowCommand = new RelayCommand(ShowOptionsWindow);
            ExitCommand = new RelayCommand(Exit);
            ShowWebSiteCommand = new RelayCommand(ShowWebSite);
            ShowDevelopersTwitterCommand = new RelayCommand(ShowDevelopersTwitter);
            CheckUpdateCommand = new RelayCommand(CheckUpdate);
            AddNewConnectionCommand = new RelayCommand(RequestAddNewConnection);
            RemoveSelectedConnectionCommand = new RelayCommand(RemoveSelectedConnection);
            ClearAllCommentsCommand = new RelayCommand(ClearAllComments);
            ShowUserInfoCommand = new RelayCommand(ShowUserInfo);
            ShowUserListCommand = new RelayCommand(ShowUserList);
        }

        private void OnSelectedSiteChanged(ConnectionId connId, PluginId selectedSite)
        {
            var conn = _connDict[connId];
            var site = _siteDict[selectedSite];
            conn.SetSelectedSite(site);
        }

        private void OnPluginAdded(string pluginName, PluginId pluginId)
        {
            var vm = new PluginMenuItemViewModel(pluginName, pluginId, _adapter);
            PluginMenuItemCollection.Add(vm);
        }
        readonly List<ILiveSiteMessageProcessor> _siteMessageProcessor = new()
        {
            new YouTubeMessageProcessor(),
            new TwitchMessageProcessor(),
            new BigoMessageProcessor(),
            new MildomMessageProcessor(),
            new MirrativMessageProcessor(),
            new NicoLiveMessageProcessor(),
            new MixchMessageProcessor(),
            new OpenrecMessageProcessor(),
            new ShowRoomMessageProcessor(),
            new TwicasMessageProcessor(),
            new WhowatchMessageProcessor(),
            new LineLiveMessageProcessor(),
        };
        private void OnMessageReceived(ConnectionId connectionId, ISiteMessage message, MyUser? user)
        {
            var connection = _connDict[connectionId];
            var connName = _connNameDict[connectionId];

            IMcvCommentViewModel? vm = null;
            foreach (var site in _siteMessageProcessor)
            {
                if (site.IsValidMessage(message))
                {
                    vm = site.CreateViewModel(message, connName, _adapter.Options, user);
                    break;
                }
            }
            if (vm != null)
            {
                OnMessageReceived(vm);
            }
        }
        private void OnMessageReceived(IMcvCommentViewModel vm)
        {
            _dispatcher.Invoke(() =>
            {
                _comments.Add(vm);
            });
        }
        private void OnBrowserRemoved(BrowserProfileId browserId)
        {
            var browser = GetBrowserViewModel(browserId);
            _browserDict.Remove(browser.Id, out var _);
            Browsers.Remove(browser);
        }
        private void OnBrowserAdded(BrowserProfileId browserId, string name, string? profileName)
        {
            var browser = new BrowserViewModel(browserId, name, profileName);
            _browserDict.TryAdd(browserId, browser);
            Browsers.Add(browser);
        }
        private void OnSiteRemoved(PluginId sitePluginId)
        {
            var site = GetSiteViewModel(sitePluginId);
            _siteDict.Remove(site.Id, out var _);
            Sites.Remove(site);
        }
        ObservableCollection<SiteViewModel> Sites { get; } = new ObservableCollection<SiteViewModel>();
        ObservableCollection<BrowserViewModel> Browsers { get; } = new ObservableCollection<BrowserViewModel>();
        private void OnSiteAdded(PluginId sitePluginId, string name)
        {
            if (_siteDict.ContainsKey(sitePluginId))
            {
                return;
            }
            var site = new SiteViewModel(sitePluginId, name);
            _siteDict.TryAdd(sitePluginId, site);
            Sites.Add(site);
        }
        private void ShowUserInfo()
        {

            var current = CommentVm.SelectedComment;
            if (current is null) return;
            var userId = current.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                Debug.WriteLine("UserIdがnull");
                return;
            }
            ShowUserInfo(userId, _comments, _adapter.Options, _adapter, this);
        }
        public UserViewModel GetUserVm(string userId)
        {
            return _userVmDict[userId];
        }
        private readonly Dictionary<string, UserViewModel> _userVmDict = new();
        public static void ShowUserInfo(string userId, ObservableCollection<IMcvCommentViewModel> comments, IMainViewPluginOptions options, IAdapter adapter, IUserViewModelProvider userProvider)
        {
            var user = adapter.GetUser(userId);
            var userVm = userProvider.GetUserVm(userId);
            var view = new CollectionViewSource { Source = comments }.View;
            view.Filter = obj =>
            {
                if (obj is not IMcvCommentViewModel cvm)
                {
                    return false;
                }
                return cvm.UserId == userId;
            };
            var userInfoVm = new UserInfoViewModel(view, userVm, options);
            WeakReferenceMessenger.Default.Send(new ShowUserInfoViewMessage(userInfoVm));
        }
        private void ShowUserList()
        {
            WeakReferenceMessenger.Default.Send(new ShowUserListViewMessage(new UserListViewModel(_users, _comments, _adapter.Options, _adapter, this)));
        }
        private readonly ObservableCollection<UserViewModel> _users = new();

        private async void CheckUpdate()
        {
            try
            {
                await CheckIfUpdateExists(false);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        private void ShowDevelopersTwitter()
        {
            try
            {
                Process.Start("https://twitter.com/kv510k");
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        private void ShowWebSite()
        {
            try
            {
                Process.Start("https://ryu-s.github.io/app/multicommentviewer");
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        private void Exit()
        {
            RequestClose();
        }
    }
    interface IUserViewModelProvider
    {
        UserViewModel GetUserVm(string userId);
    }
}
