using System;
using SitePlugin;
using Common;
using System.Diagnostics;
using System.Windows.Controls;

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
        public void Init()
        {
            _userStore.Init();
        }
        public ICommentProvider CreateCommentProvider()
        {
            //return new YouTubeCommentProvider(connectionName, _options, _siteOptions);
            return new Test2.CommentProvider(_options, _server, _siteOptions, _logger, _userStore);
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
            var resolver = new VidResolver();
            return resolver.IsValidInput(input);
        }

        public UserControl GetCommentPostPanel(ICommentProvider commentProvider)
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
        protected virtual IUserStore CreateUserStore()
        {
            return new SQLiteUserStore(_options.SettingsDirPath + "\\" + "users_" + DisplayName + ".db", _logger);
        }

        public void Save()
        {
            _userStore.Save();
        }

        private readonly ICommentOptions _options;
        private readonly IYouTubeLibeServer _server;
        private readonly ILogger _logger;
        private readonly IUserStore _userStore;
        private Test2.YouTubeLiveSiteOptions _siteOptions;
        public YouTubeLiveSiteContext(ICommentOptions options, IYouTubeLibeServer server, ILogger logger)
        {
            _options = options;
            _server = server;
            _logger = logger;
            _userStore = CreateUserStore();
        }
    }
}
