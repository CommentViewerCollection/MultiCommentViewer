using System.Collections.Generic;
namespace Mcv.MainViewPlugin;
class UserStore
{
    private readonly Dictionary<string, MyUser> _dict = new();
    public MyUser GetUser(string userId)
    {
        if (_dict.TryGetValue(userId, out var user))
        {
            return user;
        }
        else
        {
            user = new MyUser(userId);
            _dict.Add(userId, user);
            return user;
        }
    }
}
