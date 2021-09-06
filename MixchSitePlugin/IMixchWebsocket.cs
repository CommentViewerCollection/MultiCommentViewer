using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MixchSitePlugin
{
    interface IMixchWebsocket
    {
        event EventHandler<IPacket> Received;

        Task ReceiveAsync(string movieId, string userAgent, List<Cookie> cookies);
        Task SendAsync(IPacket packet);
        Task SendAsync(string s);
        void Disconnect();
    }
}
