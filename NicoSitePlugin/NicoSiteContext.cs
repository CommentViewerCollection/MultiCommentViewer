using Mcv.PluginV2;
using System;
using System.Diagnostics;
using System.Windows.Controls;
namespace NicoSitePlugin
{
    public class NicoSiteContext : SiteContextBase
    {
        public override string DisplayName => "ニコ生";
        protected override SiteType SiteType => SiteType.NicoLive;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new NicoOptionsPanel();
                panel.SetViewModel(new NicoSiteOptionsViewModel(_siteOptions));
                return new NicoOptionsTabPage(DisplayName, panel);
            }
        }
        public override ICommentProvider CreateCommentProvider()
        {
            return new TestCommentProvider(_siteOptions, _server, _logger);
        }
        public override void LoadOptions(string path, IIo io)
        {
            _siteOptions = new NicoSiteOptions();
            try
            {
                var s = io.ReadFile(path);

                _siteOptions.Deserialize(s);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogException(ex, "", path);
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
            _siteOptions = new NicoSiteOptions();
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
        public override bool IsValidInput(string input)
        {
            return Tools.IsLivePageUrl(input) || Tools.IsChannelUrl(input) || Tools.IsCommunityUrl(input);
        }

        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            var nicoCommentProvider = commentProvider as INicoCommentProvider;
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
        private NicoSiteOptions _siteOptions;
        private readonly IDataSource _server;
        private readonly ILogger _logger;

        public NicoSiteContext(IDataSource server, ILogger logger)
            : base(logger)
        {
            _server = server;
            _logger = logger;
        }
    }
}
