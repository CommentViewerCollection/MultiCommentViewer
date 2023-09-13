using Mcv.PluginV2;
using System.Collections.Generic;

namespace LineLiveSitePlugin
{
    internal class LineLiveMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }
        public IEnumerable<IMessagePart>? UsernameItems { get; }

        public LineLiveMessageContext(ILineLiveMessage message, bool isInitialComment)
        {
            Message = message;
            IsInitialComment = isInitialComment;
        }
    }
}
