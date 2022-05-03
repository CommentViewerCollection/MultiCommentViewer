using Mcv.PluginV2;

namespace WhowatchSitePlugin
{
    internal class WhowatchMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }

        public WhowatchMessageContext(IWhowatchMessage message, string? userId, string? newNickname)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
        }
    }
}
