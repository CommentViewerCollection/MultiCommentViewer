namespace SitePlugin
{
    public interface IUserStore
    {
        void Init();
        IUser GetUser(string userId);
        void Update(IUser user);
    }

}
