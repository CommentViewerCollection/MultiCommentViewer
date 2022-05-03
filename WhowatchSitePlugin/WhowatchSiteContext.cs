using Mcv.PluginV2;
using System;
using System.Diagnostics;

namespace WhowatchSitePlugin
{
    public class WhowatchSiteContext : SiteContextBase
    {
        private IWhowatchSiteOptions _siteOptions;
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        public override string DisplayName => "ふわっち";
        protected override SiteType SiteType => SiteType.Whowatch;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new TabPagePanel();
                panel.SetViewModel(new WhowatchSiteOptionsViewModel(_siteOptions));
                return new WhowatchOptionsTabPage(DisplayName, panel);
            }
        }

        public override ICommentProvider CreateCommentProvider()
        {
            return new WhowatchCommentProvider(_server, _siteOptions, _logger);
        }

        public override System.Windows.Controls.UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            var cp = commentProvider as WhowatchCommentProvider;
            Debug.Assert(cp != null);
            if (cp == null)
                return null;

            var vm = new CommentPostPanelViewModel(cp, _logger);
            var panel = new CommentPostPanel
            {
                //IsEnabled = false,
                DataContext = vm
            };
            return panel;
        }

        public override bool IsValidInput(string input)
        {
            return Tools.IsValidUrl(input);
        }
        protected virtual IWhowatchSiteOptions CreateWhowatchSiteOptions()
        {
            return new WhowatchSiteOptions();
        }
        public override void LoadOptions(string path, IIo io)
        {
            _siteOptions = CreateWhowatchSiteOptions();
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
        public override void LoadOptions(string rawOptions)
        {
            _siteOptions = new WhowatchSiteOptions();
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
        public override string GetSiteOptions()
        {
            return _siteOptions.Serialize();
        }
        protected virtual IDataServer CreateServer()
        {
            return new DataServer();
        }
        public WhowatchSiteContext(ILogger logger)
            : base(logger)
        {
            _server = CreateServer();
            _logger = logger;
        }
    }
}
