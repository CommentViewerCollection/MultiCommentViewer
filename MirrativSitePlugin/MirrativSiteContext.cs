using Mcv.PluginV2;
using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace MirrativSitePlugin
{
    public class MirrativSiteContext : SiteContextBase
    {
        public override string DisplayName => "Mirrativ";
        protected override SiteType SiteType => SiteType.Mirrativ;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new TabPagePanel();
                panel.SetViewModel(new MirrativSiteOptionsViewModel(_siteOptions));
                return new MirrativOptionsTabPage(DisplayName, panel);
            }
        }

        public override ICommentProvider CreateCommentProvider()
        {
            return new MirrativCommentProvider2(_server, _logger, _siteOptions);
        }
        private MirrativSiteOptions _siteOptions;
        public override void LoadOptions(string path, IIo io)
        {
            _siteOptions = new MirrativSiteOptions();
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
            _siteOptions = new MirrativSiteOptions();
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
            return Tools.IsValidLiveId(input) || Tools.IsValidUserId(input);
        }
        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            return null;
        }
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        public MirrativSiteContext(IDataServer server, ILogger logger)
            : base(logger)
        {
            _server = server;
            _logger = logger;
        }
    }
}
