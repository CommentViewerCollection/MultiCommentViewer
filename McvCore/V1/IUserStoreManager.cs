using Mcv.PluginV2;
using SitePlugin;
using System;
using System.Collections.Generic;

namespace McvCore.V1
{
    public interface IUserStoreManager
    {
        event EventHandler<IUser> UserAdded;

        IUser GetUser(SiteId siteType, string userId);
        IEnumerable<IUser> GetAllUsers(SiteId siteType);
        void SetUserStore(SiteId siteType, IUserStore userStore);
        void Init(SiteId siteType);
        void Save(SiteId siteType);
    }
}
