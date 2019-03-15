using System;
using System.Threading.Tasks;

namespace MirrativSitePlugin
{
    public interface IWebSocket
    {
        event EventHandler<string> Received;
        event EventHandler Opened;
        Task SendAsync(string s);
        Task ReceiveAsync();
        void Disconnect();
    }
}
