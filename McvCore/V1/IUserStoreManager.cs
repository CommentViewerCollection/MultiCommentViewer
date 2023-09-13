using Mcv.PluginV2;
using System;
using System.Collections.Generic;

namespace Mcv.Core.V1;

interface IUserStoreManager
{
    event EventHandler<McvUser> UserAdded;

    McvUser GetUser(PluginId siteType, string userId);
    IEnumerable<McvUser> GetAllUsers(PluginId siteType);
    void SetUserStore(PluginId siteType, IUserStore userStore);
    //void Init(PluginId siteType);
    void Save();
    void Save(PluginId siteType);
}
