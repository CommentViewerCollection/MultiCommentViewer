using Mcv.PluginV2;

namespace NicoSitePlugin
{
    internal class NicoMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? NewNickname { get; }
        public bool IsInitialComment { get; }
        public string? UserId { get; }

        public NicoMessageContext(INicoMessage message, string? userId, string? newNickname, bool isInitialComment)
        {
            Message = message;
            NewNickname = newNickname;
            IsInitialComment = isInitialComment;
        }
    }
}
