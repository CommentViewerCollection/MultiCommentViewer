using Mcv.PluginV2;
using SitePlugin;
using System;
using System.Collections.Generic;

namespace McvCore.V1
{
    public class UserStoreManager : IUserStoreManager
    {
        public event EventHandler<IUser>? UserAdded;
        public IUser GetUser(SiteId siteType, string userId)
        {
            var userStore = _dict[siteType];
            var user = userStore.GetUser(userId);
            return user;
        }
        public IEnumerable<IUser> GetAllUsers(SiteId siteType)
        {
            var userStore = _dict[siteType];
            return userStore.GetAllUsers();
        }
        public void Save(SiteId siteType)
        {
            var userStore = _dict[siteType];
            userStore.Save();
        }
        public void SetUserStore(SiteId siteType, IUserStore userStore)
        {
            _dict.Add(siteType, userStore);
        }
        public void Init(SiteId siteType)
        {
            var userStore = _dict[siteType];
            userStore.UserAdded += (s, e) => UserAdded?.Invoke(s, e);
            userStore.Init();
        }
        Dictionary<SiteId, IUserStore> _dict = new Dictionary<SiteId, IUserStore>();
    }
}
