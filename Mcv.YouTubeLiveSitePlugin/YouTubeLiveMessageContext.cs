using Mcv.PluginV2;
using System.Collections.Generic;

namespace Mcv.YouTubeLiveSitePlugin
{
    internal class YouTubeLiveMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }
        public string? UserId { get; }
        public IEnumerable<IMessagePart>? UsernameItems { get; }

        public YouTubeLiveMessageContext(IYouTubeLiveMessage message, string? userId, IEnumerable<IMessagePart>? usernameItems, string? newNickname, bool isInitialComment)
        {
            Message = message;
            UserId = userId;
            UsernameItems = usernameItems;
            NewNickname = newNickname;
            IsInitialComment = isInitialComment;
        }
    }
}
