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

        public ICommentProvider CreateCommentProvider(ConnectionName connectionName)
        {
            return new TwitchCommentProvider(connectionName, new TwitchServer(), _logger, _options, _siteOptions,_userStore, _dispatcher);
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
        public bool IsValidInput(string input)
        {
            //チャンネル名だけ来られても他のサイトのものの可能性があるからfalse
            //"twitch.tv/"の後に文字列があったらtrueとする。
            var b = Regex.IsMatch(input, "twitch\\.tv/[a-zA-Z0-9_]+");
            return b;
        }

        public UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            return null;
        }

        private readonly IOptions _options;
        private readonly ILogger _logger;
        private readonly IUserStore _userStore;
        private readonly Dispatcher _dispatcher;
        public TwitchSiteContext(IOptions options, ILogger logger, IUserStore userStore, Dispatcher dispatcher)
        {
            _options = options;
            _logger = logger;
            _userStore = userStore;
            _dispatcher = dispatcher;
        }
    }
}
