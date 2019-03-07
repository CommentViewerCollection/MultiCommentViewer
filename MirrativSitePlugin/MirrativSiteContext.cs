using Common;
using SitePlugin;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace MirrativSitePlugin
{
    public class MirrativSiteContext : ISiteContext
    {
        public Guid Guid => new Guid("6DAFA768-280D-4E70-8494-FD5F31812EF5");

        public string DisplayName => "Mirrativ";

        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new TabPagePanel();
                panel.SetViewModel(new MirrativSiteOptionsViewModel(_siteOptions));
                return new MirrativOptionsTabPage(DisplayName, panel);
            }
        }

        public ICommentProvider CreateCommentProvider()
        {
            return new MirrativCommentProvider(_server, _logger, _options, _siteOptions, _userStore);
        }
        private MirrativSiteOptions _siteOptions;
        public void LoadOptions(string path, IIo io)
        {
            _siteOptions = new MirrativSiteOptions();
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
                _logger.LogException(ex, "", $"path={path}");
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
        public bool IsValidInput(string input)
        {
            return Tools.IsValidLiveId(input) || Tools.IsValidUserId(input);
        }
        public UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            return null;
        }
        protected virtual IUserStore CreateUserStore()
        {
            return new SQLiteUserStore(_options.SettingsDirPath + "\\" + "users_" + DisplayName + ".db", _logger);
        }
        private readonly ICommentOptions _options;
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly IUserStore _userStore;
        public MirrativSiteContext(ICommentOptions options, IDataServer server, ILogger logger)
        {
            _options = options;
            _server = server;
            _logger = logger;
            _userStore = CreateUserStore();
        }
    }
}
