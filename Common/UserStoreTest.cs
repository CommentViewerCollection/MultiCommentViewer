using System;
using System.Collections.Generic;
using SitePlugin;
namespace Common
{
    public class UserStoreTest : IUserStore
    {
        public event EventHandler<IUser> UserAdded;
        System.Collections.Concurrent.ConcurrentDictionary<string, IUser> _dict = new System.Collections.Concurrent.ConcurrentDictionary<string, IUser>();
        public IUser GetUser(string userid)
        {
            if (!_dict.TryGetValue(userid, out IUser user))
            {
                user = new UserTest(userid);
                _dict.AddOrUpdate(userid, user, (_, u) => u);
            }
            return user;
        }
        
        public void Init()
        {
        }

        public void Update(IUser user)
        {
            throw new NotImplementedException();
        }
        public void Save()
        {

        }

        public IEnumerable<IUser> GetAllUsers()
        {
            return new List<IUser>();
        }
    }
}
