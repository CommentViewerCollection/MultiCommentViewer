using System;
using System.Diagnostics;
using System.Windows.Controls;
using Mcv.PluginV2;

namespace Mcv.YouTubeLiveSitePlugin
{
    public class YouTubeLiveSiteContext : SiteContextBase, IYouTubeSiteContext
    {
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
            return new CommentProviderNext(_server, _siteOptions, _logger);
        }
        public override void LoadOptions(string rawOptions)
        {
            _siteOptions = new YouTubeLiveSiteOptions();
            try
            {
                _siteOptions.Deserialize(rawOptions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex, "", "");
            }
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
        public override string GetSiteOptions()
        {
            return _siteOptions.Serialize();
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
            var youtubeCommentProvider = commentProvider as CommentProviderNext;
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

        private readonly IYouTubeLiveServer _server;
        private readonly ILogger _logger;
        private YouTubeLiveSiteOptions _siteOptions;
        public YouTubeLiveSiteContext(IYouTubeLiveServer server, ILogger logger)
            : base(logger)
        {
            _server = server;
            _logger = logger;
        }
    }
}
