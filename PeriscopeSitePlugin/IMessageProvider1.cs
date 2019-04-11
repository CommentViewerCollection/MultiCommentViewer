using System;
using System.Threading.Tasks;

namespace PeriscopeSitePlugin
{
    /// <summary>
    /// あいうえお
    /// </summary>
    interface IMessageProvider
    {
        event EventHandler<IInternalMessage> Received;
        Task ReceiveAsync(string hostname, string accessToken, string roomId);
    }
}
