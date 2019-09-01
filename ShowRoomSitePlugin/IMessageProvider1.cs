using System;
using System.Threading.Tasks;

namespace ShowRoomSitePlugin
{
    /// <summary>
    /// あいうえお
    /// </summary>
    interface IMessageProvider
    {
        event EventHandler<IInternalMessage> Received;
        Task ReceiveAsync(string bcsvr_host, string bcsvr_key);
        void Disconnect();
        Task SendAsync(string message);
    }
}
