using System;
using System.Collections.Generic;

namespace Mcv.PluginV2;

public interface IUserStore
{
    event EventHandler<McvUser> UserAdded;
    void Init();
    McvUser GetUser(string userId);
    //void Update(IUser user);
    void Save();
    IEnumerable<McvUser> GetAllUsers();
}
