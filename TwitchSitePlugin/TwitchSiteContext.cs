using System;
using System.Linq;
using System.Text;
using SitePlugin;
using System.Diagnostics;
using System.Windows.Threading;
using Common;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using SitePluginCommon;

namespace TwitchSitePlugin
{
    public abstract class SiteContextBase : ISiteContext
    {
        protected readonly IUserStoreManager _userStoreManager;
        private readonly ILogger _logger;

        protected abstract SiteType SiteType { get; }
        public abstract Guid Guid { get; }
        public abstract string DisplayName { get; }
        public abstract IOptionsTabPage TabPanel { get; }

        protected ICommentOptions Options { get; }

        public abstract ICommentProvider CreateCommentProvider();

        public abstract UserControl GetCommentPostPanel(ICommentProvider commentProvider);

        public IUser GetUser(string userId)
        {
            return _userStoreManager.GetUser(SiteType, userId);
        }

        public virtual void Init()
        {
            _userStoreManager.Init(SiteType);
        }

        public abstract bool IsValidInput(string input);

        public abstract void LoadOptions(string path, IIo io);

        public virtual void Save()
        {
            _userStoreManager.Save(SiteType);
        }

        public abstract void SaveOptions(string path, IIo io);
        public SiteContextBase(ICommentOptions options, IUserStoreManager userStoreManager, ILogger logger)
        {
            Options = options;
            _userStoreManager = userStoreManager;
            _logger = logger;
            var userStore = new SQLiteUserStore(options.SettingsDirPath + "\\" + "users_" + DisplayName + ".db", logger);
            userStoreManager.SetUserStore(SiteType, userStore);
        }
    }
    public class TwitchSiteContext : SiteContextBase
    {
        public override Guid Guid => new Guid("22F7824A-EA1B-411E-85CA-6C9E6BE94E39");

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
            return new TwitchCommentProvider(_server, _logger, _options, _siteOptions, _userStoreManager)
            {
                SiteContextGuid = Guid,
            };
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
        public override void Init()
        {
            base.Init();
        }
        public override void Save()
        {
            base.Save();
        }
        public override bool IsValidInput(string input)
        {
            //チャンネル名だけ来られても他のサイトのものの可能性があるからfalse
            //"twitch.tv/"の後に文字列があったらtrueとする。
            var b = Regex.IsMatch(input, "twitch\\.tv/[a-zA-Z0-9_]+");
            return b;
        }

        public override UserControl GetCommentPostPanel(ICommentProvider commentProvider)
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
        private readonly ICommentOptions _options;
        private readonly IDataServer _server;
        private readonly ILogger _logger;
        public TwitchSiteContext(ICommentOptions options, IDataServer server, ILogger logger, IUserStoreManager userStoreManager)
            : base(options, userStoreManager, logger)
        {
            _options = options;
            _server = server;
            _logger = logger;
        }
    }
}
