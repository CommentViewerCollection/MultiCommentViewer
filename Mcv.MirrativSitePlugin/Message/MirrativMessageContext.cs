using Mcv.PluginV2;

namespace MirrativSitePlugin
{
    internal class MirrativMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }

        public MirrativMessageContext(IMirrativMessage message, string? userId, string? newNickname, bool isInitialComment)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
            IsInitialComment = isInitialComment;
        }
    }
}
