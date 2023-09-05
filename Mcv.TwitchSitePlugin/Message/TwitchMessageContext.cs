using Mcv.PluginV2;

namespace TwitchSitePlugin
{
    internal class TwitchMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? NewNickname { get; }
        public string? UserId { get; }

        public TwitchMessageContext(ITwitchMessage message, string? userId, string? newNickname)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
        }
    }
}
