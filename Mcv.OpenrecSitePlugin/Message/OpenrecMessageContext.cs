using Mcv.PluginV2;

namespace OpenrecSitePlugin
{
    internal class OpenrecMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }

        public OpenrecMessageContext(IOpenrecMessage message, string? userId, string? newNickname, bool isInitialComment)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
            IsInitialComment = isInitialComment;
        }
    }
}
