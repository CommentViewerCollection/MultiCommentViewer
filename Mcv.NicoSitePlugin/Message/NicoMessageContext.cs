using Mcv.PluginV2;
using System.Collections.Generic;

namespace NicoSitePlugin
{
    internal class NicoMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }
        public string? UserId { get; }
        public IEnumerable<IMessagePart>? UsernameItems { get; }

        public NicoMessageContext(INicoMessage message, string? userId, string? newNickname, bool isInitialComment)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
            IsInitialComment = isInitialComment;
        }
    }
}
