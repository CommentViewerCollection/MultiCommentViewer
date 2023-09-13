using Mcv.PluginV2;
using System.Collections.Generic;

namespace TwicasSitePlugin
{
    internal class TwicasMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }
        public IEnumerable<IMessagePart>? UsernameItems { get; }

        public TwicasMessageContext(ITwicasMessage message, string? userId, string? newNickname, bool isInitialComment)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
            IsInitialComment = isInitialComment;
        }
    }
}
