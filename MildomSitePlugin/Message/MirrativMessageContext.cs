using Mcv.PluginV2;

namespace MildomSitePlugin
{
    internal class MildomMessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }

        public MildomMessageContext(IMildomMessage message, string? userId, string? newNickname)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
        }
    }
}
