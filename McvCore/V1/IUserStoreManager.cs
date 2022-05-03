using Mcv.PluginV2;
using System;
using System.Collections.Generic;

namespace McvCore.V1;

public interface IUserStoreManager
{
    event EventHandler<McvUser> UserAdded;

    McvUser GetUser(PluginId siteType, string userId);
    IEnumerable<McvUser> GetAllUsers(PluginId siteType);
    void SetUserStore(PluginId siteType, IUserStore userStore);
    void Init(PluginId siteType);
    void Save();
    void Save(PluginId siteType);
}
