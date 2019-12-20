using System;
using System.Threading.Tasks;

namespace SitePluginCommon.AutoReconnector
{
    [Obsolete]
    public interface IConnector
    {
        Task<bool> IsLivingAsync();
        Task<DisconnectReason> ConnectAsync();
        void Disconnect();
    }
}
