using System;
using System.Windows.Controls;
using System.Diagnostics;
using Mcv.PluginV2;

namespace OpenrecSitePlugin
{
    public class OpenrecSiteContext : SiteContextBase
    {
        public override string DisplayName => "OPENREC";
        protected override SiteType SiteType => SiteType.Openrec;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new OpenrecOptionsPanel();
                panel.SetViewModel(new OpenrecOptionsViewModel(_siteOptions));
                return new OpenrecOptionsTabPage(DisplayName, panel);
            }
        }

        public override ICommentProvider CreateCommentProvider()
        {
            return new CommentProvider(_siteOptions, _logger);
        }
        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            var nicoCommentProvider = commentProvider as CommentProvider;
            Debug.Assert(nicoCommentProvider != null);
            if (nicoCommentProvider == null)
                return null;

            var vm = new CommentPostPanelViewModel(nicoCommentProvider, _logger);
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

        public override void LoadOptions(string path, IIo io)
        {
            _siteOptions = new OpenrecSiteOptions();
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
                _logger.LogException(ex, "", path);
            }
        }
        public override void LoadOptions(string rawOptions)
        {
            _siteOptions = new OpenrecSiteOptions();
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
        private OpenrecSiteOptions _siteOptions;
        private ILogger _logger;

        public OpenrecSiteContext(ILogger logger)
            : base(logger)
        {
            _logger = logger;
        }
    }
}
