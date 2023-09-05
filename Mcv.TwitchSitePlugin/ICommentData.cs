namespace TwitchSitePlugin
{
    interface ICommentData
    {
        string Id { get; }
        string Emotes { get; }
        string Message { get; }
        string UserId { get; }
        string Username { get; }
        string DisplayName { get; }
        System.DateTime SentAt { get; }
    }
}