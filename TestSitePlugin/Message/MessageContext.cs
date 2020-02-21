using SitePlugin;

namespace TestSitePlugin
{
    class MessageContext : IMessageContext
    {
        public ISiteMessage Message { get; }
        public IMessageMetadata Metadata { get; }
        public IMessageMethods Methods { get; }
        public MessageContext(ISiteMessage message, IMessageMetadata metadata, IMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
    }
}
