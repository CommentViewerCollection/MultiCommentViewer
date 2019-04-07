using Common;
using SitePlugin;
using System;
using System.Collections.Generic;

namespace TestSitePlugin
{
    public class TestUserStore : IUserStore
    {
        public event EventHandler<IUser> UserAdded;
        Dictionary<string, IUser> _dict = new Dictionary<string, IUser>();
        public IEnumerable<IUser> GetAllUsers()
        {
            return _dict.Values;
        }

        public IUser GetUser(string userId)
        {
            if(!_dict.TryGetValue(userId, out IUser user))
            {
                user = new UserTest(userId);
                _dict.Add(userId, user);
            }
            return user;
        }

        public void Init()
        {
        }

        public void Save()
        {
        }
    }
}
