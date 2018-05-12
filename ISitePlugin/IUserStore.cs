using System;
using System.Collections.Generic;

namespace SitePlugin
{
    public interface IUserStore
    {
        event EventHandler<IUser> UserAdded;
        void Init();
        IUser GetUser(string userId);
        //void Update(IUser user);
        void Save();
        IEnumerable<IUser> GetAllUsers();
    }

}
