using System;
using System.Collections.Generic;
using Common;
using SitePlugin;
namespace YouTubeLiveCommentViewer
{
    public class UserStoreTest : IUserStore
    {
        System.Collections.Concurrent.ConcurrentDictionary<string, IUser> _dict = new System.Collections.Concurrent.ConcurrentDictionary<string, IUser>();

        public event EventHandler<IUser> UserAdded;

        public IEnumerable<IUser> GetAllUsers()
        {
            throw new NotImplementedException();
        }

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

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Update(IUser user)
        {
            throw new NotImplementedException();
        }
    }
}
