using SitePlugin;

namespace TestSitePlugin
{
    class MessageContext : IMessageContext
    {
        public IMessage Message { get; }
        public IMessageMetadata Metadata { get; }
        public IMessageMethods Methods { get; }
        public MessageContext(IMessage message, IMessageMetadata metadata, IMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
    }
}
