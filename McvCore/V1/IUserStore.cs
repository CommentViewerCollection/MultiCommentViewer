using System;
using System.Collections.Generic;

namespace Mcv.Core.V1;

interface IUserStore
{
    event EventHandler<McvUser> UserAdded;
    void Load();
    McvUser GetUser(string userId);
    //void Update(IUser user);
    void Save();
    IEnumerable<McvUser> GetAllUsers();
}
