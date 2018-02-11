using System;
using SitePlugin;
using Common;
namespace YouTubeLiveSitePlugin.Old
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
            return new YouTubeCommentProvider(connectionName, _options, _siteOptions);
        }

        public void LoadOptions(string siteOptionsStr, IIo io)
        {
            _siteOptions = new YouTubeSiteOptions();
        }

        public void SaveOptions(string path, IIo io)
        {

        }
        private readonly IOptions _options;
        private YouTubeSiteOptions _siteOptions;
        public YouTubeLiveSiteContext(IOptions options)
        {
            _options = options;
        }
    }


}
