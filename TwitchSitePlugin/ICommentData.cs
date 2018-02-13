namespace TwitchSitePlugin
{
    interface ICommentData
    {
        string Emotes { get; }
        string Message { get; }
        string UserId { get; }
        string Username { get; }
    }
}