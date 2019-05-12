using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitePluginCommon
{
    public interface IUserStoreManager
    {
        event EventHandler<IUser> UserAdded;

        IUser GetUser(SiteType siteType, string userId);
        IEnumerable<IUser> GetAllUsers(SiteType siteType);
        void SetUserStore(SiteType siteType, IUserStore userStore);
        void Init(SiteType siteType);
        void Save(SiteType siteType);
    }

    public class UserStoreManager : IUserStoreManager
    {
        public event EventHandler<IUser> UserAdded;
        public IUser GetUser(SiteType siteType, string userId)
        {
            var userStore = _dict[siteType];
            var user = userStore.GetUser(userId);
            return user;
        }
        public IEnumerable<IUser> GetAllUsers(SiteType siteType)
        {
            var userStore = _dict[siteType];
            return userStore.GetAllUsers();
        }
        public void Save(SiteType siteType)
        {
            var userStore = _dict[siteType];
            userStore.Save();
        }
        public void SetUserStore(SiteType siteType, IUserStore userStore)
        {
            _dict.Add(siteType, userStore);
        }
        public void Init(SiteType siteType)
        {
            var userStore = _dict[siteType];
            userStore.UserAdded += (s, e) => UserAdded?.Invoke(s, e);
            userStore.Init();
        }
        Dictionary<SiteType, IUserStore> _dict = new Dictionary<SiteType, IUserStore>();
    }
}
