using Mcv.PluginV2;

namespace ShowRoomSitePlugin
{
    internal class MessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public string? UserId { get; }
        public string? NewNickname { get; }

        public MessageContext(IShowRoomMessage message, string? userId, string? newNickname)
        {
            Message = message;
            UserId = userId;
            NewNickname = newNickname;
        }
    }
}
