using System;
using SitePlugin;
using Common;
using System.Diagnostics;
using System.Windows.Controls;
using SitePluginCommon;

namespace YouTubeLiveSitePlugin.Test2
{
    public class YouTubeLiveSiteContext : SiteContextBase, IYouTubeSiteContext
    {
        public override Guid Guid => new Guid("F1631B64-6572-4530-ABAF-21707F15D893");

        public override string DisplayName => "YouTubeLive";
        protected override SiteType SiteType => SiteType.YouTubeLive;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new YouTubeLiveOptionsPanel();
                panel.SetViewModel(new YouTubeLiveOptionsViewModel(_siteOptions));
                return new YouTubeListOptionsTabPage(DisplayName, panel);
            }
        }
        public override ICommentProvider CreateCommentProvider()
        {
            //return new YouTubeCommentProvider(connectionName, _options, _siteOptions);
            //return new Test2.CommentProvider(_options, _server, _siteOptions, _logger, _userStoreManager)
            //{
            //    SiteContextGuid = Guid,
            //};
            return new Next.CommentProviderNext(_options, _server, _siteOptions, _logger, _userStoreManager)
            {
                SiteContextGuid = Guid,
            };
        }

        public override void LoadOptions(string path, IIo io)
        {
            _siteOptions = new YouTubeLiveSiteOptions();
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
            return VidResolver.IsValidInput(input);
        }
        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            var youtubeCommentProvider = commentProvider as Next.CommentProviderNext;
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
        private readonly IYouTubeLibeServer _server;
        private readonly ILogger _logger;
        private Test2.YouTubeLiveSiteOptions _siteOptions;
        public YouTubeLiveSiteContext(ICommentOptions options, IYouTubeLibeServer server, ILogger logger, IUserStoreManager userStoreManager)
            : base(options, userStoreManager, logger)
        {
            _options = options;
            _server = server;
            _logger = logger;
        }
    }
}
