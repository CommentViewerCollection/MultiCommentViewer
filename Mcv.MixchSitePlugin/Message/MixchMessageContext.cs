using Mcv.PluginV2;

namespace MixchSitePlugin
{
    internal class MixchMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }

        public MixchMessageContext(IMixchMessage message, string? userId, string? newNickname, bool isInitialComment)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
            IsInitialComment = isInitialComment;
        }
    }
}
