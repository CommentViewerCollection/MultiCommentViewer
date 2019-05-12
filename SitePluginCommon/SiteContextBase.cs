using System;
using SitePlugin;
using Common;
using System.Windows.Controls;

namespace SitePluginCommon
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
}
