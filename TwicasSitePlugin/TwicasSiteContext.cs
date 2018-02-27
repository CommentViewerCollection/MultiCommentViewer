using System;
using Common;
using System.Windows.Threading;
using SitePlugin;
using System.Windows.Controls;

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
        private readonly TabPagePanel _panel;
        public TwicasOptionsTabPage(string displayName, TabPagePanel panel)
        {
            HeaderText = displayName;
            _panel = panel;
        }
    }
    public class TwicasSiteContext : ISiteContext
    {
        public Guid Guid => new Guid("8649A30C-D9C8-4ADB-862D-E0DAAEA24CE2");

        public string DisplayName => "Twicas";

        public IOptionsTabPage TabPanel => throw new NotImplementedException();

        public ICommentProvider CreateCommentProvider()
        {
            return new TwicasCommentProvider(new TwicasServer(), _logger, _options, _siteOptions, _userStore, _dispatcher);
        }

        public bool IsValidInput(string input)
        {
            var broadcasterId = Tools.ExtractBroadcasterId(input);
            return !string.IsNullOrEmpty(broadcasterId);
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
        private readonly Dispatcher _dispatcher;
        public TwicasSiteContext(ICommentOptions options, ILogger logger, IUserStore userStore, Dispatcher dispatcher)
        {
            _options = options;
            _logger = logger;
            _userStore = userStore;
            _dispatcher = dispatcher;
        }
    }
}
