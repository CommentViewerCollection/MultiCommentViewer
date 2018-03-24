using System;
using Common;
using System.Windows.Threading;
using SitePlugin;
using System.Windows.Controls;
using System.Text.RegularExpressions;

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
    public class TwicasSiteContext : ISiteContext
    {
        public Guid Guid => new Guid("8649A30C-D9C8-4ADB-862D-E0DAAEA24CE2");

        public string DisplayName => "Twicas";

        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new TwicasOptionsPanel();
                panel.SetViewModel(new TwicasOptionsViewModel(_siteOptions));
                return new TwicasOptionsTabPage(DisplayName, panel);
            }
        }

        public ICommentProvider CreateCommentProvider()
        {
            return new TwicasCommentProvider(new TwicasServer(), _logger, _options, _siteOptions, _userStore);
        }

        public bool IsValidInput(string input)
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
        public void LoadOptions(string dir, IIo io)
        {
            _siteOptions = new TwicasSiteOptions();
        }

        public void SaveOptions(string dir, IIo io)
        {

        }

        public UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            return null;
        }

        private readonly ICommentOptions _options;
        private readonly ILogger _logger;
        private readonly IUserStore _userStore;
        public TwicasSiteContext(ICommentOptions options, ILogger logger, IUserStore userStore)
        {
            _options = options;
            _logger = logger;
            _userStore = userStore;
        }
    }
}
