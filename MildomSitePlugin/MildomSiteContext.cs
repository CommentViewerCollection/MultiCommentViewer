using Common;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace MildomSitePlugin
{
    public class MildomSiteContext : SiteContextBase
    {
        public override Guid Guid => new Guid("DBBA654F-0A5D-41CC-8153-5DB2D5869BCF");

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
            return new MildomCommentProvider(_server, _logger, _options, _siteOptions, _userStoreManager)
            {
                SiteContextGuid = Guid,
            };
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
        public override bool IsValidInput(string input)
        {
            return Tools.IsValidRoomUrl(input);
        }
        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            return null;
        }
        private readonly ICommentOptions _options;
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        public MildomSiteContext(ICommentOptions options, IDataServer server, ILogger logger, IUserStoreManager userStoreManager)
            : base(options, userStoreManager, logger)
        {
            _options = options;
            _server = server;
            _logger = logger;
        }
    }
}
