using System;
using System.Linq;
using System.Text;
using SitePlugin;
using Common;
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
            return new TwitchCommentProvider(connectionName, new TwitchServer(), _logger, _options, _siteOptions);
        }
        private TwitchSiteOptions _siteOptions;
        public void LoadOptions(string dir, IIo io)
        {
            _siteOptions = new TwitchSiteOptions();
        }

        public void SaveOptions(string path, IIo io)
        {
            throw new NotImplementedException();
        }
        private readonly IOptions _options;
        private readonly ILogger _logger;
        public TwitchSiteContext(IOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
        }
    }
}
