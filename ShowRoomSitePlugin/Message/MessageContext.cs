using SitePlugin;

namespace ShowRoomSitePlugin
{
    internal class MessageContext : IMessageContext
    {
        public SitePlugin.ISiteMessage Message { get; }

        public IMessageMetadata Metadata { get; }

        public IMessageMethods Methods { get; }
        public MessageContext(IShowRoomMessage message, MessageMetadata metadata, IMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
    }
}
