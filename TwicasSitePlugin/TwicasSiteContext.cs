using System;
using Common;
using System.Windows.Threading;
using SitePlugin;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Diagnostics;

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
                panel.SetViewModel(new TwicasSiteOptionsViewModel(_siteOptions));
                return new TwicasOptionsTabPage(DisplayName, panel);
            }
        }

        public ICommentProvider CreateCommentProvider()
        {
            return new TwicasCommentProvider(new TwicasServer(), _logger, _options, _siteOptions, _userStore)
            {
                SiteContextGuid = Guid,
            };
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
        public void LoadOptions(string path, IIo io)
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
                _logger.LogException(ex, "", $"path={path}");
            }
        }
        public IUser GetUser(string userId)
        {
            return _userStore.GetUser(userId);
        }
        public void Init()
        {
            _userStore.Init();
        }
        public void Save()
        {
            _userStore.Save();
        }
        public UserControl GetCommentPostPanel(ICommentProvider commentProvider)
        {
            var youtubeCommentProvider = commentProvider as TwicasCommentProvider;
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
        protected virtual IUserStore CreateUserStore()
        {
            return new SQLiteUserStore(_options.SettingsDirPath + "\\" + "users_" + DisplayName + ".db", _logger);
        }
        private readonly ICommentOptions _options;
        private readonly ILogger _logger;
        private readonly IUserStore _userStore;
        public TwicasSiteContext(ICommentOptions options, ILogger logger)
        {
            _options = options;
            _logger = logger;
            _userStore = CreateUserStore();
        }
    }
}
