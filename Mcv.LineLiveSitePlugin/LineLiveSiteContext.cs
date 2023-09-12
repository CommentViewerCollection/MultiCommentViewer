using Mcv.PluginV2;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace LineLiveSitePlugin
{
    public class LineLiveSiteContext : SiteContextBase
    {
        public override string DisplayName => "LINELIVE";
        protected override SiteType SiteType => SiteType.LineLive;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new LineLiveOptionsPanel();
                panel.SetViewModel(new LineLiveSiteOptionsViewModel(_siteOptions));
                return new LineLiveOptionsTabPage(DisplayName, panel);
            }
        }

        public override ICommentProvider CreateCommentProvider()
        {
            return new LineLiveCommentProvider(_server, _logger, _siteOptions);
        }
        private LineLiveSiteOptions _siteOptions;
        public override void LoadOptions(string path, IIo io)
        {
            _siteOptions = new LineLiveSiteOptions();
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
            _siteOptions = new LineLiveSiteOptions();
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
            if (string.IsNullOrEmpty(input)) return false;
            //最低限チャンネルIDさえあればコメントを取れるようにした
            //https://live.line.me/channels/2354725/broadcast/8428420
            var b = Regex.IsMatch(input, "line\\.me/channels/\\d+");
            return b;
        }

        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            return null;
        }
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        public LineLiveSiteContext(IDataServer server, ILogger logger)
            : base(logger)
        {
            _server = server;
        }
    }
}
