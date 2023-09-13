using Mcv.PluginV2;
using System.Collections.Generic;

namespace TwitchSitePlugin
{
    internal class TwitchMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }
        public string? UserId { get; }
        public IEnumerable<IMessagePart>? UsernameItems { get; }

        public TwitchMessageContext(ITwitchMessage message, string? userId, IEnumerable<IMessagePart>? usernameItems, string? newNickname, bool isInitialComment)
        {
            Message = message;
            UserId = userId;
            UsernameItems = usernameItems;
            NewNickname = newNickname;
            IsInitialComment = isInitialComment;
        }
    }
}
