using SitePlugin;

namespace WhowatchSitePlugin
{
    internal class WhowatchMessageContext : IMessageContext
    {
        public SitePlugin.IMessage Message { get; }

        public IMessageMetadata Metadata { get; }

        public IMessageMethods Methods { get; }
        public WhowatchMessageContext(IWhowatchMessage message, IMessageMetadata metadata, IMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
    }
}
