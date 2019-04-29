using Common;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace PeriscopeSitePlugin
{
    public class PeriscopeSiteContext : ISiteContext
    {
        public Guid Guid => new Guid("FB468FFA-D0E5-4423-968C-5B9E1D258730");

        public string DisplayName => "Periscope";

        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new PeriscopeOptionsPanel();
                panel.SetViewModel(new PeriscopeSiteOptionsViewModel(_siteOptions));
                return new PeriscopeOptionsTabPage(DisplayName, panel);
            }
        }

        public virtual ICommentProvider CreateCommentProvider()
        {
            return new PeriscopeCommentProvider(_server, _logger, _options, _siteOptions, _userStoreManager)
            {
                SiteContextGuid = Guid,
            };
        }
        public IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType.NicoLive, userId);
        }
        private PeriscopeSiteOptions _siteOptions;
        public void LoadOptions(string path, IIo io)
        {
            _siteOptions = new PeriscopeSiteOptions();
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
            _userStoreManager.Init(SiteType.Periscope);
        }
        public void Save()
        {
            _userStoreManager.Save(SiteType.Periscope);
        }
        public bool IsValidInput(string input)
        {
            var liveId = Tools.ExtractLiveId(input);
            return !string.IsNullOrEmpty(liveId);
        }

        public UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            return null;
        }
        private readonly ICommentOptions _options;
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        private readonly IUserStoreManager _userStoreManager;
        public PeriscopeSiteContext(ICommentOptions options, IDataServer server, ILogger logger, IUserStoreManager userStoreManager)
        {
            _options = options;
            _server = server;
            _logger = logger;
            _userStoreManager = userStoreManager;
            _userStoreManager.SetUserStore(SiteType.Periscope, new SQLiteUserStore(_options.SettingsDirPath + "\\" + "users_" + DisplayName + ".db", _logger));
        }
    }
}
