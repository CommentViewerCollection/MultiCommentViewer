using System;
using SitePlugin;
namespace Common
{
    public class UserStoreTest : IUserStore
    {
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
            throw new NotImplementedException();
        }

        public void Update(IUser user)
        {
            throw new NotImplementedException();
        }
    }
}
