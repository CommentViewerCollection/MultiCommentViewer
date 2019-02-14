using System;
using System.Threading.Tasks;

namespace MirrativSitePlugin
{
    public interface IMessageProvider
    {
        event EventHandler<string> Received;
        event EventHandler Opened;
        Task SendAsync(string s);
        Task ReceiveAsync();
        void Disconnect();
    }
}
