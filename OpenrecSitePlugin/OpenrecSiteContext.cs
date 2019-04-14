using System;
using System.Linq;
using System.Text;
using SitePlugin;
using Common;
using System.Windows.Controls;
using System.Diagnostics;

namespace OpenrecSitePlugin
{
    public class OpenrecSiteContext : ISiteContext
    {
        public Guid Guid => new Guid("F4434012-3E68-4DD9-B2A8-F2BD7D601723");

        public string DisplayName => "OPENREC";

        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new OpenrecOptionsPanel();
                panel.SetViewModel(new OpenrecOptionsViewModel(_siteOptions));
                return new OpenrecOptionsTabPage(DisplayName, panel);
            }
        }

        public ICommentProvider CreateCommentProvider()
        {
            return new CommentProvider(_options, _siteOptions, _logger, _userStore);
        }

        public UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            var nicoCommentProvider = commentProvider as CommentProvider;
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

        public bool IsValidInput(string input)
        {
            return Tools.IsValidUrl(input);
        }

        public void LoadOptions(string path, IIo io)
        {
            _siteOptions = new OpenrecSiteOptions();
            try
            {
                var s = io.ReadFile(path);

                _siteOptions.Deserialize(s);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex, "", $"path={path}");
            }
        }

        public void SaveOptions(string path, IIo io)
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
        public void Init()
        {
            _userStore.Init();
        }
        public void Save()
        {
            _userStore.Save();
        }
        public IUser GetUser(string userId)
        {
            return _userStore.GetUser(userId);
        }
        protected virtual IUserStore CreateUserStore()
        {
            return new SQLiteUserStore(_options.SettingsDirPath + "\\" + "users_" + DisplayName + ".db", _logger);
        }
        private OpenrecSiteOptions _siteOptions;
        private ICommentOptions _options;
        private ILogger _logger;
        private IUserStore _userStore;

        public OpenrecSiteContext(ICommentOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
            _userStore = CreateUserStore();
        }
    }
}
