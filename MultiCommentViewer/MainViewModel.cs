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

//TODO:過去コメントの取得


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
        #endregion //Commands

        #region Fields
        IOptions _options;
        IEnumerable<ISiteContext> _siteContexts;
        IEnumerable<SiteViewModel> _siteVms;
        IEnumerable<BrowserViewModel> _browserVms;

        IEnumerable<IPlugin> _plugins;
        ISitePluginManager _siteManager;
        private readonly Dispatcher _dispatcher;
        #endregion //Fields


        #region Methods
        private void ShowOptionsWindow()
        {
            MessengerInstance.Send(new ShowOptionsViewMessage(_siteContexts.Select(site => site.TabPanel)));
        }
        private void ContentRendered()
        {
            try
            {


                //ISitePluginManager _siteManager = DiContainer.Instance.GetNewInstance<ISitePluginManager>();
                //_siteManager.LoadSitePlugins(_options);
                ISitePluginLoader sitePluginLoader = DiContainer.Instance.GetNewInstance<ISitePluginLoader>();
                _siteContexts = sitePluginLoader.LoadSitePlugins(_options);
                foreach (var site in _siteContexts)
                {
                    site.LoadOptions(_options.SettingsDirPath);
                }
                _siteVms = _siteContexts.Select(c => new SiteViewModel(c));

                IBrowserLoader browserLoader = DiContainer.Instance.GetNewInstance<IBrowserLoader>();
                _browserVms = browserLoader.LoadBrowsers().Select(b => new BrowserViewModel(b));
                //もしブラウザが無かったらclass EmptyBrowserProfileを使う。
                if (_browserVms.Count() == 0)
                {
                    _browserVms = new List<BrowserViewModel>
                    {
                        new BrowserViewModel( new EmptyBrowserProfile()),
                    };
                }
            }
            catch (Exception ex)
            {
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
                Debug.WriteLine(ex.Message);
            }
        }
        #endregion //Methods


        
        public ICommand AddNewConnectionCommand { get; }
        private void AddNewConnection()
        {
            try
            {
                var connectionName = new ConnectionName();//TODO:一意の名前を設定
                var connection = new ConnectionViewModel(connectionName, _siteVms, _browserVms);
                connection.CommentsReceived += Connection_CommentsReceived;
                connection.MetadataReceived += Connection_MetadataReceived;
                var metaVm = new MetadataViewModel(connectionName);
                _metaDict.Add(connection, metaVm);
                MetaCollection.Add(metaVm);
                Connections.Add(connection);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debugger.Break();
            }
        }


        #region Properties
        public ObservableCollection<ICommentViewModel> Comments { get; } = new ObservableCollection<ICommentViewModel>();
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();
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
        #endregion
        private async void Connection_CommentsReceived(object sender, List<ICommentViewModel> e)
        {
            //TODO:Comments.AddRange()が欲しい
            await _dispatcher.BeginInvoke((Action)(() =>
            {
                foreach (var comment in e)
                {
                    Comments.Add(comment);
                }
            }), DispatcherPriority.Normal);
        }
        Dictionary<ConnectionViewModel, MetadataViewModel> _metaDict = new Dictionary<ConnectionViewModel, MetadataViewModel>();
        public ObservableCollection<MetadataViewModel> MetaCollection { get; } = new ObservableCollection<MetadataViewModel>();

        private void Connection_MetadataReceived(object sender, IMetadata e)
        {
            if (sender is ConnectionViewModel connection)
            {
                var metaVm = _metaDict[connection];
                if (e.Title != null)
                    metaVm.Title = e.Title;
                if (e.Active != null)
                    metaVm.Active = e.Active;
            }
        }
        
        

        public MainViewModel()
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            //読み込み
            IOptionsLoader optionsLoader = DiContainer.Instance.GetNewInstance<IOptionsLoader>();
            _options = optionsLoader.LoadOptions();

            MainViewContentRenderedCommand = new RelayCommand(ContentRendered);
            MainViewClosingCommand = new RelayCommand(Closing);
            ShowOptionsWindowCommand = new RelayCommand(ShowOptionsWindow);
            ExitCommand = new RelayCommand(Exit);
            ShowWebSiteCommand = new RelayCommand(ShowWebSite);
            ShowDevelopersTwitterCommand = new RelayCommand(ShowDevelopersTwitter);
            CheckUpdateCommand = new RelayCommand(CheckUpdate);
            AddNewConnectionCommand = new RelayCommand(AddNewConnection);
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

}
