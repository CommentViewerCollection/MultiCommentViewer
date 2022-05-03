using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Mcv.PluginV2;

namespace TwitchSitePlugin
{
    public class TwitchSiteContext : SiteContextBase
    {
        public override string DisplayName => "Twitch";

        protected override SiteType SiteType => SiteType.Twitch;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new TabPagePanel();
                panel.SetViewModel(new TwitchSiteOptionsViewModel(_siteOptions));
                return new TwitchOptionsTabPage(DisplayName, panel);
            }
        }

        public override ICommentProvider CreateCommentProvider()
        {
            return new TwitchCommentProvider(_server, _logger, _siteOptions);
        }
        private TwitchSiteOptions _siteOptions;
        public override void LoadOptions(string path, IIo io)
        {
            _siteOptions = new TwitchSiteOptions();
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
            _siteOptions = new TwitchSiteOptions();
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
            //チャンネル名だけ来られても他のサイトのものの可能性があるからfalse
            //"twitch.tv/"の後に文字列があったらtrueとする。
            var b = Regex.IsMatch(input, "twitch\\.tv/[a-zA-Z0-9_]+");
            return b;
        }

        public override UserControl? GetCommentPostPanel(ICommentProvider commentProvider)
        {
            var twitchCommentProvider = commentProvider as TwitchCommentProvider;
            Debug.Assert(twitchCommentProvider != null);
            if (twitchCommentProvider == null)
                return null;

            var vm = new CommentPostPanelViewModel(twitchCommentProvider, _logger);
            var panel = new CommentPostPanel
            {
                //IsEnabled = false,
                DataContext = vm
            };
            return panel;
        }
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        public TwitchSiteContext(IDataServer server, ILogger logger)
            : base(logger)
        {
            _server = server;
            _logger = logger;
        }
    }
}
