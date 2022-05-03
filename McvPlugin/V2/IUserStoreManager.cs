using System;
using System.Collections.Generic;

namespace Mcv.PluginV2;

public interface IUserStoreManager
{
    event EventHandler<McvUser> UserAdded;

    McvUser GetUser(SiteType siteType, string userId);
    IEnumerable<McvUser> GetAllUsers(SiteType siteType);
    void SetUserStore(SiteType siteType, IUserStore userStore);
    void Init(SiteType siteType);
    void Save(SiteType siteType);
}
