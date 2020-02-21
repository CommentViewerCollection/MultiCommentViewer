using SitePlugin;

namespace MixerSitePlugin
{
    internal class MixerMessageContext : IMessageContext
    {
        public SitePlugin.ISiteMessage Message { get; }

        public IMessageMetadata Metadata { get; }

        public IMessageMethods Methods { get; }
        public MixerMessageContext(IMixerMessage message, IMixerMessageMetadata metadata, IMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
    }
}
