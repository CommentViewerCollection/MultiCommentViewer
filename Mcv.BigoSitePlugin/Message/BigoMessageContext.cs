using Mcv.PluginV2;

namespace BigoSitePlugin
{
    internal class BigoMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }

        public BigoMessageContext(IBigoMessage message, bool isInitialComment)
        {
            Message = message;
            IsInitialComment = isInitialComment;
        }
    }
}
