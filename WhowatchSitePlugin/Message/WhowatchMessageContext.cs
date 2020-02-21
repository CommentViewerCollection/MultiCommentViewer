using SitePlugin;

namespace WhowatchSitePlugin
{
    internal class WhowatchMessageContext : IMessageContext
    {
        public SitePlugin.ISiteMessage Message { get; }

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
