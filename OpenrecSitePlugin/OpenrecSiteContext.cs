using System;
using System.Linq;
using System.Text;
using SitePlugin;
using Common;
using System.Windows.Controls;
using System.Diagnostics;
using SitePluginCommon;

namespace OpenrecSitePlugin
{
    public class OpenrecSiteContext : SiteContextBase
    {
        public override Guid Guid => new Guid("F4434012-3E68-4DD9-B2A8-F2BD7D601723");

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
            return new CommentProvider(_options, _siteOptions, _logger, _userStoreManager)
            {
                SiteContextGuid = Guid,
            };
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
        private OpenrecSiteOptions _siteOptions;
        private ICommentOptions _options;
        private ILogger _logger;

        public OpenrecSiteContext(ICommentOptions options, ILogger logger, IUserStoreManager userStoreManager)
            : base(options,userStoreManager, logger)
        {
            _options = options;
            _logger = logger;
        }
    }
}
