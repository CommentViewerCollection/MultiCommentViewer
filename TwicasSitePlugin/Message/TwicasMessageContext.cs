using Mcv.PluginV2;

namespace TwicasSitePlugin
{
    internal class TwicasMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }

        public TwicasMessageContext(ITwicasMessage message, string? userId, string? newNickname)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
        }
    }
}
