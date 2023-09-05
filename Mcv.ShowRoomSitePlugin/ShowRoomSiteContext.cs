using Mcv.PluginV2;
using System;
using System.Diagnostics;
using System.Windows.Controls;

namespace ShowRoomSitePlugin
{
    public class ShowRoomSiteContext : SiteContextBase
    {
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
            return new ShowRoomCommentProvider(_server, _logger, _siteOptions);
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
        public override void LoadOptions(string rawOptions)
        {
            _siteOptions = new ShowRoomSiteOptions();
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
            var liveId = Tools.ExtractLiveId(input);
            return !string.IsNullOrEmpty(liveId);
        }

        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            return null;
        }
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        public ShowRoomSiteContext(IDataServer server, ILogger logger)
            : base(logger)
        {
            _server = server;
            _logger = logger;
        }
    }
}
