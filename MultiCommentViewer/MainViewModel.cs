using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using SitePlugin;
using System.Collections.ObjectModel;
using Plugin;
using ryu_s.BrowserCookie;
using System.Diagnostics;
using System.Net;
using System.Windows.Media;

//TODO:過去コメントの取得


namespace MultiCommentViewer
{
    public class MainViewModel: ViewModelBase
    {
        public ObservableCollection<ConnectionViewModel> Connections { get; } = new ObservableCollection<ConnectionViewModel>();
        RelayCommand _showOptionsWindowCommand;
        public ICommand ShowOptionsWindowCommand
        {
            get
            {
                if(_showOptionsWindowCommand == null)
                {
                    _showOptionsWindowCommand = new RelayCommand(() =>
                    {
                        MessengerInstance.Send(new ShowOptionsViewMessage(_siteContexts.Select(site=>site.TabPanel)));
                    });
                }
                return _showOptionsWindowCommand;
            }
        }
        RelayCommand _AddNewConnectionCommand;
        public ICommand AddNewConnectionCommand
        {
            get
            {
                if(_AddNewConnectionCommand == null)
                {
                    _AddNewConnectionCommand = new RelayCommand(() =>
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
                    });
                }
                return _AddNewConnectionCommand;
            }
        }



        #region Properties
        public ObservableCollection<ICommentViewModel> Comments { get; } = new ObservableCollection<ICommentViewModel>();
        
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
        #endregion
        private void Connection_CommentsReceived(object sender, List<ICommentViewModel> e)
        {
            //TODO:Comments.AddRange()が欲しい
            foreach (var comment in e)
            {
                Comments.Add(comment);
            }
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

        RelayCommand _MainViewContentRenderedCommand;
        public ICommand MainViewContentRenderedCommand
        {
            get
            {
                if (_MainViewContentRenderedCommand == null)
                {
                    _MainViewContentRenderedCommand = new RelayCommand(ExecuteContentRenderedCommand);
                }
                return _MainViewContentRenderedCommand;
            }
        }
        IOptions _options;
        IEnumerable<ISiteContext> _siteContexts;
        IEnumerable<SiteViewModel> _siteVms;
        IEnumerable<BrowserViewModel> _browserVms;

        IEnumerable<IPlugin> _plugins;
        ISitePluginManager _siteManager;
        private void ExecuteContentRenderedCommand()
        {
            try
            {
                

                //ISitePluginManager _siteManager = DiContainer.Instance.GetNewInstance<ISitePluginManager>();
                //_siteManager.LoadSitePlugins(_options);
                ISitePluginLoader sitePluginLoader = DiContainer.Instance.GetNewInstance<ISitePluginLoader>();
                _siteContexts = sitePluginLoader.LoadSitePlugins(_options);
                foreach(var site in _siteContexts)
                {
                    site.LoadOptions(_options.SettingsDirPath);
                }
                _siteVms = _siteContexts.Select(c => new SiteViewModel(c));

                IBrowserLoader browserLoader = DiContainer.Instance.GetNewInstance<IBrowserLoader>();
                _browserVms = browserLoader.LoadBrowsers().Select(b => new BrowserViewModel(b));
                //もしブラウザが無かったらclass EmptyBrowserProfileを使う。
                if(_browserVms.Count() == 0)
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

        public MainViewModel()
        {
            //読み込み
            IOptionsLoader optionsLoader = DiContainer.Instance.GetNewInstance<IOptionsLoader>();
            _options = optionsLoader.LoadOptions();
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
