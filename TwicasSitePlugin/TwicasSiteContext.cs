using System;
using System.Windows.Controls;
using System.Diagnostics;
using Mcv.PluginV2;

namespace TwicasSitePlugin
{
    public class TwicasOptionsTabPage : IOptionsTabPage
    {
        public string HeaderText { get; }

        public System.Windows.Controls.UserControl TabPagePanel => _panel;

        public void Apply()
        {
            var optionsVm = _panel.GetViewModel();
            optionsVm.OriginOptions.Set(optionsVm.ChangedOptions);
        }

        public void Cancel()
        {
        }
        private readonly TwicasOptionsPanel _panel;
        public TwicasOptionsTabPage(string displayName, TwicasOptionsPanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
    public class TwicasSiteContext : SiteContextBase
    {
        public override string DisplayName => "ツイキャス";
        protected override SiteType SiteType => SiteType.Twicas;
        public override IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new TwicasOptionsPanel();
                panel.SetViewModel(new TwicasSiteOptionsViewModel(_siteOptions));
                return new TwicasOptionsTabPage(DisplayName, panel);
            }
        }

        public override ICommentProvider CreateCommentProvider()
        {
            return new TwicasCommentProvider2(new TwicasServer(), _logger, _siteOptions);
        }

        public override bool IsValidInput(string input)
        {
            return Tools.IsValidUrl(input);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="isIdOk">URLだけでなく、IDのみでもtrueを返すか</param>
        /// <returns></returns>
        public bool IsValidInput(string input, bool isIdOk)
        {
            if (!isIdOk)
            {
                return Tools.IsValidUrl(input);
            }
            else
            {
                return Tools.IsValidUrl(input) || Tools.IsValidUserId(input);
            }
        }

        private TwicasSiteOptions _siteOptions;
        public override void LoadOptions(string path, IIo io)
        {
            _siteOptions = new TwicasSiteOptions();
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
            _siteOptions = new TwicasSiteOptions();
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
        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            var youtubeCommentProvider = commentProvider as TwicasCommentProvider2;
            Debug.Assert(youtubeCommentProvider != null);
            if (youtubeCommentProvider == null)
                return null;

            var vm = new CommentPostPanelViewModel(youtubeCommentProvider, _logger);
            var panel = new CommentPostPanel
            {
                //IsEnabled = false,
                DataContext = vm
            };
            return panel;
        }
        private readonly ILogger _logger;
        public TwicasSiteContext(ILogger logger)
            : base(logger)
        {
            _logger = logger;
        }
    }
}
