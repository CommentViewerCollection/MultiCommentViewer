using Common;
using SitePlugin;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace LineLiveSitePlugin
{
    public class LineLiveSiteContext : ISiteContext
    {
        public Guid Guid => new Guid("36F139CA-EAB9-45B1-8CDC-AD47A4051BD3");

        public string DisplayName => "LINELIVE";

        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new LineLiveOptionsPanel();
                panel.SetViewModel(new LineLiveSiteOptionsViewModel(_siteOptions));
                return new LineLiveOptionsTabPage(DisplayName, panel);
            }
        }

        public virtual ICommentProvider CreateCommentProvider()
        {
            return new LineLiveCommentProvider(_server, _logger, _options, _siteOptions, _userStore);
        }
        private LineLiveSiteOptions _siteOptions;
        public void LoadOptions(string path, IIo io)
        {
            _siteOptions = new LineLiveSiteOptions();
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
        public IUser GetUser(string userId)
        {
            return _userStore.GetUser(userId);
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
            if (string.IsNullOrEmpty(input)) return false;
            //最低限チャンネルIDさえあればコメントを取れるようにした
            //https://live.line.me/channels/2354725/broadcast/8428420
            var b = Regex.IsMatch(input, "line\\.me/channels/\\d+");
            return b;
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
        public LineLiveSiteContext(ICommentOptions options, IDataServer server, ILogger logger)
        {
            _options = options;
            _server = server;
            _logger = logger;
            _userStore = CreateUserStore();
        }
    }
}
