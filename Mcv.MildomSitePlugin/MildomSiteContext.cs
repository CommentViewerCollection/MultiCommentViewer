using Mcv.PluginV2;
using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace MildomSitePlugin
{
    public class MildomSiteContext : SiteContextBase
    {
        public override string DisplayName => "Mildom";
        protected override SiteType SiteType => SiteType.Mildom;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new TabPagePanel();
                panel.SetViewModel(new MildomSiteOptionsViewModel(_siteOptions));
                return new MildomOptionsTabPage(DisplayName, panel);
            }
        }
        public override ICommentProvider CreateCommentProvider()
        {
            return new MildomCommentProvider(_server, _logger, _siteOptions);
        }
        private MildomSiteOptions _siteOptions;
        public override void LoadOptions(string path, IIo io)
        {
            _siteOptions = new MildomSiteOptions();
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
            _siteOptions = new MildomSiteOptions();
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
            return Tools.IsValidRoomUrl(input);
        }
        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            return null;
        }
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        public MildomSiteContext(IDataServer server, ILogger logger)
            : base(logger)
        {
            _server = server;
            _logger = logger;
        }
    }
}
