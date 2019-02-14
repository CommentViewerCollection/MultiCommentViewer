using System;
using System.Threading.Tasks;

namespace WhowatchSitePlugin
{
    public interface IWebsocket
    {
        event EventHandler<string> Received;
        event EventHandler Opened;
        Task SendAsync(string s);
        Task ReceiveAsync();
        void Disconnect();
    }
}
