using System;
using System.Threading.Tasks;

namespace MildomSitePlugin
{
    public interface IWebSocket
    {
        event EventHandler<string> Received;
        event EventHandler<byte[]> BinaryReceived;
        event EventHandler Opened;
        Task SendAsync(string s);
        Task SendAsync(byte[] b);
        Task ReceiveAsync();
        void Disconnect();
    }
}
