using Common;
using SitePlugin;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WhowatchSitePlugin
{
    public class WhowatchSiteContext : ISiteContext
    {
        private IWhowatchSiteOptions _siteOptions;
        private readonly ICommentOptions _options;
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly IUserStore _userStore;

        public Guid Guid => new Guid("EA695072-BABB-4FC9-AB9F-2F87D829AE7D");

        public string DisplayName => "ふわっち";
        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new TabPagePanel();
                panel.SetViewModel(new WhowatchSiteOptionsViewModel(_siteOptions));
                return new WhowatchOptionsTabPage(DisplayName, panel);
            }
        }

        public virtual ICommentProvider CreateCommentProvider()
        {
            return new WhowatchCommentProvider(_server, _options, _siteOptions, _userStore, _logger);
        }

        public System.Windows.Controls.UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            return null;
        }

        public void Init()
        {
            _userStore.Init();
        }

        public bool IsValidInput(string input)
        {
            return Tools.IsValidUrl(input);
        }
        protected virtual IWhowatchSiteOptions CreateWhowatchSiteOptions()
        {
            return new WhowatchSiteOptions();
        }
        public void LoadOptions(string path, IIo io)
        {
            _siteOptions = CreateWhowatchSiteOptions();
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

        public void Save()
        {
            _userStore.Save();
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
                _logger.LogException(ex, "", $"path={path}");
            }
        }
        protected virtual IUserStore CreateUserStore()
        {
            return new SQLiteUserStore(_options.SettingsDirPath + "\\" + "users_" + DisplayName + ".db", _logger);
        }
        protected virtual IDataServer CreateServer()
        {
            return new DataServer();
        }
        public WhowatchSiteContext(ICommentOptions options, ILogger logger)
        {
            _options = options;
            _server = CreateServer();
            _logger = logger;
            _userStore = CreateUserStore();
        }
    }
}
