using Mcv.PluginV2;

namespace BigoSitePlugin
{
    internal class BigoMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }

        public BigoMessageContext(IBigoMessage message)
        {
            Message = message;
        }
    }
}
