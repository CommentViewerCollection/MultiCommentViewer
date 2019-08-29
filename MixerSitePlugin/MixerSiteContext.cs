using Common;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace MixerSitePlugin
{
    public class MixerSiteContext : SiteContextBase
    {
        public override Guid Guid => new Guid("2F5A99B2-8861-467D-B030-9820811967BE");

        public override string DisplayName => "Mixer";
        protected override SiteType SiteType => SiteType.Mixer;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new TabPagePanel();
                panel.SetViewModel(new MixerSiteOptionsViewModel(_siteOptions));
                return new MixerOptionsTabPage(DisplayName, panel);
            }
        }

        public override ICommentProvider CreateCommentProvider()
        {
            return new MixerCommentProvider(_server, _logger, _options, _siteOptions, _userStoreManager)
            {
                SiteContextGuid = Guid,
            };
        }
        private MixerSiteOptions _siteOptions;
        public override void LoadOptions(string path, IIo io)
        {
            _siteOptions = new MixerSiteOptions();
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
            return Tools.IsValidLiveId(input) || Tools.IsValidUserId(input);
        }
        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            return null;
        }
        private readonly ICommentOptions _options;
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        public MixerSiteContext(ICommentOptions options, IDataServer server, ILogger logger, IUserStoreManager userStoreManager)
            : base(options, userStoreManager, logger)
        {
            _options = options;
            _server = server;
            _logger = logger;
        }
    }
}
