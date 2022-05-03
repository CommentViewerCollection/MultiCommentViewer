using Mcv.PluginV2;

namespace NicoSitePlugin
{
    internal class NicoMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? NewNickname { get; }
        public string? UserId { get; }

        public NicoMessageContext(INicoMessage message, string? userId, string? newNickname)
        {
            Message = message;
            NewNickname = newNickname;
        }
    }
}
