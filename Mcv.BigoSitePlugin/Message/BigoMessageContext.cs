using Mcv.PluginV2;
using System.Collections.Generic;

namespace BigoSitePlugin
{
    internal class BigoMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }
        public IEnumerable<IMessagePart>? UsernameItems { get; }

        public BigoMessageContext(IBigoMessage message, bool isInitialComment)
        {
            Message = message;
            IsInitialComment = isInitialComment;
        }
    }
}
