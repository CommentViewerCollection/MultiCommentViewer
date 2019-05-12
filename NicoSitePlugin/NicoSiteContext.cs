using Common;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
namespace NicoSitePlugin
{
    public class NicoSiteContext : INicoSiteContext
    {
        public Guid Guid => new Guid("5A477452-FF28-4977-9064-3A4BC7C63252");
        public string DisplayName => "ニコ生";

        IOptionsTabPage ISiteContext.TabPanel
        {
            get
            {
                var panel = new NicoOptionsPanel();
                panel.SetViewModel(new NicoSiteOptionsViewModel(_siteOptions));
                return new NicoOptionsTabPage(DisplayName, panel);
            }
        }

        private INicoCommentProvider GetNicoCommentProvider()
        {
            return new NicoCommentProvider(_options, _siteOptions, _server, _logger, _userStoreManager)
            {
                SiteContextGuid = Guid,
            };
        }
        ICommentProvider ISiteContext.CreateCommentProvider()
        {
            return GetNicoCommentProvider();
        }
        private string GetOptionsPath(string dir)
        {
            return System.IO.Path.Combine(dir, DisplayName + ".txt");
        }
        void ISiteContext.LoadOptions(string path, IIo io)
        {
            _siteOptions = new NicoSiteOptions();
            try
            {
                var s = io.ReadFile(path);

                _siteOptions.Deserialize(s);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex, "", path);
            }
        }

        void ISiteContext.SaveOptions(string path, IIo io)
        {
            try
            {
                var s = _siteOptions.Serialize();
                io.WriteFile(path, s);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex, "", path);
            }
        }

        bool ISiteContext.IsValidInput(string input)
        {
            return NicoCommentProvider.IsValidInput(_options, _siteOptions, _userStoreManager, _server, _logger, null, input, Guid);
        }

        UserControl ISiteContext.GetCommentPostPanel(ICommentProvider commentProvider)
        {
            var nicoCommentProvider = commentProvider as INicoCommentProvider;
            Debug.Assert(nicoCommentProvider != null);
            if (nicoCommentProvider == null)
                return null;

            var vm = new CommentPostPanelViewModel(nicoCommentProvider, _logger);
            var panel = new CommentPostPanel
            {
                //IsEnabled = false,
                DataContext = vm
            };
            return panel;
        }
        INicoSiteOptions INicoSiteContext.GetNicoSiteOptions()
        {
            return _siteOptions;
        }
        INicoCommentProvider INicoSiteContext.CreateNicoCommentProvider()
        {
            return GetNicoCommentProvider();
        }
        public IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.NicoLive, userId);
        }
        public void Init()
        {
            _userStoreManager.Init(SiteType.NicoLive);
        }
        public void Save()
        {
            _userStoreManager.Save(SiteType.NicoLive);
        }
        protected virtual IUserStore CreateUserStore()
        {
            return new SQLiteUserStore(_options.SettingsDirPath + "\\" + "users_" + DisplayName + ".db", _logger);
        }
        private NicoSiteOptions _siteOptions;
        private readonly ICommentOptions _options;
        private readonly IDataSource _server;
        private readonly Func<string, int, int, ISplitBuffer, IStreamSocket> _streamSocketFactory;

        //private readonly Action<IStreamSocket> _streamSocketFactory;
        private readonly ILogger _logger;
        private readonly IUserStoreManager _userStoreManager;
        //private readonly IUserStore _userStore;

        public NicoSiteContext(ICommentOptions options, IDataSource server,Func<string,int,int,ISplitBuffer,IStreamSocket> StreamSocketFactory, ILogger logger, IUserStoreManager userStoreManager)
        {
            _options = options;
            _server = server;
            _streamSocketFactory = StreamSocketFactory;
            _logger = logger;
            _userStoreManager = userStoreManager;
            //_userStore = CreateUserStore();
            userStoreManager.SetUserStore(SiteType.NicoLive, new SQLiteUserStore(_options.SettingsDirPath + "\\" + "users_" + DisplayName + ".db", _logger));
        }
    }
}
