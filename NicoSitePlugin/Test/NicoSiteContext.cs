using System;
using SitePlugin;
using System.Diagnostics;
using Common;
using System.Text.RegularExpressions;
namespace NicoSitePlugin.Old
{
    public class NicoSiteContext : ISiteContext
    {
        public Guid Guid => new Guid("5A477452-FF28-4977-9064-3A4BC7C63252");
        public string DisplayName => "ニコ生";

        public IOptionsTabPage TabPanel
        {
            get
            {
                var panel = new NicoOptionsPanel();
                panel.SetViewModel(new NicoSiteOptionsViewModel(_siteOptions));
                return new NicoOptionsTabPage(DisplayName, panel);
            }
        }

        public ICommentProvider CreateCommentProvider(ConnectionName connectionName)
        {
            //return new NicoCommentProvider(connectionName, _options, _siteOptions);
            return new Test2.NicoCommentProvider3(connectionName,_options,_siteOptions, new Test2.CommentProvider2(_siteOptions.ResNum), new DataSource(), _logger, _userStore);
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
        private NicoSiteOptions _siteOptions;
        private readonly IOptions _options;
        private readonly ILogger _logger;
        private readonly IUserStore _userStore;

        public NicoSiteContext(IOptions options, ILogger logger, IUserStore userStore)
        {
            _options = options;
            _logger = logger;
            _userStore = userStore;
        }
    }
}
