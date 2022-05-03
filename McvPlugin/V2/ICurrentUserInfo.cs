namespace Mcv.PluginV2;

/// <summary>
/// 
/// </summary>
public interface ICurrentUserInfo
{
    string Username { get; }
    //string UserId { get; }
    bool IsLoggedIn { get; }
}
