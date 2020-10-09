using System;
using SitePlugin;
using Common;
using System.Diagnostics;
using System.Windows.Controls;
using SitePluginCommon;

namespace BigoSitePlugin
{
    public class BigoSiteContext : SiteContextBase, IBigoSiteContext
    {
        public override Guid Guid => new Guid("0BB9758A-D755-48B9-A102-E4D26F9A9591");

        public override string DisplayName => "Bigo";
        protected override SiteType SiteType => SiteType.Bigo;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new BigoOptionsPanel();
                panel.SetViewModel(new BigoOptionsViewModel(_siteOptions));
                return new BigoOptionsTabPage(DisplayName, panel);
            }
        }
        public override ICommentProvider CreateCommentProvider()
        {
            //return new YouTubeCommentProvider(connectionName, _options, _siteOptions);
            return new CommentProvider(_options, _server, _siteOptions, _logger, _userStoreManager)
            {
                SiteContextGuid = Guid,
            };
        }

        public override void LoadOptions(string path, IIo io)
        {
            _siteOptions = new BigoSiteOptions();
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

        public override void SaveOptions(string path, IIo io)
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

        public override bool IsValidInput(string input)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, "bigo\\.tv/\\d+");
        }
        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            var youtubeCommentProvider = commentProvider as CommentProvider;
            Debug.Assert(youtubeCommentProvider != null);
            if (youtubeCommentProvider == null)
                return null;

            var vm = new CommentPostPanelViewModel(youtubeCommentProvider, _logger);
            var panel = new CommentPostPanel
            {
                //IsEnabled = false,
                DataContext = vm
            };
            return panel;
        }

        private readonly ICommentOptions _options;
        private readonly IBigoServer _server;
        private readonly ILogger _logger;
        private BigoSiteOptions _siteOptions;
        public BigoSiteContext(ICommentOptions options, IBigoServer server, ILogger logger, IUserStoreManager userStoreManager)
            : base(options, userStoreManager, logger)
        {
            _options = options;
            _server = server;
            _logger = logger;
        }
    }
}
