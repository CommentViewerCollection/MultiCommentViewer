using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using SitePlugin;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Common;
using Plugin;

using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MultiCommentViewer
{
    public class ConnectionContext
    {
        public ConnectionName ConnectionName { get; set; }
        public ICommentProvider CommentProvider { get; set; }
        public Guid SiteGuid { get; set; }
    }
    public class SelectedSiteChangedEventArgs : EventArgs
    {
        public ConnectionName ConnectionName { get; set; }
        public ConnectionContext OldValue { get; set; }
        public ConnectionContext NewValue { get; set; }
    }
    public class ConnectionViewModel : ViewModelBase, IConnectionStatus
    {
        public ConnectionName ConnectionName => _connectionName;
        public string Name
        {
            get { return _connectionName.Name; }
            set { _connectionName.Name = value; }
        }
        public ICommentProvider CommentProvider => _commentProvider;

        string IConnectionStatus.Guid => Guid.ToString();
        #region IsSelected
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                RaisePropertyChanged();
            }
        }
        #endregion //IsSelected
        public ObservableCollection<SiteViewModel> Sites { get; }
        public ObservableCollection<BrowserViewModel> Browsers { get; }
        private SiteViewModel _selectedSite;
        private ICommentProvider _commentProvider = null;
        private readonly ConnectionName _connectionName;
        public ICommand ConnectCommand { get; }
        public ICommand DisconnectCommand { get; }
        public event EventHandler<SelectedSiteChangedEventArgs> SelectedSiteChanged;

        private ConnectionContext _beforeContext;
        private ConnectionContext _currentContext;
        public SiteViewModel SelectedSite
        {
            get { return _selectedSite; }
            set
            {
                if (_selectedSite == value)
                    return;
                //一番最初は_commentProviderはnull
                var before = _commentProvider;
                if (before != null)
                {
                    Debug.Assert(before.CanConnect, "接続中に変更はできない");
                    before.CanConnectChanged -= CommentProvider_CanConnectChanged;
                    before.CanDisconnectChanged -= CommentProvider_CanDisconnectChanged;
                    before.CommentReceived -= CommentProvider_CommentReceived;
                    before.InitialCommentsReceived -= CommentProvider_InitialCommentsReceived;
                    before.MetadataUpdated -= CommentProvider_MetadataUpdated;
                    before.Connected -= CommentProvider_Connected;
                }
                _selectedSite = value;
                var nextGuid = _selectedSite.Guid;
                var next = _commentProvider = _sitePluginLoader.CreateCommentProvider(nextGuid);
                next.CanConnectChanged += CommentProvider_CanConnectChanged;
                next.CanDisconnectChanged += CommentProvider_CanDisconnectChanged;
                next.CommentReceived += CommentProvider_CommentReceived;
                next.InitialCommentsReceived += CommentProvider_InitialCommentsReceived;
                next.MetadataUpdated += CommentProvider_MetadataUpdated;
                next.Connected += CommentProvider_Connected;
                UpdateLoggedInInfo();

                System.Windows.Controls.UserControl commentPanel;
                try
                {
                    commentPanel = _sitePluginLoader.GetCommentPostPanel(nextGuid, next);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                    commentPanel = null;
                }
                CommentPostPanel = commentPanel;

                _beforeContext = _currentContext;
                _currentContext = new ConnectionContext
                {
                     ConnectionName = this.ConnectionName,
                      CommentProvider = next,
                       SiteGuid = nextGuid,
                       //SiteContext = _selectedSite.Site,
                };
                RaisePropertyChanged();
                SelectedSiteChanged?.Invoke(this, new SelectedSiteChangedEventArgs
                {
                    ConnectionName = this.ConnectionName,
                    OldValue = _beforeContext,
                    NewValue = _currentContext
                });
            }
        }
        private async void UpdateLoggedInInfo()
        {
            var cp = _commentProvider;
            if(cp == null)
            {
                return;
            }
            SitePlugin.ICurrentUserInfo currentUserInfo = null;
            var br = SelectedBrowser;
            if(br != null)
            {
                _dispatcher.Invoke(()=>
                {
                    LoggedInUsername = "";
                });
                try
                {
                    currentUserInfo = await cp.GetCurrentUserInfo(br.Browser);
                }
                catch (Exception ex)
                {
                    _logger.LogException(ex);
                }
            }
            _dispatcher.Invoke(() =>
            {
                //UpdateLoggedInInfo()がasync voidだから自分より後に実行されたものが既にLoggedInUsernameをセットしている可能性がある。そのためcpとbrに変更が無いか確認する必要がある。
                if (cp == _commentProvider && br == SelectedBrowser)
                {
                    if (currentUserInfo == null)
                    {
                        LoggedInUsername = "";
                    }
                    else
                    {
                        LoggedInUsername = currentUserInfo.IsLoggedIn ? currentUserInfo.Username : "(未ログイン)";
                    }
                }

            });
        }
        /// <summary>
        /// 配信者のユーザIDとかコミュニティIDのような毎回そこからリアルタイムの配信に接続できる文字列であるか
        /// 配信IDだと毎回変わるため保存しても無意味。
        /// </summary>
        public bool IsInputStoringNeeded { get; private set; }
        /// <summary>
        /// 保存して次回起動時にリストアする文字列
        /// </summary>
        public string UrlToRestore { get; private set; }

        private string _LoggedInUsername;
        public string LoggedInUsername
        {
            get => _LoggedInUsername;
            set
            {
                _LoggedInUsername = value;
                RaisePropertyChanged();
            }
        }
        private void CommentProvider_Connected(object sender, ConnectedEventArgs e)
        {
            IsInputStoringNeeded = e.IsInputStoringNeeded;
            UrlToRestore = e.UrlToRestore;
        }

        private System.Windows.Controls.UserControl _commentPostPanel;
        public System.Windows.Controls.UserControl CommentPostPanel
        {
            get { return _commentPostPanel; }
            set
            {
                if (_commentPostPanel == value) return;
                _commentPostPanel = value;
                RaisePropertyChanged();
            }
        }

        private void CommentProvider_MetadataUpdated(object sender, IMetadata e)
        {
            MetadataReceived?.Invoke(this, e);//senderはConnection
        }
        public event EventHandler<RenamedEventArgs> Renamed;
        public event EventHandler<ICommentViewModel> CommentReceived;
        public event EventHandler<List<ICommentViewModel>> InitialCommentsReceived;
        public event EventHandler<IMetadata> MetadataReceived;
        private void CommentProvider_CommentReceived(object sender, ICommentViewModel e)
        {
            CommentReceived?.Invoke(this, e);
        }
        private void CommentProvider_InitialCommentsReceived(object sender, List<ICommentViewModel> e)
        {
            InitialCommentsReceived?.Invoke(this, e);
        }
        private void CommentProvider_CanDisconnectChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(CanDisconnect));
        }

        private void CommentProvider_CanConnectChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(CanConnect));
        }

        private BrowserViewModel _selectedBrowser;
        public BrowserViewModel SelectedBrowser
        {
            get { return _selectedBrowser; }
            set
            {
                if (_selectedBrowser == value)
                    return;
                _selectedBrowser = value;
                UpdateLoggedInInfo();
                RaisePropertyChanged();
            }
        }
        public bool CanConnect
        {
            get { return _commentProvider.CanConnect; }
        }
        public bool CanDisconnect
        {
            get { return _commentProvider.CanDisconnect; }
        }
        private bool _needSave;
        /// <summary>
        /// ユーザがこのConnectionの情報の保存を要求しているか
        /// </summary>
        public bool NeedSave
        {
            get => _needSave;
            set
            {
                _needSave = value;
                RaisePropertyChanged();
            }
        }
        private string _input;
        public string Input
        {
            get { return _input; }
            set
            {
                if (_input == value)
                    return;
                _input = value;

                var guid = _sitePluginLoader.GetValidSiteGuid(value);
                if (guid != Guid.Empty)
                {
                    var vm = _siteVmDict[guid];
                    SelectedSite = vm;
                }
            }
        }
        /// <summary>
        /// 自動サイト選択機能が無い版
        /// </summary>
        public string InputWithNoAutoSiteSelect
        {
            get => _input;
            set
            {
                if (_input == value)
                    return;
                _input = value;
                RaisePropertyChanged();
            }
        }

        public ConnectionContext GetCurrent()
        {
            var context = new ConnectionContext { ConnectionName = this.ConnectionName, SiteGuid= SelectedSite.Guid, CommentProvider = _commentProvider };
            return context;
        }

        private async void Connect()
        {
            try
            {
                //接続中は削除できないように選択を外す
                IsSelected = false;
                var input = Input;
                var browser = SelectedBrowser.Browser;
                await _commentProvider.ConnectAsync(input, browser);


            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
            }
        }
        private void Disconnect()
        {
            try
            {
                _commentProvider.Disconnect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex);
            }
        }
        string _beforeName;
        private readonly ILogger _logger;
        private readonly ISitePluginLoader _sitePluginLoader;
        //private readonly IEnumerable<ISiteContext> _sites;
        private readonly Dictionary<Guid, SiteViewModel> _siteVmDict = new Dictionary<Guid, SiteViewModel>();
        /// <summary>
        /// ConnectionNameは重複可だから一意識別のために必要
        /// </summary>
        public Guid Guid { get; }
        private readonly Dispatcher _dispatcher;
        public ConnectionViewModel(ConnectionName connectionName, IEnumerable<SiteViewModel> sites, IEnumerable<BrowserViewModel> browsers, ILogger logger, ISitePluginLoader sitePluginLoader)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            Guid = Guid.NewGuid();
            _logger = logger;
            _sitePluginLoader = sitePluginLoader;
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _beforeName = _connectionName.Name;
            _connectionName.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(_connectionName.Name):
                        var newName = _connectionName.Name;
                        Renamed?.Invoke(this, new RenamedEventArgs(_beforeName, newName));
                        _beforeName = newName;
                        RaisePropertyChanged(nameof(Name));
                        break;
                }
            };

            if (sites == null)
            {
                throw new ArgumentNullException(nameof(sites));
            }
            Sites = new ObservableCollection<SiteViewModel>();
            foreach (var siteVm in sites)
            {
                _siteVmDict.Add(siteVm.Guid, siteVm);
                Sites.Add(siteVm);
            }
            //Sites = new ObservableCollection<SiteViewModel>(sites);
            if (Sites.Count > 0)
            {
                SelectedSite = Sites[0];
            }

            Browsers = new ObservableCollection<BrowserViewModel>(browsers);
            if (Browsers.Count > 0)
            {
                SelectedBrowser = Browsers[0];
            }
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
        }
    }
    public class RenamedEventArgs : EventArgs
    {
        public string NewValue { get; }
        public string OldValue { get; }
        public RenamedEventArgs(string oldValue, string newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
