using System;
using System.Threading.Tasks;

namespace TwitchSitePlugin
{
    interface IMetadataProvider
    {
        event EventHandler<Stream> MetadataUpdated;

        void Disconnect();
        Task ReceiveAsync();
    }
}