using Common;
using SitePlugin;
using SitePluginCommon;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ShowRoomSitePlugin
{
    public class ShowRoomSiteContext : SiteContextBase
    {
        public override Guid Guid => new Guid("C64FBE36-029E-483D-AA56-F1906C42B43B");

        public override string DisplayName => "SHOWROOM";
        protected override SiteType SiteType => SiteType.ShowRoom;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new ShowRoomOptionsPanel();
                panel.SetViewModel(new ShowRoomSiteOptionsViewModel(_siteOptions));
                return new ShowRoomOptionsTabPage(DisplayName, panel);
            }
        }

        public override ICommentProvider CreateCommentProvider()
        {
            return new ShowRoomCommentProvider(_server, _logger, _options, _siteOptions, _userStoreManager)
            {
                SiteContextGuid = Guid,
            };
        }
        private ShowRoomSiteOptions _siteOptions;
        public override void LoadOptions(string path, IIo io)
        {
            _siteOptions = new ShowRoomSiteOptions();
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
            var liveId = Tools.ExtractLiveId(input);
            return !string.IsNullOrEmpty(liveId);
        }

        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            return null;
        }
        private readonly ICommentOptions _options;
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        public ShowRoomSiteContext(ICommentOptions options, IDataServer server, ILogger logger, IUserStoreManager userStoreManager)
            : base(options, userStoreManager, logger)
        {
            _options = options;
            _server = server;
            _logger = logger;
        }
    }
}
