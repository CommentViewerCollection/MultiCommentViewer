using SitePlugin;

namespace TwicasSitePlugin
{
    internal class TwicasMessageContext : IMessageContext
    {
        public SitePlugin.ISiteMessage Message { get; }

        public IMessageMetadata Metadata { get; }

        public IMessageMethods Methods { get; }
        public TwicasMessageContext(ITwicasMessage message, MessageMetadata metadata, IMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
    }
}
