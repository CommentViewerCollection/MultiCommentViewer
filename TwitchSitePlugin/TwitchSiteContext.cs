using System;
using System.Linq;
using System.Text;
using SitePlugin;
using System.Diagnostics;
using System.Windows.Threading;
using Common;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace TwitchSitePlugin
{
    public class TwitchSiteContext : ISiteContext
    {
        public Guid Guid => new Guid("22F7824A-EA1B-411E-85CA-6C9E6BE94E39");

        public string DisplayName => "Twitch";

        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new TabPagePanel();
                panel.SetViewModel(new TwitchSiteOptionsViewModel(_siteOptions));
                return new TwitchOptionsTabPage(DisplayName, panel);
            }
        }

        public ICommentProvider CreateCommentProvider()
        {
            return new TwitchCommentProvider(_server, _logger, _options, _siteOptions,_userStore);
        }
        private TwitchSiteOptions _siteOptions;
        public void LoadOptions(string path, IIo io)
        {
            _siteOptions = new TwitchSiteOptions();
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
            //チャンネル名だけ来られても他のサイトのものの可能性があるからfalse
            //"twitch.tv/"の後に文字列があったらtrueとする。
            var b = Regex.IsMatch(input, "twitch\\.tv/[a-zA-Z0-9_]+");
            return b;
        }
        public IUser GetUser(string userId)
        {
            return _userStore.GetUser(userId);
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
        public TwitchSiteContext(ICommentOptions options, IDataServer server, ILogger logger)
        {
            _options = options;
            _server = server;
            _logger = logger;
            _userStore = CreateUserStore();
        }
    }
}
