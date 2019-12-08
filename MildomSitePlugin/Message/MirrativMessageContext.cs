using SitePlugin;

namespace MildomSitePlugin
{
    internal class MildomMessageContext : IMessageContext
    {
        public SitePlugin.IMessage Message { get; }

        public IMessageMetadata Metadata { get; }

        public IMessageMethods Methods { get; }
        public MildomMessageContext(IMildomMessage message, IMildomMessageMetadata metadata, IMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
    }
}
