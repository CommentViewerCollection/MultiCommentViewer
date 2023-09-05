using Mcv.PluginV2;

namespace OpenrecSitePlugin
{
    internal class OpenrecMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }

        public OpenrecMessageContext(IOpenrecMessage message, string? userId, string? newNickname)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
        }
    }
}
