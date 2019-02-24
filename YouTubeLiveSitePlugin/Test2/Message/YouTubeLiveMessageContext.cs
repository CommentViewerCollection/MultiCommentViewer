using SitePlugin;

namespace YouTubeLiveSitePlugin.Test2
{
    internal class YouTubeLiveMessageContext : IMessageContext
    {
        public SitePlugin.IMessage Message { get; }

        public IMessageMetadata Metadata { get; }

        public IMessageMethods Methods { get; }
        public YouTubeLiveMessageContext(IYouTubeLiveMessage message, YouTubeLiveMessageMetadata metadata, IMessageMethods methods)
        {
            Message = message;
            Metadata = metadata;
            Methods = methods;
        }
    }
}
