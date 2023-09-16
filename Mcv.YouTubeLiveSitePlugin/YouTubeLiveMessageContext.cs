using Mcv.PluginV2;

namespace Mcv.YouTubeLiveSitePlugin
{
    internal class YouTubeLiveMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }
        public string? UserId { get; }

        public YouTubeLiveMessageContext(IYouTubeLiveMessage message, string? userId, string? newNickname, bool isInitialComment)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
            IsInitialComment = isInitialComment;
        }
    }
}
