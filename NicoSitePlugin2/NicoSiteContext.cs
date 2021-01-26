using Common;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
namespace NicoSitePlugin
{
    public class NicoSiteContext : SiteContextBase
    {
        public override Guid Guid => new Guid("5A477452-FF28-4977-9064-3A4BC7C63252");
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
            return new TestCommentProvider(_options, _siteOptions, _server, _logger, _userStoreManager)
            {
                SiteContextGuid = Guid,
            };
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
        protected virtual IUserStore CreateUserStore()
        {
            return new SQLiteUserStore(_options.SettingsDirPath + "\\" + "users_" + DisplayName + ".db", _logger);
        }
        private NicoSiteOptions _siteOptions;
        private readonly ICommentOptions _options;
        private readonly IDataSource _server;
        private readonly ILogger _logger;

        public NicoSiteContext(ICommentOptions options, IDataSource server, ILogger logger, IUserStoreManager userStoreManager)
            : base(options, userStoreManager, logger)
        {
            _options = options;
            _server = server;
            _logger = logger;
        }
    }
}
