using System;
using System.Windows.Controls;
using System.Diagnostics;
using Mcv.PluginV2;

namespace MixchSitePlugin
{
    public class MixchSiteContext : SiteContextBase
    {
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
            return new CommentProvider(_siteOptions, _logger);
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
        public override void LoadOptions(string rawOptions)
        {
            _siteOptions = new MixchSiteOptions();
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
        private MixchSiteOptions _siteOptions;
        private ILogger _logger;

        public MixchSiteContext(ILogger logger)
            : base(logger)
        {
            _logger = logger;
        }
    }
}
