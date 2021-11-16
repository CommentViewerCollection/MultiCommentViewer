using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MixchSitePlugin
{
    interface IMixchWebsocket
    {
        event EventHandler<Packet> Received;

        Task ReceiveAsync(LiveUrlInfo liveUrlInfo, string userAgent, List<Cookie> cookies);
        Task SendAsync(string s);
        void Disconnect();
    }
}
