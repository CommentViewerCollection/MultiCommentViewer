using SitePlugin;

namespace MixchSitePlugin
{
    internal class MixchMessageContext : IMessageContext
    {
        public SitePlugin.ISiteMessage Message { get; }

        public IMessageMetadata Metadata { get; }

        public IMessageMethods Methods { get; }
        public MixchMessageContext(IMixchMessage message, MessageMetadata metadata, IMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
    }
}
