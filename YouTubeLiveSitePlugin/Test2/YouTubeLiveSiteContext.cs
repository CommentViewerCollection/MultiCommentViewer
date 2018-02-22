using System;
using SitePlugin;
using Common;
using System.Diagnostics;
namespace YouTubeLiveSitePlugin.Test2
{
    public class YouTubeLiveSiteContext : IYouTubeSiteContext
    {
        public Guid Guid => new Guid("F1631B64-6572-4530-ABAF-21707F15D893");

        public string DisplayName => "YouTubeLive";

        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new YouTubeLiveOptionsPanel();
                panel.SetViewModel(new YouTubeLiveOptionsViewModel(_siteOptions));
                return new YouTubeListOptionsTabPage(DisplayName, panel);
            }
        }

        public ICommentProvider CreateCommentProvider(ConnectionName connectionName)
        {
            //return new YouTubeCommentProvider(connectionName, _options, _siteOptions);
            return new Test2.CommentProvider(connectionName, _options, _siteOptions, _logger);
        }

        public void LoadOptions(string path, IIo io)
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
            return Tools.TryGetVid(input, out string vid);
        }

        private readonly IOptions _options;
        private readonly ILogger _logger;
        private Test2.YouTubeLiveSiteOptions _siteOptions;
        public YouTubeLiveSiteContext(IOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
        }
    }
}
