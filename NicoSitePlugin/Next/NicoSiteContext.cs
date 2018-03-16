using Common;
using SitePlugin;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace NicoSitePlugin.Next
{
    public class NicoSiteContext : ISiteContext
    {
        public Guid Guid => new Guid("5A477452-FF28-4977-9064-3A4BC7C63252");
        public string DisplayName => "ニコ生Next";

        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new NicoOptionsPanel();
                panel.SetViewModel(new NicoSiteOptionsViewModel(_siteOptions));
                return new NicoOptionsTabPage(DisplayName, panel);
            }
        }

        public ICommentProvider CreateCommentProvider()
        {
            return new NicoCommentProvider(_options, _siteOptions, new DataSource(), _logger, _userStore);
        }
        private string GetOptionsPath(string dir)
        {
            return System.IO.Path.Combine(dir, DisplayName + ".txt");
        }
        public void LoadOptions(string path, IIo io)
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

        public void SaveOptions(string path, IIo io)
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

        public bool IsValidInput(string input)
        {
            var b = Regex.IsMatch(input, "lv\\d+");
            return b;
        }

        public UserControl GetCommentPostPanel(ICommentProvider commentProvider)
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

        private NicoSiteOptions _siteOptions;
        private readonly ICommentOptions _options;
        private readonly ILogger _logger;
        private readonly IUserStore _userStore;

        public NicoSiteContext(ICommentOptions options, ILogger logger, IUserStore userStore)
        {
            _options = options;
            _logger = logger;
            _userStore = userStore;
        }
    }
}
