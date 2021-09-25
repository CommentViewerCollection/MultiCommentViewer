using System;
using System.Linq;
using System.Text;
using SitePlugin;
using Common;
using System.Windows.Controls;
using System.Diagnostics;
using SitePluginCommon;

namespace MixchSitePlugin
{
    public class MixchSiteContext : SiteContextBase
    {
        public override Guid Guid => new Guid("F4434012-3E68-4DD9-B2A8-F2BD7D601724");
        // TODO: Guidを自動生成する

        public override string DisplayName => "ミクチャ";

        protected override SiteType SiteType => SiteType.Mixch;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new MixchOptionsPanel();
                panel.SetViewModel(new MixchOptionsViewModel(_siteOptions));
                return new MixchOptionsTabPage(DisplayName, panel);
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
            var lCommentProvider = commentProvider as CommentProvider;
            Debug.Assert(lCommentProvider != null);
            if (lCommentProvider == null)
                return null;

            var vm = new CommentPostPanelViewModel(lCommentProvider, _logger);
            var panel = new CommentPostPanel
            {
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
            _siteOptions = new MixchSiteOptions();
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
        private MixchSiteOptions _siteOptions;
        private ICommentOptions _options;
        private ILogger _logger;

        public MixchSiteContext(ICommentOptions options, ILogger logger, IUserStoreManager userStoreManager)
            : base(options, userStoreManager, logger)
        {
            _options = options;
            _logger = logger;
        }
    }
}
